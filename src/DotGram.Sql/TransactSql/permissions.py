# Writes PermissionCatalogue.cs: what permission may be written of what securable class. And from the
# same pairs, Sql2023Ast.Permissions.cs: the tree's PrivilegeKind and SecurableClass.
#
#     python src/DotGram.Sql/TransactSql/permissions.py              asks the server, writes both
#     python src/DotGram.Sql/TransactSql/permissions.py --catalogue  asks no server: reads the pairs back
#                                                                    from PermissionCatalogue.cs and
#                                                                    writes the tree's file alone
#
# The pairs come from the SERVER, and are then held to the PARSER one statement at a time, which is
# not the same list:
#
#     SELECT class_desc, permission_name FROM sys.fn_builtin_permissions(DEFAULT)
#
# gives 301 rows over 29 classes, and `GRANT CONTROL ON CREDENTIAL::x TO u` is `Incorrect syntax near
# 'CREDENTIAL'`. So CREDENTIAL is a class the view lists and the parser will not take, its two pairs
# can never be reached, and the catalogue holds 299. A class the view omits is not one either: TABLE,
# VIEW, PROCEDURE, SEQUENCE, COLUMN MASTER KEY, EXTERNAL DATA SOURCE and six more were asked and all
# refused, all of them being OBJECT.
#
# Why the tree needs this at all: with an ON clause the engine checks the permission against the
# class WHILE PARSING and answers 102 -- `GRANT SELECT ON ENDPOINT::x` is `Incorrect syntax near
# 'SELECT'`, SELECT being none of ENDPOINT's five. Without an ON clause it does not: mixed scopes are
# Msg 4620 at level 16, after the parse, and the server's business rather than this grammar's.
#
# ALL, ALL PRIVILEGES and EXEC are in no row of the view and are read of every class, so they are
# written in by hand here, as `PermissionName` in the grammar does with them.
#
# THE TREE'S ENUMS ARE THIS CATALOGUE, NOT A LIST KEPT BESIDE IT (D148). PrivilegeKind is the standard's
# ten actions and then every permission here that the standard does not name, each called for its words
# -- VIEW DEFINITION is ViewDefinition -- and SecurableClass is the classes. PermissionCatalogueTests
# holds the enums, this table and the grammar's two lists to one another, so a permission added here and
# not there fails a test rather than a consumer. The words a member stands for are PermissionWords.Text
# and nothing else, which is what keeps the way back open: should the enum give way to plain strings --
# D148 says when -- every member's string is already that table's right-hand side.
#
# The test holds the committed members to the committed catalogue; it does not hold the generated file
# to a fresh run of this script, since Python is not part of CI. So after any change to the catalogue or
# to the templates below, run this script (with --catalogue where there is no server) and commit what it
# writes.
#
# Run it against a server whose version the grammar is written for, and read the diff: a permission or
# a class that a release adds appears here, and the grammar's own PermissionName and SecurableClassName
# want the same addition. Nothing reads this table at run time except the parser's guard.

import re
import subprocess
import sys
import textwrap

SQLCMD = r'C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\180\Tools\Binn\sqlcmd.exe'
SERVER = 'localhost'
OUTPUT = 'src/DotGram.Sql/TransactSql/PermissionCatalogue.cs'
TREE   = 'src/DotGram.Sql/Standard/Sql2023Ast.Permissions.cs'

# The class the view lists and the parser refuses. Asked, not assumed: see the header.
UNREACHABLE = {'CREDENTIAL'}

# The standard's <action>s, in the order the tree has always held them, so that their numbers do not
# move: the standard's grammar and the hand-written parser build them. Six are SQL Server's too, and
# are the same member there; USAGE, TRIGGER, UNDER and ALL PRIVILEGES are no row of the view.
STANDARD = ['ALL PRIVILEGES', 'SELECT', 'DELETE', 'INSERT', 'UPDATE', 'REFERENCES', 'USAGE', 'TRIGGER',
            'UNDER', 'EXECUTE']

