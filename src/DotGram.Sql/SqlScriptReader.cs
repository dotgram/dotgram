using System;
using System.Collections.Generic;
using System.Text;

#if NET8_0_OR_GREATER
using System.Buffers;
#endif

namespace DotGram.Sql;

/// <summary>
/// One reading of a script: a forward scan that cuts it into the batches a client tool sends.
/// </summary>
/// <remarks>
/// <para>
/// <b>Not a grammar, on purpose.</b> What sqlcmd does is not a language's syntax but a tool's
/// input loop: a variable store that a later <c>GO $(n)</c> reads, a lexical state that runs on
/// through an included file into the rest of the file that included it, a line that is the
/// tool's or the server's according to how it starts. So the statement of what it does is not
/// this code either but the table of sqlcmd's own answers it is tested against, one script a row,
/// compared byte for byte with what sqlcmd sent.
/// </para>
/// <para>
/// Five states, as sqlcmd has them: ordinary text, a string, a quoted identifier, a bracketed
/// one and a block comment — which does not nest, whatever the server makes of it. A line comment
/// is the rest of its line. Only a few characters change anything, so the scan jumps from one to
/// the next; and only at the start of a line, in ordinary text, is a line asked whether it is the
/// tool's: a separator, or a command.
/// </para>
/// </remarks>
sealed class SqlScriptReader
{
	enum State
	{
		Text,
		String,
		Quoted,
		Bracketed,
		Comment,
	}

	// What stops the scan in each state. `$` only where variables are substituted; nothing in a
	// comment but its close, since nothing in one is substituted.
#if NET8_0_OR_GREATER
	static readonly SearchValues<char> TextStops      = SearchValues.Create("'\"[-/$");
	static readonly SearchValues<char> StringStops    = SearchValues.Create("'$");
	static readonly SearchValues<char> QuotedStops    = SearchValues.Create("\"$");
	static readonly SearchValues<char> BracketedStops = SearchValues.Create("]$");
	static readonly SearchValues<char> PlainStops     = SearchValues.Create("'\"[-/");
	static readonly SearchValues<char> CommentStops   = SearchValues.Create("*");
#else
	static readonly char[] TextStops      = "'\"[-/$".ToCharArray();
	static readonly char[] StringStops    = "'$".ToCharArray();
	static readonly char[] QuotedStops    = "\"$".ToCharArray();
	static readonly char[] BracketedStops = "]$".ToCharArray();
	static readonly char[] PlainStops     = "'\"[-/".ToCharArray();
	static readonly char[] CommentStops   = "*".ToCharArray();
#endif

	/// <summary>
	/// How deep <c>:r</c> may nest. sqlcmd has no stated limit; a cycle is caught by name before
	/// this is reached, so this is for a chain of distinct files, which no script needs this long.
	/// </summary>
	const int Deepest = 32;

	readonly ScriptOptions _options;
	readonly string        _separator;
	readonly bool          _commands;
	readonly bool          _substitute;

	readonly Dictionary<string, string> _variables = new(StringComparer.OrdinalIgnoreCase);
	readonly List<Run>                  _pending   = [];
	readonly List<ScriptOrigin>         _open      = [];

	State _state;
	bool  _stopped;
	bool  _joined;

	public readonly List<ScriptBatch>      Batches     = [];
	public readonly List<ScriptDirective>  Directives  = [];
	public readonly List<ScriptDiagnostic> Diagnostics = [];

	public bool SawClientSyntax;

	public SqlScriptReader(ScriptOptions options)
	{
		_options    = options;
		_separator  = options.Profile.Separator;
		_commands   = options.Profile.Commands;
		_substitute = _commands && options.SubstituteVariables;

		if (options.Variables is not null)
		{
			foreach (var variable in options.Variables)
				_variables[variable.Key] = variable.Value;
		}
	}

	/// <summary>
	/// A run of text waiting to be sent: a stretch of one source, or a reference to substitute
	/// when the batch is sent.
	/// </summary>
	readonly struct Run(ScriptOrigin origin, int at, int length, string? name)
	{
		public readonly ScriptOrigin Origin = origin;
		public readonly int          At     = at;
		public readonly int          Length = length;
		public readonly string?      Name   = name;
	}

