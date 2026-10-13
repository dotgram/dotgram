using System;

namespace DotGram.Handwritten;

/// <summary>Every <c>&lt;reserved word&gt;</c> of ISO/IEC 9075-2:2023 §5.2, and <c>None</c> for a word that is not one.</summary>
/// <remarks>
/// The order is the standard's own, and <see cref="SqlWords.Spellings"/> is in the same order:
/// the two are read together, so a word added to one is added to the other.
/// </remarks>
enum SqlWord
{
	/// <summary>No reserved word: a name, or something that is no word at all.</summary>
	Name,

	/// <summary><c>ABS</c></summary>
	Abs,
	/// <summary><c>ABSENT</c></summary>
	Absent,
	/// <summary><c>ACOS</c></summary>
	Acos,
	/// <summary><c>ALL</c></summary>
	All,
	/// <summary><c>ALLOCATE</c></summary>
	Allocate,
	/// <summary><c>ALTER</c></summary>
	Alter,
	/// <summary><c>AND</c></summary>
	And,
	/// <summary><c>ANY</c></summary>
	Any,
	/// <summary><c>ANY_VALUE</c></summary>
	AnyValue,
	/// <summary><c>ARE</c></summary>
	Are,
	/// <summary><c>ARRAY</c></summary>
	Array,
	/// <summary><c>ARRAY_AGG</c></summary>
	ArrayAgg,
	/// <summary><c>ARRAY_MAX_CARDINALITY</c></summary>
	ArrayMaxCardinality,
	/// <summary><c>AS</c></summary>
	As,
	/// <summary><c>ASENSITIVE</c></summary>
	Asensitive,
	/// <summary><c>ASIN</c></summary>
	Asin,
	/// <summary><c>ASYMMETRIC</c></summary>
	Asymmetric,
	/// <summary><c>AT</c></summary>
	At,
	/// <summary><c>ATAN</c></summary>
	Atan,
	/// <summary><c>ATOMIC</c></summary>
	Atomic,
	/// <summary><c>AUTHORIZATION</c></summary>
	Authorization,
	/// <summary><c>AVG</c></summary>
	Avg,
	/// <summary><c>BEGIN</c></summary>
	Begin,
	/// <summary><c>BEGIN_FRAME</c></summary>
	BeginFrame,
	/// <summary><c>BEGIN_PARTITION</c></summary>
	BeginPartition,
	/// <summary><c>BETWEEN</c></summary>
	Between,
	/// <summary><c>BIGINT</c></summary>
	Bigint,
	/// <summary><c>BINARY</c></summary>
	Binary,
	/// <summary><c>BLOB</c></summary>
	Blob,
	/// <summary><c>BOOLEAN</c></summary>
	Boolean,
	/// <summary><c>BOTH</c></summary>
	Both,
	/// <summary><c>BTRIM</c></summary>
	Btrim,
	/// <summary><c>BY</c></summary>
	By,
	/// <summary><c>CALL</c></summary>
	Call,
	/// <summary><c>CALLED</c></summary>
	Called,
	/// <summary><c>CARDINALITY</c></summary>
	Cardinality,
	/// <summary><c>CASCADED</c></summary>
	Cascaded,
	/// <summary><c>CASE</c></summary>
	Case,
	/// <summary><c>CAST</c></summary>
	Cast,
	/// <summary><c>CEIL</c></summary>
	Ceil,
	/// <summary><c>CEILING</c></summary>
	Ceiling,
	/// <summary><c>CHAR</c></summary>
	Char,
	/// <summary><c>CHAR_LENGTH</c></summary>
	CharLength,
	/// <summary><c>CHARACTER</c></summary>
	Character,
	/// <summary><c>CHARACTER_LENGTH</c></summary>
	CharacterLength,
	/// <summary><c>CHECK</c></summary>
	Check,
	/// <summary><c>CLASSIFIER</c></summary>
	Classifier,
	/// <summary><c>CLOB</c></summary>
	Clob,
	/// <summary><c>CLOSE</c></summary>
	Close,
	/// <summary><c>COALESCE</c></summary>
	Coalesce,
	/// <summary><c>COLLATE</c></summary>
	Collate,
	/// <summary><c>COLLECT</c></summary>
	Collect,
	/// <summary><c>COLUMN</c></summary>
	Column,
	/// <summary><c>COMMIT</c></summary>
	Commit,
	/// <summary><c>CONDITION</c></summary>
	Condition,
	/// <summary><c>CONNECT</c></summary>
	Connect,
	/// <summary><c>CONSTRAINT</c></summary>
	Constraint,
	/// <summary><c>CONTAINS</c></summary>
	Contains,
	/// <summary><c>CONVERT</c></summary>
	Convert,
	/// <summary><c>COPY</c></summary>
	Copy,
	/// <summary><c>CORR</c></summary>
	Corr,
	/// <summary><c>CORRESPONDING</c></summary>
	Corresponding,
	/// <summary><c>COS</c></summary>
	Cos,
	/// <summary><c>COSH</c></summary>
	Cosh,
	/// <summary><c>COUNT</c></summary>
	Count,
	/// <summary><c>COVAR_POP</c></summary>
	CovarPop,
	/// <summary><c>COVAR_SAMP</c></summary>
	CovarSamp,
	/// <summary><c>CREATE</c></summary>
	Create,
	/// <summary><c>CROSS</c></summary>
	Cross,
	/// <summary><c>CUBE</c></summary>
	Cube,
	/// <summary><c>CUME_DIST</c></summary>
	CumeDist,
	/// <summary><c>CURRENT</c></summary>
	Current,
	/// <summary><c>CURRENT_CATALOG</c></summary>
	CurrentCatalog,
	/// <summary><c>CURRENT_DATE</c></summary>
	CurrentDate,
	/// <summary><c>CURRENT_DEFAULT_TRANSFORM_GROUP</c></summary>
	CurrentDefaultTransformGroup,
	/// <summary><c>CURRENT_PATH</c></summary>
	CurrentPath,
	/// <summary><c>CURRENT_ROLE</c></summary>
	CurrentRole,
	/// <summary><c>CURRENT_ROW</c></summary>
	CurrentRow,
	/// <summary><c>CURRENT_SCHEMA</c></summary>
	CurrentSchema,
	/// <summary><c>CURRENT_TIME</c></summary>
	CurrentTime,
	/// <summary><c>CURRENT_TIMESTAMP</c></summary>
	CurrentTimestamp,
	/// <summary><c>CURRENT_TRANSFORM_GROUP_FOR_TYPE</c></summary>
	CurrentTransformGroupForType,
	/// <summary><c>CURRENT_USER</c></summary>
	CurrentUser,
	/// <summary><c>CURSOR</c></summary>
	Cursor,
	/// <summary><c>CYCLE</c></summary>
	Cycle,
	/// <summary><c>DATE</c></summary>
	Date,
	/// <summary><c>DAY</c></summary>
	Day,
	/// <summary><c>DEALLOCATE</c></summary>
	Deallocate,
	/// <summary><c>DEC</c></summary>
	Dec,
	/// <summary><c>DECFLOAT</c></summary>
	Decfloat,
	/// <summary><c>DECIMAL</c></summary>
	Decimal,
	/// <summary><c>DECLARE</c></summary>
	Declare,
	/// <summary><c>DEFAULT</c></summary>
	Default,
	/// <summary><c>DEFINE</c></summary>
	Define,
	/// <summary><c>DELETE</c></summary>
	Delete,
	/// <summary><c>DENSE_RANK</c></summary>
	DenseRank,
	/// <summary><c>DEREF</c></summary>
	Deref,
	/// <summary><c>DESCRIBE</c></summary>
	Describe,
	/// <summary><c>DETERMINISTIC</c></summary>
	Deterministic,
	/// <summary><c>DISCONNECT</c></summary>
	Disconnect,
	/// <summary><c>DISTINCT</c></summary>
	Distinct,
	/// <summary><c>DOUBLE</c></summary>
	Double,
	/// <summary><c>DROP</c></summary>
	Drop,
	/// <summary><c>DYNAMIC</c></summary>
	Dynamic,
	/// <summary><c>EACH</c></summary>
	Each,
	/// <summary><c>ELEMENT</c></summary>
	Element,
	/// <summary><c>ELSE</c></summary>
	Else,
	/// <summary><c>EMPTY</c></summary>
	Empty,
	/// <summary><c>END</c></summary>
	End,
	/// <summary><c>END_FRAME</c></summary>
	EndFrame,
	/// <summary><c>END_PARTITION</c></summary>
	EndPartition,
	/// <summary><c>END-EXEC</c></summary>
	EndExec,
	/// <summary><c>EQUALS</c></summary>
	Equals,
	/// <summary><c>ESCAPE</c></summary>
	Escape,
	/// <summary><c>EVERY</c></summary>
	Every,
	/// <summary><c>EXCEPT</c></summary>
	Except,
	/// <summary><c>EXEC</c></summary>
	Exec,
	/// <summary><c>EXECUTE</c></summary>
	Execute,
	/// <summary><c>EXISTS</c></summary>
	Exists,
	/// <summary><c>EXP</c></summary>
	Exp,
	/// <summary><c>EXTERNAL</c></summary>
	External,
	/// <summary><c>EXTRACT</c></summary>
	Extract,
	/// <summary><c>FALSE</c></summary>
	False,
	/// <summary><c>FETCH</c></summary>
	Fetch,
	/// <summary><c>FILTER</c></summary>
	Filter,
	/// <summary><c>FIRST_VALUE</c></summary>
	FirstValue,
	/// <summary><c>FLOAT</c></summary>
	Float,
	/// <summary><c>FLOOR</c></summary>
	Floor,
	/// <summary><c>FOR</c></summary>
	For,
	/// <summary><c>FOREIGN</c></summary>
	Foreign,
	/// <summary><c>FRAME_ROW</c></summary>
	FrameRow,
	/// <summary><c>FREE</c></summary>
	Free,
	/// <summary><c>FROM</c></summary>
	From,
	/// <summary><c>FULL</c></summary>
	Full,
	/// <summary><c>FUNCTION</c></summary>
	Function,
	/// <summary><c>FUSION</c></summary>
	Fusion,
	/// <summary><c>GET</c></summary>
	Get,
	/// <summary><c>GLOBAL</c></summary>
	Global,
	/// <summary><c>GRANT</c></summary>
	Grant,
	/// <summary><c>GREATEST</c></summary>
	Greatest,
	/// <summary><c>GROUP</c></summary>
	Group,
	/// <summary><c>GROUPING</c></summary>
	Grouping,
	/// <summary><c>GROUPS</c></summary>
	Groups,
	/// <summary><c>HAVING</c></summary>
	Having,
	/// <summary><c>HOLD</c></summary>
	Hold,
	/// <summary><c>HOUR</c></summary>
	Hour,
	/// <summary><c>IDENTITY</c></summary>
	Identity,
	/// <summary><c>IN</c></summary>
	In,
	/// <summary><c>INDICATOR</c></summary>
	Indicator,
	/// <summary><c>INITIAL</c></summary>
	Initial,
	/// <summary><c>INNER</c></summary>
	Inner,
	/// <summary><c>INOUT</c></summary>
	Inout,
	/// <summary><c>INSENSITIVE</c></summary>
	Insensitive,
	/// <summary><c>INSERT</c></summary>
	Insert,
	/// <summary><c>INT</c></summary>
	Int,
	/// <summary><c>INTEGER</c></summary>
	Integer,
	/// <summary><c>INTERSECT</c></summary>
	Intersect,
	/// <summary><c>INTERSECTION</c></summary>
	Intersection,
	/// <summary><c>INTERVAL</c></summary>
	Interval,
	/// <summary><c>INTO</c></summary>
	Into,
	/// <summary><c>IS</c></summary>
	Is,
	/// <summary><c>JOIN</c></summary>
	Join,
	/// <summary><c>JSON</c></summary>
	Json,
	/// <summary><c>JSON_ARRAY</c></summary>
	JsonArray,
	/// <summary><c>JSON_ARRAYAGG</c></summary>
	JsonArrayagg,
	/// <summary><c>JSON_EXISTS</c></summary>
	JsonExists,
	/// <summary><c>JSON_OBJECT</c></summary>
	JsonObject,
	/// <summary><c>JSON_OBJECTAGG</c></summary>
	JsonObjectagg,
	/// <summary><c>JSON_QUERY</c></summary>
	JsonQuery,
	/// <summary><c>JSON_SCALAR</c></summary>
	JsonScalar,
	/// <summary><c>JSON_SERIALIZE</c></summary>
	JsonSerialize,
	/// <summary><c>JSON_TABLE</c></summary>
	JsonTable,
	/// <summary><c>JSON_TABLE_PRIMITIVE</c></summary>
	JsonTablePrimitive,
	/// <summary><c>JSON_VALUE</c></summary>
	JsonValue,
	/// <summary><c>LAG</c></summary>
	Lag,
	/// <summary><c>LANGUAGE</c></summary>
	Language,
	/// <summary><c>LARGE</c></summary>
	Large,
	/// <summary><c>LAST_VALUE</c></summary>
	LastValue,
	/// <summary><c>LATERAL</c></summary>
	Lateral,
	/// <summary><c>LEAD</c></summary>
	Lead,
	/// <summary><c>LEADING</c></summary>
	Leading,
	/// <summary><c>LEAST</c></summary>
	Least,
	/// <summary><c>LEFT</c></summary>
	Left,
	/// <summary><c>LIKE</c></summary>
	Like,
	/// <summary><c>LIKE_REGEX</c></summary>
	LikeRegex,
	/// <summary><c>LISTAGG</c></summary>
	Listagg,
	/// <summary><c>LN</c></summary>
	Ln,
	/// <summary><c>LOCAL</c></summary>
	Local,
	/// <summary><c>LOCALTIME</c></summary>
	Localtime,
	/// <summary><c>LOCALTIMESTAMP</c></summary>
	Localtimestamp,
	/// <summary><c>LOG</c></summary>
	Log,
	/// <summary><c>LOG10</c></summary>
	Log10,
	/// <summary><c>LOWER</c></summary>
	Lower,
	/// <summary><c>LPAD</c></summary>
	Lpad,
	/// <summary><c>LTRIM</c></summary>
	Ltrim,
	/// <summary><c>MATCH</c></summary>
	Match,
	/// <summary><c>MATCH_NUMBER</c></summary>
	MatchNumber,
	/// <summary><c>MATCH_RECOGNIZE</c></summary>
	MatchRecognize,
	/// <summary><c>MATCHES</c></summary>
	Matches,
	/// <summary><c>MAX</c></summary>
	Max,
	/// <summary><c>MEMBER</c></summary>
	Member,
	/// <summary><c>MERGE</c></summary>
	Merge,
	/// <summary><c>METHOD</c></summary>
	Method,
	/// <summary><c>MIN</c></summary>
	Min,
	/// <summary><c>MINUTE</c></summary>
	Minute,
	/// <summary><c>MOD</c></summary>
	Mod,
	/// <summary><c>MODIFIES</c></summary>
	Modifies,
	/// <summary><c>MODULE</c></summary>
	Module,
	/// <summary><c>MONTH</c></summary>
	Month,
	/// <summary><c>MULTISET</c></summary>
	Multiset,
	/// <summary><c>NATIONAL</c></summary>
	National,
	/// <summary><c>NATURAL</c></summary>
	Natural,
	/// <summary><c>NCHAR</c></summary>
	Nchar,
	/// <summary><c>NCLOB</c></summary>
	Nclob,
	/// <summary><c>NEW</c></summary>
	New,
	/// <summary><c>NO</c></summary>
	No,
	/// <summary><c>NONE</c></summary>
	None,
	/// <summary><c>NORMALIZE</c></summary>
	Normalize,
	/// <summary><c>NOT</c></summary>
	Not,
	/// <summary><c>NTH_VALUE</c></summary>
	NthValue,
	/// <summary><c>NTILE</c></summary>
	Ntile,
	/// <summary><c>NULL</c></summary>
	Null,
	/// <summary><c>NULLIF</c></summary>
	Nullif,
	/// <summary><c>NUMERIC</c></summary>
	Numeric,
	/// <summary><c>OCCURRENCES_REGEX</c></summary>
	OccurrencesRegex,
	/// <summary><c>OCTET_LENGTH</c></summary>
	OctetLength,
	/// <summary><c>OF</c></summary>
	Of,
	/// <summary><c>OFFSET</c></summary>
	Offset,
	/// <summary><c>OLD</c></summary>
	Old,
	/// <summary><c>OMIT</c></summary>
	Omit,
	/// <summary><c>ON</c></summary>
	On,
	/// <summary><c>ONE</c></summary>
	One,
	/// <summary><c>ONLY</c></summary>
	Only,
	/// <summary><c>OPEN</c></summary>
	Open,
	/// <summary><c>OR</c></summary>
	Or,
	/// <summary><c>ORDER</c></summary>
	Order,
	/// <summary><c>OUT</c></summary>
	Out,
	/// <summary><c>OUTER</c></summary>
	Outer,
	/// <summary><c>OVER</c></summary>
	Over,
	/// <summary><c>OVERLAPS</c></summary>
	Overlaps,
	/// <summary><c>OVERLAY</c></summary>
	Overlay,
	/// <summary><c>PARAMETER</c></summary>
	Parameter,
	/// <summary><c>PARTITION</c></summary>
	Partition,
	/// <summary><c>PATTERN</c></summary>
	Pattern,
	/// <summary><c>PER</c></summary>
	Per,
	/// <summary><c>PERCENT</c></summary>
	Percent,
	/// <summary><c>PERCENT_RANK</c></summary>
	PercentRank,
	/// <summary><c>PERCENTILE_CONT</c></summary>
	PercentileCont,
	/// <summary><c>PERCENTILE_DISC</c></summary>
	PercentileDisc,
	/// <summary><c>PERIOD</c></summary>
	Period,
	/// <summary><c>PORTION</c></summary>
	Portion,
	/// <summary><c>POSITION</c></summary>
	Position,
	/// <summary><c>POSITION_REGEX</c></summary>
	PositionRegex,
	/// <summary><c>POWER</c></summary>
	Power,
	/// <summary><c>PRECEDES</c></summary>
	Precedes,
	/// <summary><c>PRECISION</c></summary>
	Precision,
	/// <summary><c>PREPARE</c></summary>
	Prepare,
	/// <summary><c>PRIMARY</c></summary>
	Primary,
	/// <summary><c>PROCEDURE</c></summary>
	Procedure,
	/// <summary><c>PTF</c></summary>
	Ptf,
	/// <summary><c>RANGE</c></summary>
	Range,
	/// <summary><c>RANK</c></summary>
	Rank,
	/// <summary><c>READS</c></summary>
	Reads,
	/// <summary><c>REAL</c></summary>
	Real,
	/// <summary><c>RECURSIVE</c></summary>
	Recursive,
	/// <summary><c>REF</c></summary>
	Ref,
	/// <summary><c>REFERENCES</c></summary>
	References,
	/// <summary><c>REFERENCING</c></summary>
	Referencing,
	/// <summary><c>REGR_AVGX</c></summary>
	RegrAvgx,
	/// <summary><c>REGR_AVGY</c></summary>
	RegrAvgy,
	/// <summary><c>REGR_COUNT</c></summary>
	RegrCount,
	/// <summary><c>REGR_INTERCEPT</c></summary>
	RegrIntercept,
	/// <summary><c>REGR_R2</c></summary>
	RegrR2,
	/// <summary><c>REGR_SLOPE</c></summary>
	RegrSlope,
	/// <summary><c>REGR_SXX</c></summary>
	RegrSxx,
	/// <summary><c>REGR_SXY</c></summary>
	RegrSxy,
	/// <summary><c>REGR_SYY</c></summary>
	RegrSyy,
	/// <summary><c>RELEASE</c></summary>
	Release,
	/// <summary><c>RESULT</c></summary>
	Result,
	/// <summary><c>RETURN</c></summary>
	Return,
	/// <summary><c>RETURNS</c></summary>
	Returns,
	/// <summary><c>REVOKE</c></summary>
	Revoke,
	/// <summary><c>RIGHT</c></summary>
	Right,
	/// <summary><c>ROLLBACK</c></summary>
	Rollback,
	/// <summary><c>ROLLUP</c></summary>
	Rollup,
	/// <summary><c>ROW</c></summary>
	Row,
	/// <summary><c>ROW_NUMBER</c></summary>
	RowNumber,
	/// <summary><c>ROWS</c></summary>
	Rows,
	/// <summary><c>RPAD</c></summary>
	Rpad,
	/// <summary><c>RTRIM</c></summary>
	Rtrim,
	/// <summary><c>RUNNING</c></summary>
	Running,
	/// <summary><c>SAVEPOINT</c></summary>
	Savepoint,
	/// <summary><c>SCOPE</c></summary>
	Scope,
	/// <summary><c>SCROLL</c></summary>
	Scroll,
	/// <summary><c>SEARCH</c></summary>
	Search,
	/// <summary><c>SECOND</c></summary>
	Second,
	/// <summary><c>SEEK</c></summary>
	Seek,
	/// <summary><c>SELECT</c></summary>
	Select,
	/// <summary><c>SENSITIVE</c></summary>
	Sensitive,
	/// <summary><c>SESSION_USER</c></summary>
	SessionUser,
	/// <summary><c>SET</c></summary>
	Set,
	/// <summary><c>SHOW</c></summary>
	Show,
	/// <summary><c>SIMILAR</c></summary>
	Similar,
	/// <summary><c>SIN</c></summary>
	Sin,
	/// <summary><c>SINH</c></summary>
	Sinh,
	/// <summary><c>SKIP</c></summary>
	Skip,
	/// <summary><c>SMALLINT</c></summary>
	Smallint,
	/// <summary><c>SOME</c></summary>
	Some,
	/// <summary><c>SPECIFIC</c></summary>
	Specific,
	/// <summary><c>SPECIFICTYPE</c></summary>
	Specifictype,
	/// <summary><c>SQL</c></summary>
	Sql,
	/// <summary><c>SQLEXCEPTION</c></summary>
	Sqlexception,
	/// <summary><c>SQLSTATE</c></summary>
	Sqlstate,
	/// <summary><c>SQLWARNING</c></summary>
	Sqlwarning,
	/// <summary><c>SQRT</c></summary>
	Sqrt,
	/// <summary><c>START</c></summary>
	Start,
	/// <summary><c>STATIC</c></summary>
	Static,
	/// <summary><c>STDDEV_POP</c></summary>
	StddevPop,
	/// <summary><c>STDDEV_SAMP</c></summary>
	StddevSamp,
	/// <summary><c>SUBMULTISET</c></summary>
	Submultiset,
	/// <summary><c>SUBSET</c></summary>
	Subset,
	/// <summary><c>SUBSTRING</c></summary>
	Substring,
	/// <summary><c>SUBSTRING_REGEX</c></summary>
	SubstringRegex,
	/// <summary><c>SUCCEEDS</c></summary>
	Succeeds,
	/// <summary><c>SUM</c></summary>
	Sum,
	/// <summary><c>SYMMETRIC</c></summary>
	Symmetric,
	/// <summary><c>SYSTEM</c></summary>
	System,
	/// <summary><c>SYSTEM_TIME</c></summary>
	SystemTime,
	/// <summary><c>SYSTEM_USER</c></summary>
	SystemUser,
	/// <summary><c>TABLE</c></summary>
	Table,
	/// <summary><c>TABLESAMPLE</c></summary>
	Tablesample,
	/// <summary><c>TAN</c></summary>
	Tan,
	/// <summary><c>TANH</c></summary>
	Tanh,
	/// <summary><c>THEN</c></summary>
	Then,
	/// <summary><c>TIME</c></summary>
	Time,
	/// <summary><c>TIMESTAMP</c></summary>
	Timestamp,
	/// <summary><c>TIMEZONE_HOUR</c></summary>
	TimezoneHour,
	/// <summary><c>TIMEZONE_MINUTE</c></summary>
	TimezoneMinute,
	/// <summary><c>TO</c></summary>
	To,
	/// <summary><c>TRAILING</c></summary>
	Trailing,
	/// <summary><c>TRANSLATE</c></summary>
	Translate,
	/// <summary><c>TRANSLATE_REGEX</c></summary>
	TranslateRegex,
	/// <summary><c>TRANSLATION</c></summary>
	Translation,
	/// <summary><c>TREAT</c></summary>
	Treat,
	/// <summary><c>TRIGGER</c></summary>
	Trigger,
	/// <summary><c>TRIM</c></summary>
	Trim,
	/// <summary><c>TRIM_ARRAY</c></summary>
	TrimArray,
	/// <summary><c>TRUE</c></summary>
	True,
	/// <summary><c>TRUNCATE</c></summary>
	Truncate,
	/// <summary><c>UESCAPE</c></summary>
	Uescape,
	/// <summary><c>UNION</c></summary>
	Union,
	/// <summary><c>UNIQUE</c></summary>
	Unique,
	/// <summary><c>UNKNOWN</c></summary>
	Unknown,
	/// <summary><c>UNNEST</c></summary>
	Unnest,
	/// <summary><c>UPDATE</c></summary>
	Update,
	/// <summary><c>UPPER</c></summary>
	Upper,
	/// <summary><c>USER</c></summary>
	User,
	/// <summary><c>USING</c></summary>
	Using,
	/// <summary><c>VALUE</c></summary>
	Value,
	/// <summary><c>VALUES</c></summary>
	Values,
	/// <summary><c>VALUE_OF</c></summary>
	ValueOf,
	/// <summary><c>VAR_POP</c></summary>
	VarPop,
	/// <summary><c>VAR_SAMP</c></summary>
	VarSamp,
	/// <summary><c>VARBINARY</c></summary>
	Varbinary,
	/// <summary><c>VARCHAR</c></summary>
	Varchar,
	/// <summary><c>VARYING</c></summary>
	Varying,
	/// <summary><c>VERSIONING</c></summary>
	Versioning,
	/// <summary><c>WHEN</c></summary>
	When,
	/// <summary><c>WHENEVER</c></summary>
	Whenever,
	/// <summary><c>WHERE</c></summary>
	Where,
	/// <summary><c>WIDTH_BUCKET</c></summary>
	WidthBucket,
	/// <summary><c>WINDOW</c></summary>
	Window,
	/// <summary><c>WITH</c></summary>
	With,
	/// <summary><c>WITHIN</c></summary>
	Within,
	/// <summary><c>WITHOUT</c></summary>
	Without,
	/// <summary><c>YEAR</c></summary>
	Year,
}