# What T-SQL writes shorter for a standard member, which the tree keeps as Privilege.Abbreviated.
ABBREVIATED = {'ALL PRIVILEGES': 'ALL', 'EXECUTE': 'EXEC'}

CR = '\r\n'


def asked(query):
    done = subprocess.run(
        [SQLCMD, '-S', SERVER, '-E', '-d', 'master', '-C', '-l', '5', '-h', '-1', '-W', '-s', '|',
         '-Q', 'SET NOCOUNT ON; ' + query],
        capture_output=True, text=True, timeout=120)

    if done.returncode != 0:
        sys.exit('the server would not answer: ' + (done.stderr or done.stdout))

    return [line.strip() for line in done.stdout.splitlines() if '|' in line]


def pairs():
    rows = asked('SELECT class_desc, permission_name FROM sys.fn_builtin_permissions(DEFAULT) '
                 'ORDER BY class_desc, permission_name;')
    named = []

    for row in rows:
        klass, permission = (one.strip().upper() for one in row.split('|', 1))

        if klass not in UNREACHABLE:
            named.append((klass, permission))

    return named


def catalogued():
    # The pairs as the catalogue holds them, one `"CLASS|PERMISSION",` a line: what was asked of the
    # server last, and all a machine without one can know.
    text  = open(OUTPUT, encoding='utf-8-sig').read()
    named = re.findall(r'^\t\t"([A-Z ]+)\|([A-Z ]+)",\r?$', text, re.M)

    if not named:
        sys.exit(OUTPUT + ' holds no pairs this script can read')

    return named


def written(named):
    # Joined with a bare newline, because the whole text is converted to CRLF once at the end: joining
    # with CRLF here put a second carriage return on every one of these 299 lines, which the compiler
    # did not mind and git showed as 298 changed lines the moment it normalized the file.
    rows = '\n'.join('\t\t"%s|%s",' % pair for pair in sorted(named))

    return HEAD.replace('@ROWS@', rows).replace('@COUNT@', str(len(named))).replace('\n', CR)


def member(words):
    # A member is its words, capitalized and run together: nothing to choose, so nothing to argue with,
    # and the words can be had back from the name by anyone who needs them.
    return ''.join(word.capitalize() for word in words.split())


def enum_rows(entries):
    # entries: (words, summary). A member with its summary above it, wrapped as a hand would wrap it.
    rows = []

    for words, summary in entries:
        lines = textwrap.wrap(summary, 100, break_long_words=False, break_on_hyphens=False)
        text  = '\n'.join('\t/// ' + line for line in lines)

        rows.append('\t/// <summary>\n%s\n\t/// </summary>\n\t%s,' % (text, member(words)))

    return '\n'.join(rows)


def arms(enum, words):
    width = max(len(enum) + 1 + len(member(one)) for one in words)

    rows  = ['\t\t\t%s => "%s",' % ((enum + '.' + member(one)).ljust(width), one) for one in words]

    # Null, not a text: the writer refuses a value with no words rather than printing some other one.
    return '\n'.join(rows + ['\t\t\t%s => null,' % '_'.ljust(width)])