	public void Read(string text)
	{
		var origin = new ScriptOrigin(_options.SourceName, text);

		_open.Add(origin);
		ReadSource(origin, Start(text), lineStart: true);
		_open.RemoveAt(_open.Count - 1);

		if (!_stopped)
			Send(1, separator: null);
	}

	/// <summary>
	/// Where reading begins: after a byte order mark, unless that is all there is — sqlcmd sends a
	/// file of nothing else as a batch holding it.
	/// </summary>
	static int Start(string text)
	{
		return text.Length > 1 && text[0] == '\uFEFF' ? 1 : 0;
	}

	/// <summary>
	/// Reads one source from <paramref name="at"/>, answering whether its end left the reading at
	/// the start of a line.
	/// </summary>
	bool ReadSource(ScriptOrigin origin, int at, bool lineStart)
	{
		var text = origin.Text;
		var end  = text.Length;

		while (at < end && !_stopped)
		{
			if (lineStart && _state == State.Text)
			{
				// A block comment that opens a line is passed over first, and the rest of the line it
				// closes on starts a line again: `/* c */ GO` ends a batch at the `*/`. Only at the very
				// start — after a space it is text, and so is a second comment after the first.
				if (at + 1 < end && text[at] == '/' && text[at + 1] == '*')
				{
					var close = text.IndexOf("*/", at + 2, StringComparison.Ordinal);
					var stop  = close < 0 ? end : close + 2;

					Append(origin, at, stop - at);

					if (close < 0)
					{
						_state = State.Comment;
						return false;
					}

					at = stop;
				}

				// Carriage returns before anything else on a line are sent, and leave the line where
				// it was: sqlcmd reads `\rGO` as a separator and sends the `\r` with the batch before.
				var returns = at;

				while (returns < end && text[returns] == '\r')
					returns++;

				Append(origin, at, returns - at);
				at = returns;

				if (Command(origin, at, out var next))
				{
					at        = next;
					lineStart = !_joined;
					_joined   = false;
					continue;
				}
			}

			var newline = text.IndexOf('\n', at);
			var stopAt  = newline < 0 ? end : newline + 1;

			at        = Scan(origin, at, stopAt);
			lineStart = newline >= 0;
		}

		return lineStart;
	}

	/// <summary>
	/// Scans <c>[at, stop)</c>, part of one line, keeping what it holds for the batch and noting
	/// every variable reference; answers where it stopped, which is <paramref name="stop"/>.
	/// </summary>
	int Scan(ScriptOrigin origin, int at, int stop)
	{
		var text = origin.Text;
		var run  = at;

		while (at < stop)
		{
			var rest = text.AsSpan(at, stop - at);
			var next = _state switch
			{
				State.Text      => _substitute ? rest.IndexOfAny(TextStops) : rest.IndexOfAny(PlainStops),
				State.String    => _substitute ? rest.IndexOfAny(StringStops) : rest.IndexOf('\''),
				State.Quoted    => _substitute ? rest.IndexOfAny(QuotedStops) : rest.IndexOf('"'),
				State.Bracketed => _substitute ? rest.IndexOfAny(BracketedStops) : rest.IndexOf(']'),
				_               => rest.IndexOfAny(CommentStops),
			};

			if (next < 0)
				break;

			at += next;

			var character = text[at];
			var following = at + 1 < stop ? text[at + 1] : '\0';

			switch (character)
			{
				case '\'' when _state == State.Text:
					_state = State.String;
					at++;
					break;
				case '"' when _state == State.Text:
					_state = State.Quoted;
					at++;
					break;
				case '[' when _state == State.Text:
					_state = State.Bracketed;
					at++;
					break;
				case '\'':
				case '"':
				case ']':
					// The close, unless doubled, which is the character itself and stays inside.
					if (following == character)
					{
						at += 2;
					}
					else
					{
						_state = State.Text;
						at++;
					}

					break;
				case '-':
					// A line comment is the rest of the line, and nothing in it is substituted.
					at = following == '-' ? stop : at + 1;
					break;
				case '/':
					if (following == '*')
					{
						_state = State.Comment;
						at    += 2;
					}
					else
					{
						at++;
					}

					break;
				case '*':
					if (following == '/')
					{
						_state = State.Text;
						at    += 2;
					}
					else
					{
						at++;
					}

					break;
				default:
					if (following != '(')
					{
						at++;
						break;
					}

					// `$(name)`, closed on the same line, or the script ends here: sqlcmd refuses a
					// reference it cannot read, wherever outside a comment it stands.
					var name = Name(text, at + 2, stop);

					if (name < 0 || name >= stop || text[name] != ')')
					{
						Fatal("Syntax error in a variable reference.", origin, at, stop - at);
						return stop;
					}

					SawClientSyntax = true;

					Append(origin, run, at - run);
					_pending.Add(new Run(origin, at, name + 1 - at, text.Substring(at + 2, name - at - 2)));

					at  = name + 1;
					run = at;
					break;
			}
		}

		Append(origin, run, stop - run);
		return stop;
	}