def tree(named):
    classes_of = {}

    for klass, permission in named:
        classes_of.setdefault(permission, set()).add(klass)

    permissions = sorted(classes_of)
    classes     = sorted({klass for klass, _ in named})
    added       = [one for one in permissions if one not in STANDARD]
    everything  = STANDARD + added

    # Two runs of words that capitalize alike would be one member, which no catalogue so far has done.
    for words in (everything, classes):
        if len({member(one) for one in words}) != len(words):
            sys.exit('two runs of words would be one member among: ' + ', '.join(words))

    def said(words):
        text = '<c>%s</c>' % words

        if words in ABBREVIATED:
            text += ', and T-SQL\'s <c>%s</c> where <see cref="Privilege.Abbreviated"/> is set' % ABBREVIATED[words]

        if words == 'ALL PRIVILEGES':
            return text + '. Read of every class.'

        if words not in classes_of:
            return text + '. The standard\'s; SQL Server has no such permission.'

        return text + '. Of ' + ', '.join(sorted(classes_of[words])) + '.'

    standard = enum_rows((one, said(one)) for one in STANDARD)
    ours     = enum_rows((one, said(one)) for one in added)
    kinds    = enum_rows((one, '<c>%s::</c>' % one) for one in classes)

    text = (TREE_HEAD
            .replace('@PERMISSIONS@', str(len(permissions)))
            .replace('@CLASSES@', str(len(classes)))
            .replace('@ADDED@', str(len(added)))
            .replace('@STANDARD_MEMBERS@', standard)
            .replace('@TSQL_MEMBERS@', ours)
            .replace('@CLASS_MEMBERS@', kinds)
            .replace('@PRIVILEGE_ARMS@', arms('PrivilegeKind', everything))
            .replace('@CLASS_ARMS@', arms('SecurableClass', classes)))

    return text.replace('\n', CR)


HEAD = '''// <auto-generated/>
#nullable enable

// Written by src/DotGram.Sql/TransactSql/permissions.py from the server's own answers. Do not edit:
// add the permission or the class there, and to the grammar's PermissionName or SecurableClassName.
//
// @COUNT@ pairs, from the 301 `sys.fn_builtin_permissions` gives less CREDENTIAL's two, which no
// statement can reach: the view lists CREDENTIAL as a class and the parser refuses `CREDENTIAL::x`.

using System;
using System.Collections.Generic;

namespace DotGram.Sql.TransactSql;

/// <summary>What permission may be written of what securable class, fixed when the grammar was built.</summary>
/// <remarks>
/// The parser reads this table and asks nothing of a consumer's object (D25): it is constants, decided
/// at build time. The engine makes the pairing a SYNTAX rule where an ON clause names a class --
/// <c>GRANT SELECT ON ENDPOINT::x</c> is 102, SELECT being none of ENDPOINT's five -- and leaves mixed
/// scopes without an ON clause to bind time, Msg 4620, which is the server's to answer and not ours.
/// </remarks>
internal static class PermissionCatalogue
{
\tstatic readonly HashSet<string> Pairs = new(StringComparer.Ordinal)
\t{
@ROWS@
\t};

\t/// <summary>Whether every permission written may be written of the securable's class.</summary>
\t/// <param name="permissions">The permissions as read, column lists and all.</param>
\t/// <param name="on">The securable clause as read, or null where the statement names none.</param>
\tinternal static bool Allows(string[]? permissions, string? on)
\t{
\t\t// No ON clause names no class, and then the engine does not make it syntax either.
\t\tif (permissions is null || on is null)
\t\t\treturn true;

\t\tvar named = Class(on);

\t\tforeach (var permission in permissions)
\t\t\tif (!Allows(named, permission))
\t\t\t\treturn false;

\t\treturn true;
\t}

\t/// <summary>Whether one permission may be written of one class.</summary>
\tinternal static bool Allows(string named, string permission)
\t{
\t\tvar one = Normalized(permission);

\t\t// ALL and ALL PRIVILEGES are every class's, and EXEC is EXECUTE's other spelling. The view has
\t\t// a row for none of the three.
\t\tif (one is "ALL" or "ALL PRIVILEGES")
\t\t\treturn true;

\t\tif (one == "EXEC")
\t\t\tone = "EXECUTE";

\t\treturn Pairs.Contains(named + "|" + one);
\t}

\t/// <summary>The class a securable clause names, which is OBJECT where it names only a name.</summary>
\tinternal static string Class(string on)
\t{
\t\tvar at = on.IndexOf("::", StringComparison.Ordinal);

\t\treturn at < 0 ? "OBJECT" : Normalized(on.Substring(0, at));
\t}

\t/// <summary>Upper case, one space between words, and without a column list.</summary>
\tstatic string Normalized(string text)
\t{
\t\tvar at = text.IndexOf('(');

\t\tif (at >= 0)
\t\t\ttext = text.Substring(0, at);

\t\treturn string.Join(
\t\t\t" ", text.ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
\t}
}
'''