	/// <summary>
	/// The end of a variable's name that begins at <paramref name="at"/>, or -1 where none does: a
	/// letter or <c>_</c>, then letters, digits, <c>_</c> and <c>-</c>.
	/// </summary>
	static int Name(string text, int at, int stop)
	{
		if (at >= stop || !(IsLetter(text[at]) || text[at] == '_'))
			return -1;

		at++;

		while (at < stop && (IsLetter(text[at]) || IsDigit(text[at]) || text[at] == '_' || text[at] == '-'))
			at++;

		return at;
	}

	// ── A line that is the tool's ────────────────────────────────────────────

	/// <summary>
	/// Whether the line at <paramref name="at"/> is the tool's — a separator or a command — and if
	/// so acts on it and says where the next line starts.
	/// </summary>
	bool Command(ScriptOrigin origin, int at, out int next)
	{
		var text    = origin.Text;
		var newline = text.IndexOf('\n', at);
		var end     = newline < 0 ? text.Length : newline;

		next = newline < 0 ? text.Length : newline + 1;

		while (at < end && IsBlank(text[at]))
			at++;

		if (at == end)
			return false;

		var line = new Line(origin, at, end);

		if (Word(text, at, end, _separator) is var afterSeparator and > 0)
		{
			if (!Bounded(text, afterSeparator, end))
				return false;

			SawClientSyntax = true;
			Separator(line, afterSeparator);
			return true;
		}

		if (!_commands)
			return false;

		if (text[at] == ':')
		{
			if (at + 2 < end && text[at + 1] == '!' && text[at + 2] == '!')
				return Directive(line, "!!", at + 3);

			var word = at + 1;

			while (word < end && IsLetter(text[word]))
				word++;

			if (word == at + 1 || !Bounded(text, word, end))
				return false;

			var name = text.Substring(at + 1, word - at - 1).ToLowerInvariant();

			switch (name)
			{
				case "setvar":
					SawClientSyntax = true;
					SetVariable(line, word);
					return true;
				case "r":
					SawClientSyntax = true;
					Include(line, word);
					return true;
				case "on":
					// `:on error`, written so: one blank between the words, and whatever follows
					// `error` is its argument. `:on` and anything else is text.
					if (word < end && IsBlank(text[word]) && Word(text, word + 1, end, "error") is var afterError and > 0)
						return Directive(line, name, word);

					return false;
				case "exit":
					return Exit(line, word, ref next);
				case "quit":
				case "reset":
				case "ed":
				case "error":
				case "out":
				case "perftrace":
				case "connect":
				case "list":
				case "listvar":
				case "help":
				case "serverlist":
				case "xml":
					return Directive(line, name, word);
				default:
					// A word sqlcmd has no command for is text, and the server is sent it.
					return false;
			}
		}

		if (at + 1 < end && text[at] == '!' && text[at + 1] == '!')
			return Directive(line, "!!", at + 2);

		// Four commands may be written without the colon.
		foreach (var bare in Bare)
		{
			var afterBare = Word(text, at, end, bare);

			if (afterBare < 0 || !Bounded(text, afterBare, end))
				continue;

			return bare == "exit" ? Exit(line, afterBare, ref next) : Directive(line, bare, afterBare);
		}

		return false;
	}