TREE_HEAD = '''// <auto-generated/>
#nullable enable

// Written by src/DotGram.Sql/TransactSql/permissions.py from PermissionCatalogue.cs: its @PERMISSIONS@
// permissions and @CLASSES@ classes, after the standard's ten actions. Do not edit: a permission or a
// class is added to the catalogue, and this file is written again (D148).

using System;

namespace DotGram.Sql.Ast;

/// <summary>
/// What a privilege is a privilege to do: the standard's ten actions, and every permission SQL Server
/// publishes.
/// </summary>
/// <remarks>
/// <para>
/// The first ten are the standard's &lt;action&gt;. The @ADDED@ after them are T-SQL's, GRANT (Transact-SQL):
/// the permissions <c>sys.fn_builtin_permissions</c> lists that the standard does not, each named for its
/// words -- <c>VIEW DEFINITION</c> is <see cref="PrivilegeKind.ViewDefinition"/>. Where the two name one
/// permission, <c>SELECT</c> or <c>EXECUTE</c>, the member is the standard's.
/// </para>
/// <para>
/// A closed list because the engine's is: a permission SQL Server does not know is a syntax error there
/// and a refusal here. The members' numbers follow the words' order and move when a permission is
/// added, so nothing should keep them.
/// </para>
/// </remarks>
public enum PrivilegeKind
{
\t// BNF: <action>, in the order the tree has always held it.
@STANDARD_MEMBERS@

\t// T-SQL: GRANT (Transact-SQL). The catalogue's permissions the standard does not name.
@TSQL_MEMBERS@
}

/// <summary>
/// T-SQL: GRANT (Transact-SQL). The class a securable is written with: <c>SCHEMA</c> in
/// <c>ON SCHEMA::s</c>.
/// </summary>
/// <remarks>
/// The classes the parser takes, a closed list as the permissions are: SQL Server refuses any other
/// word before <c>::</c> as syntax. A securable written with its class is
/// <see cref="PrivilegeObject.Classed"/>; <c>ON t</c>, with none, is <see cref="PrivilegeObject.Named"/>.
/// </remarks>
public enum SecurableClass
{
@CLASS_MEMBERS@
}

/// <summary>
/// The words a permission and a class are written in, which is all the writer prints of them.
/// </summary>
static class PermissionWords
{
\t/// <summary>
\t/// A privilege's words, or null for a value that has none.
\t/// </summary>
\tpublic static string? Text(PrivilegeKind kind)
\t{
\t\treturn kind switch
\t\t{
@PRIVILEGE_ARMS@
\t\t};
\t}

\t/// <summary>
\t/// A class's words, without the <c>::</c> after them, or null for a value that has none.
\t/// </summary>
\tpublic static string? Text(SecurableClass kind)
\t{
\t\treturn kind switch
\t\t{
@CLASS_ARMS@
\t\t};
\t}
}
'''

if __name__ == '__main__':
    offline = '--catalogue' in sys.argv[1:]
    named   = catalogued() if offline else pairs()

    if not offline:
        open(OUTPUT, 'w', encoding='utf-8-sig', newline='').write(written(named))
        print('%s: %d pairs over %d classes' % (OUTPUT, len(named), len({one[0] for one in named})))

    open(TREE, 'w', encoding='utf-8-sig', newline='').write(tree(named))
    print('%s: %d permissions, %d classes' % (TREE, len({one[1] for one in named}), len({one[0] for one in named})))