	static readonly string[] Bare = ["exit", "quit", "reset", "ed"];

	/// <summary>
	/// <c>exit</c>, or <c>exit(query)</c>, whose brackets sqlcmd reads on over line breaks until they
	/// close: the command is every line up to the one that closes them.
	/// </summary>
	bool Exit(Line line, int after, ref int next)
	{
		var text = line.Text;
		var at   = Skip(text, after, line.End);

		if (at < line.End && text[at] == '(' && text.IndexOf(')', at, line.End - at) < 0)
		{
			var close = text.IndexOf(')', line.End);

			if (close < 0)
			{
				SawClientSyntax = true;
				Fatal("Unexpected end of script near command 'exit'.", line);
				next = text.Length;
				return true;
			}

			var newline = text.IndexOf('\n', close);

			line = new Line(line.Origin, line.At, newline < 0 ? text.Length : newline);
			next = newline < 0 ? text.Length : newline + 1;
		}

		return Directive(line, "exit", after);
	}

	/// <summary>
	/// A line of the tool's: where it was written and where its command's word ends.
	/// </summary>
	readonly struct Line(ScriptOrigin origin, int at, int end)
	{
		public readonly ScriptOrigin Origin = origin;
		public readonly int          At     = at;
		public readonly int          End    = end;

		public string Text
		{
			get
			{
				return Origin.Text;
			}
		}

		public ScriptLocation Location
		{
			get
			{
				var end = End;

				while (end > At && IsSpace(Origin.Text[end - 1]))
					end--;

				return new ScriptLocation(Origin.Name, new SqlSpan(At, end - At));
			}
		}

		/// <summary>
		/// The rest of the line from <paramref name="from"/>, without the spacing around it.
		/// </summary>
		public string Rest(int from)
		{
			var text  = Origin.Text;
			var start = Skip(text, from, End);
			var end   = End;

			while (end > start && IsSpace(text[end - 1]))
				end--;

			return text.Substring(start, end - start);
		}
	}

	/// <summary>
	/// A command that is reported and not run, once its line is one sqlcmd would read.
	/// </summary>
	bool Directive(Line line, string name, int after)
	{
		var arguments = line.Rest(after);

		SawClientSyntax = true;

		if (Malformed(name, arguments) is { } problem)
		{
			Fatal(problem, line);
			return true;
		}

		Directives.Add(new ScriptDirective(name, arguments, line.Location, Batches.Count));
		return true;
	}

	/// <summary>
	/// Why sqlcmd would refuse a command's arguments, or null where it reads them.
	/// </summary>
	static string? Malformed(string name, string arguments)
	{
		switch (name)
		{
			case "exit":
				// Nothing, or a query in brackets: `exit(SELECT 1)`.
				return arguments.Length == 0 || arguments[0] == '(' ? null : "Syntax error near command 'exit'.";
			case "on":
				// `:on error exit` or `:on error ignore`: one word after `error`, and no more.
				var words = arguments.Substring(5).Trim(' ', '\t', '\r');

				return words.IndexOfAny([' ', '\t', '\r']) < 0 ? null : "Syntax error near command ':on error'.";
			case "quit":
			case "reset":
			case "ed":
			case "list":
			case "listvar":
			case "help":
			case "serverlist":
				return arguments.Length == 0 ? null : $"Syntax error near command '{name}'.";
			case "error":
			case "out":
			case "perftrace":
			case "connect":
			case "!!":
				return arguments.Length > 0 ? null : $"Syntax error near command '{name}'.";
			case "xml":
				// `on` or `off`; sqlcmd for Linux reads either and then says it has no XML mode.
				return arguments.Length > 0 && IsLetter(arguments[0]) ? null : "Syntax error near command 'xml'.";
			default:
				return null;
		}
	}

	/// <summary>
	/// A separator line: the batch so far is sent, as many times as it says.
	/// </summary>
	void Separator(Line line, int after)
	{
		var text  = line.Text;
		var at    = Skip(text, after, line.End);
		var count = 1L;

		if (at < line.End && !Comment(text, at, line.End))
		{
			string digits;
			int    tail;

			if (_substitute && text[at] == '$' && at + 1 < line.End && text[at + 1] == '(')
			{
				// `GO $(n)`: the count is a variable's, and only the reference is written there.
				var name = Name(text, at + 2, line.End);

				if (name < 0 || name >= line.End || text[name] != ')')
				{
					Fatal("Syntax error in a variable reference.", line);
					return;
				}

				var variable = text.Substring(at + 2, name - at - 2);
				var after2   = Skip(text, name + 1, line.End);

				if (after2 < line.End && !Comment(text, after2, line.End))
				{
					Fatal($"Syntax error near command '{_separator}'.", line);
					return;
				}

				// A variable with no value, or with one that is not a count: sqlcmd drops the batch
				// and goes on, saying so only for the first.
				if (!_variables.TryGetValue(variable, out var value))
				{
					Diagnostics.Add(new ScriptDiagnostic(
						ScriptSeverity.Warning, $"'{variable}' scripting variable not defined.", line.Location));
					_pending.Clear();
					return;
				}

				digits = value.Trim(' ', '\t');

				if (digits.Length == 0 || !AllDigits(digits))
				{
					Diagnostics.Add(new ScriptDiagnostic(
						ScriptSeverity.Warning, $"'{value}' is not a count, so the batch is not sent.", line.Location));
					_pending.Clear();
					return;
				}

				tail = name + 1;
			}
			else
			{
				var end = at;

				while (end < line.End && IsDigit(text[end]))
					end++;

				digits = text.Substring(at, end - at);
				tail   = end;
			}

			tail = Skip(text, tail, line.End);

			if (digits.Length == 0 || !AllDigits(digits) || tail < line.End && !Comment(text, tail, line.End))
			{
				Fatal($"Syntax error near command '{_separator}'.", line);
				return;
			}

			count = Count(digits);
		}

		Send(count, line.Location);
	}

	static long Count(string digits)
	{
		var count = 0L;

		foreach (var digit in digits)
		{
			if (count > (long.MaxValue - 9) / 10)
				return long.MaxValue;

			count = count * 10 + (digit - '0');
		}

		return count;
	}

	/// <summary>
	/// <c>:setvar name value</c>: the value is stored as written, not substituted, its quotes
	/// taken off; with no value the variable is gone.
	/// </summary>
	void SetVariable(Line line, int after)
	{
		var text  = line.Text;
		var at    = Skip(text, after, line.End);
		var name  = Name(text, at, line.End);
		var end   = line.End;

		while (end > at && IsSpace(text[end - 1]))
			end--;

		if (after >= line.End || !IsBlank(text[after]) || name < 0 || name < end && !IsSpace(text[name]))
		{
			Fatal("Syntax error near command ':setvar'.", line);
			return;
		}

		var variable = text.Substring(at, name - at);
		var start    = Skip(text, name, end);

		Directives.Add(new ScriptDirective("setvar", line.Rest(after), line.Location, Batches.Count));

		if (start == end)
		{
			_variables.Remove(variable);
			return;
		}

		if (text[start] != '"')
		{
			// One word: a blank inside, or a quote, and sqlcmd refuses the line.
			for (var i = start; i < end; i++)
			{
				if (IsSpace(text[i]) || text[i] == '"')
				{
					Fatal("Syntax error near command ':setvar'.", line);
					return;
				}
			}

			_variables[variable] = text.Substring(start, end - start);
			return;
		}

		// A quoted value, with `""` for a quote inside.
		var value = new StringBuilder();
		var at2   = start + 1;

		while (true)
		{
			var quote = text.IndexOf('"', at2, end - at2);

			if (quote < 0)
			{
				Fatal("Unexpected end of script near command ':setvar'.", line);
				return;
			}

			value.Append(text, at2, quote - at2);

			if (quote + 1 < end && text[quote + 1] == '"')
			{
				value.Append('"');
				at2 = quote + 2;
				continue;
			}

			at2 = quote + 1;
			break;
		}

		if (at2 < end)
		{
			Fatal("Syntax error near command ':setvar'.", line);
			return;
		}

		_variables[variable] = value.ToString();
	}

	/// <summary>
	/// <c>:r file</c>: the file's text is read in place, as if written here, carrying on in whatever
	/// string or comment the reading was in.
	/// </summary>
	void Include(Line line, int after)
	{
		var written = line.Rest(after);

		Directives.Add(new ScriptDirective("r", written, line.Location, Batches.Count));

		var path = Path(line, after);

		if (path is null)
			return;

		var source = _options.ResolveInclude?.Invoke(new ScriptInclude(path, line.Location));

		if (source is null)
		{
			Diagnostics.Add(new ScriptDiagnostic(ScriptSeverity.Error, $"'{path}': Invalid filename.", line.Location));
			return;
		}

		foreach (var open in _open)
		{
			if (string.Equals(open.Name, source.Name, StringComparison.Ordinal))
			{
				Diagnostics.Add(new ScriptDiagnostic(
					ScriptSeverity.Error, $"File '{source.Name}' recursively included.", line.Location));
				return;
			}
		}

		if (_open.Count > Deepest)
		{
			Diagnostics.Add(new ScriptDiagnostic(
				ScriptSeverity.Error, $"'{path}': included more than {Deepest} deep.", line.Location));
			return;
		}

		var origin = new ScriptOrigin(source.Name, source.Text);

		_open.Add(origin);

		// A file whose last line has no line break runs on into the line after the :r.
		_joined = !ReadSource(origin, Start(source.Text), lineStart: true);

		_open.RemoveAt(_open.Count - 1);
	}

	/// <summary>
	/// The file a <c>:r</c> names: one word, its references substituted now, or one in double quotes,
	/// taken as written; null where sqlcmd refuses the line, which ends the script.
	/// </summary>
	string? Path(Line line, int after)
	{
		var text = line.Text;
		var at   = Skip(text, after, line.End);
		var end  = at;

		if (at == line.End)
		{
			Fatal("Syntax error near command ':r'.", line);
			return null;
		}

		string path;

		if (text[at] == '"')
		{
			var close = text.IndexOf('"', at + 1, line.End - at - 1);

			if (close < 0)
			{
				Fatal("Unexpected end of script near command ':r'.", line);
				return null;
			}

			path = text.Substring(at + 1, close - at - 1);
			end  = close + 1;
		}
		else
		{
			var result = new StringBuilder();

			while (end < line.End && !IsSpace(text[end]))
			{
				if (text[end] != '$' || !_substitute)
				{
					result.Append(text[end++]);
					continue;
				}

				var name = end + 1 < line.End && text[end + 1] == '(' ? Name(text, end + 2, line.End) : -1;

				if (name < 0 || name >= line.End || text[name] != ')')
				{
					Fatal("Syntax error near command ':r'.", line);
					return null;
				}

				var variable = text.Substring(end + 2, name - end - 2);

				if (_variables.TryGetValue(variable, out var value))
				{
					result.Append(value);
				}
				else
				{
					Diagnostics.Add(new ScriptDiagnostic(
						ScriptSeverity.Warning, $"'{variable}' scripting variable not defined.", line.Location));
					result.Append(text, end, name + 1 - end);
				}

				end = name + 1;
			}

			path = result.ToString();
		}

		if (Skip(text, end, line.End) < line.End)
		{
			Fatal("Syntax error near command ':r'.", line);
			return null;
		}

		return path;
	}

	// ── The batch ────────────────────────────────────────────────────────────

	void Append(ScriptOrigin origin, int at, int length)
	{
		if (length <= 0)
			return;

		if (_pending.Count > 0)
		{
			var last = _pending[_pending.Count - 1];

			if (last.Name is null && ReferenceEquals(last.Origin, origin) && last.At + last.Length == at)
			{
				_pending[_pending.Count - 1] = new Run(origin, last.At, last.Length + length, null);
				return;
			}
		}

		_pending.Add(new Run(origin, at, length, null));
	}

	/// <summary>
	/// Sends the batch so far, substituting its references with the variables as they stand now,
	/// at its end; a batch with nothing in it is not sent.
	/// </summary>
	void Send(long count, ScriptLocation? separator)
	{
		if (_pending.Count == 0)
			return;

		if (_pending.Count == 1 && _pending[0].Name is null)
		{
			var run = _pending[0];

			_pending.Clear();
			Batches.Add(new ScriptBatch(
				run.Origin.Text, run.At, run.Length, count, verbatim: true, separator,
				[new ScriptPiece(run.At, run.Length, run.Origin, run.At, run.Length, substituted: false)]));
			return;
		}

		// An undefined reference stays as written, so it is text like the text around it, and a
		// batch whose only references are undefined is still one run of its source.
		var runs = new List<Run>(_pending.Count);

		foreach (var run in _pending)
		{
			if (run.Name is not null && !_variables.ContainsKey(run.Name))
			{
				Diagnostics.Add(new ScriptDiagnostic(
					ScriptSeverity.Warning,
					$"'{run.Name}' scripting variable not defined.",
					new ScriptLocation(run.Origin.Name, new SqlSpan(run.At, run.Length))));
			}

			var plain = run.Name is null || !_variables.ContainsKey(run.Name);

			if (plain && runs.Count > 0)
			{
				var last = runs[runs.Count - 1];
				var same = last.Name is null || !_variables.ContainsKey(last.Name);

				if (same && ReferenceEquals(last.Origin, run.Origin) && last.At + last.Length == run.At)
				{
					runs[runs.Count - 1] = new Run(run.Origin, last.At, last.Length + run.Length, null);
					continue;
				}
			}

			runs.Add(plain ? new Run(run.Origin, run.At, run.Length, null) : run);
		}

		_pending.Clear();

		if (runs.Count == 1 && runs[0].Name is null)
		{
			var only = runs[0];

			Batches.Add(new ScriptBatch(
				only.Origin.Text, only.At, only.Length, count, verbatim: true, separator,
				[new ScriptPiece(only.At, only.Length, only.Origin, only.At, only.Length, substituted: false)]));
			return;
		}

		var text   = new StringBuilder();
		var pieces = new ScriptPiece[runs.Count];

		for (var i = 0; i < runs.Count; i++)
		{
			var run = runs[i];
			var at  = text.Length;

			if (run.Name is null)
			{
				text.Append(run.Origin.Text, run.At, run.Length);
				pieces[i] = new ScriptPiece(at, run.Length, run.Origin, run.At, run.Length, substituted: false);
			}
			else
			{
				var value = _variables[run.Name];

				text.Append(value);
				pieces[i] = new ScriptPiece(at, value.Length, run.Origin, run.At, run.Length, substituted: true);
			}
		}

		Batches.Add(new ScriptBatch(text.ToString(), 0, text.Length, count, verbatim: false, separator, pieces));
	}

	void Fatal(string message, Line line)
	{
		Fatal(message, line.Origin, line.At, line.Location.Span.Length);
	}

	void Fatal(string message, ScriptOrigin origin, int at, int length)
	{
		Diagnostics.Add(new ScriptDiagnostic(
			ScriptSeverity.Fatal, message, new ScriptLocation(origin.Name, new SqlSpan(at, length))));

		_stopped = true;
		_pending.Clear();
	}

	// ── Characters ───────────────────────────────────────────────────────────

	/// <summary>
	/// Where a word that may end a command starts its own end: the position after
	/// <paramref name="word"/> where the line at <paramref name="at"/> begins with it in any case,
	/// or -1.
	/// </summary>
	static int Word(string text, int at, int end, string word)
	{
		if (end - at < word.Length)
			return -1;

		for (var i = 0; i < word.Length; i++)
		{
			if (ToLower(text[at + i]) != ToLower(word[i]))
				return -1;
		}

		return at + word.Length;
	}

	/// <summary>
	/// Whether a command's word ends at <paramref name="at"/>, which is what sqlcmd asks before it
	/// reads the line as its own.
	/// </summary>
	/// <remarks>
	/// Asked of sqlcmd one character at a time: the end of the line, a blank or a carriage return,
	/// and the characters that open something — <c>( - / ' " $ [ !</c> — end the word, and the line
	/// is the command's, to be read or refused as one (<c>GO--</c> reads, <c>GO(</c> is refused).
	/// Anything else — letters, digits, <c>_ . ; , * = : ) # @ +</c> — goes on with the word, and
	/// the line is text: <c>GO;</c>, <c>exit_x</c>, <c>quit.x</c>.
	/// </remarks>
	static bool Bounded(string text, int at, int end)
	{
		if (at >= end)
			return true;

		return text[at] is ' ' or '\t' or '\r' or '(' or '-' or '/' or '\'' or '"' or '$' or '[' or '!';
	}

	static bool Comment(string text, int at, int end)
	{
		return at + 1 < end && text[at] == '-' && text[at + 1] == '-';
	}

	static int Skip(string text, int at, int end)
	{
		while (at < end && IsSpace(text[at]))
			at++;

		return at;
	}

	static bool AllDigits(string text)
	{
		foreach (var character in text)
		{
			if (!IsDigit(character))
				return false;
		}

		return true;
	}

	static bool IsBlank(char character)
	{
		return character is ' ' or '\t';
	}

	/// <summary>
	/// Spacing inside a line of the tool's: a blank, or a carriage return, which ends a command's
	/// line as a blank would — <c>GO\r</c> at the end of a file is a separator.
	/// </summary>
	static bool IsSpace(char character)
	{
		return character is ' ' or '\t' or '\r';
	}

	static bool IsLetter(char character)
	{
		return character is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
	}

	static bool IsDigit(char character)
	{
		return character is >= '0' and <= '9';
	}

	static char ToLower(char character)
	{
		return character is >= 'A' and <= 'Z' ? (char)(character | 0x20) : character;
	}
}

/// <summary>
/// One text a script is read from: the script itself or an included file, by identity, since the
/// same file included twice is two of these.
/// </summary>
sealed class ScriptOrigin(string? name, string text)
{
	public readonly string? Name = name;
	public readonly string  Text = text;
}

/// <summary>
/// A stretch of a batch's text and where it came from: verbatim from a source, or the value of a
/// reference that was written there.
/// </summary>
readonly struct ScriptPiece(int textAt, int length, ScriptOrigin source, int sourceAt, int sourceLength, bool substituted)
{
	public readonly int          TextAt       = textAt;
	public readonly int          Length       = length;
	public readonly ScriptOrigin Source       = source;
	public readonly int          SourceAt     = sourceAt;
	public readonly int          SourceLength = sourceLength;
	public readonly bool         Substituted  = substituted;

	public int TextEnd
	{
		get
		{
			return TextAt + Length;
		}
	}

	/// <summary>
	/// Where a range that begins at <paramref name="at"/> of the batch begins in the source: the
	/// reference's start, for one that begins in a value.
	/// </summary>
	public int Start(int at)
	{
		if (Substituted)
			return SourceAt;

		return SourceAt + Math.Min(Math.Max(at - TextAt, 0), Length);
	}

	/// <summary>
	/// Where a range that ends at <paramref name="end"/> of the batch ends in the source: the
	/// reference's end, for one that ends in a value.
	/// </summary>
	public int End(int end)
	{
		if (Substituted)
			return end > TextAt ? SourceAt + SourceLength : SourceAt;

		return SourceAt + Math.Min(Math.Max(end - TextAt, 0), Length);
	}
}
