using System;

namespace DotGram.Sql.Ast;

// T-SQL's security family: schemas, roles, logins and users, and the keys, certificates and credentials that
// lock things. A setting that is a name and a value is an Option (proposal 20); a place that comes from one of
// several sources is a small family named after the grammar's words.

public abstract partial record Statement
{
	public partial record CreateRole
	{
		/// <summary>
		/// T-SQL: CREATE ROLE (Transact-SQL). <c>AUTHORIZATION owner_name</c>: the owner of the role; null where none is
		/// written. The standard's <c>WITH ADMIN</c> is <see cref="Admin"/>, which T-SQL does not read.
		/// </summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER ROLE (Transact-SQL). <c>ALTER ROLE role_name { ADD MEMBER m | DROP MEMBER m | WITH NAME = new }</c>:
	/// one action, as the grammar's <c>RoleAction</c> reads it.
	/// </summary>
	public sealed record AlterRole(Identifier Name, RoleAction Action) : Statement;

	/// <summary>
	/// T-SQL: CREATE SERVER ROLE (Transact-SQL). <c>CREATE SERVER ROLE role_name [AUTHORIZATION server_principal]</c>.
	/// </summary>
	public sealed record CreateServerRole(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE SERVER ROLE (Transact-SQL). <c>AUTHORIZATION server_principal</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER SERVER ROLE (Transact-SQL). <c>ALTER SERVER ROLE name { ADD MEMBER m | DROP MEMBER m | WITH NAME = new }</c>.
	/// </summary>
	public sealed record AlterServerRole(Identifier Name, RoleAction Action) : Statement;

	/// <summary>
	/// T-SQL: CREATE APPLICATION ROLE (Transact-SQL). <c>CREATE APPLICATION ROLE name WITH PASSWORD = 'p' [, DEFAULT_SCHEMA = s]</c>:
	/// the settings, one at least.
	/// </summary>
	public sealed record CreateApplicationRole(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: ALTER APPLICATION ROLE (Transact-SQL). <c>ALTER APPLICATION ROLE name WITH NAME = n | PASSWORD = 'p' |
	/// DEFAULT_SCHEMA = s, …</c>: the settings, one at least.
	/// </summary>
	public sealed record AlterApplicationRole(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: CREATE LOGIN (Transact-SQL). <c>CREATE LOGIN name { WITH PASSWORD = … [, settings] | FROM source }</c>.
	/// </summary>
	public sealed record CreateLogin(Identifier Name, LoginSource Source) : Statement
	{
		/// <summary>
		/// T-SQL: CREATE LOGIN (Transact-SQL). The settings after the password (<c>, SID = 0x…</c>), or after
		/// <c>FROM WINDOWS WITH</c> and <c>FROM EXTERNAL PROVIDER WITH</c>; none for a certificate or an asymmetric key, whose
		/// one setting is its credential.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER LOGIN (Transact-SQL). <c>ALTER LOGIN name { ENABLE | DISABLE | WITH settings | ADD CREDENTIAL c | DROP
	/// CREDENTIAL c }</c>.
	/// </summary>
	public sealed record AlterLogin(Identifier Name, AlterLoginAction Action) : Statement;

	/// <summary>
	/// T-SQL: CREATE USER (Transact-SQL). <c>CREATE USER name [source] [WITH settings]</c>.
	/// </summary>
	public sealed record CreateUser(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE USER (Transact-SQL). Where the user comes from; null for a user of the database alone.</summary>
		public UserSource? Source { get; init; }

		/// <summary>T-SQL: CREATE USER (Transact-SQL). <c>WITH setting, …</c>; empty where it is not written.</summary>
		public SqlList<Option> Options { get; init; } = [];
	}

	/// <summary>T-SQL: ALTER USER (Transact-SQL). <c>ALTER USER name WITH setting, …</c>: the settings, one at least.</summary>
	public sealed record AlterUser(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: ALTER SCHEMA (Transact-SQL). <c>ALTER SCHEMA name TRANSFER [class ::] securable</c>.
	/// </summary>
	public sealed record AlterSchema(Identifier Name, QualifiedName Securable) : Statement
	{
		/// <summary>T-SQL: ALTER SCHEMA (Transact-SQL). The class written before the securable, <c>TYPE::</c>; null where none is.</summary>
		public SecurableClass? Class { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER AUTHORIZATION (Transact-SQL). <c>ALTER AUTHORIZATION ON [class ::] entity TO { principal | SCHEMA
	/// OWNER }</c>.
	/// </summary>
	public sealed record AlterAuthorization(QualifiedName Entity, AuthorizationTo To) : Statement
	{
		/// <summary>T-SQL: ALTER AUTHORIZATION (Transact-SQL). The class written before the entity, <c>OBJECT::</c>; null where none is.</summary>
		public SecurableClass? Class { get; init; }
	}
}

/// <summary>T-SQL: ALTER ROLE (Transact-SQL), ALTER SERVER ROLE (Transact-SQL). What is done to the role.</summary>
public abstract record RoleAction : ISqlNode
{
	/// <summary>Where in the text this action was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this action was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>ADD MEMBER principal</c>.</summary>
	public sealed record AddMember(Identifier Member) : RoleAction;

	/// <summary><c>DROP MEMBER principal</c>.</summary>
	public sealed record DropMember(Identifier Member) : RoleAction;

	/// <summary><c>WITH NAME = new_name</c>.</summary>
	public sealed record WithName(Identifier NewName) : RoleAction;
}

/// <summary>T-SQL: ALTER AUTHORIZATION (Transact-SQL). Whom the entity is given to.</summary>
public abstract record AuthorizationTo : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>TO principal_name</c>.</summary>
	public sealed record Principal(Identifier Name) : AuthorizationTo;

	/// <summary><c>TO SCHEMA OWNER</c>.</summary>
	public sealed record SchemaOwner : AuthorizationTo;
}

/// <summary>T-SQL: CREATE LOGIN (Transact-SQL). Where a login comes from, which also decides the settings it may have.</summary>
public abstract record LoginSource : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>
	/// <c>WITH PASSWORD = 'p' [HASHED] [MUST_CHANGE]</c>: the password, a string or, with <see cref="Hashed"/>, a hash in
	/// hex. The settings after it are the statement's <see cref="Statement.CreateLogin.Options"/>.
	/// </summary>
	public sealed record Password(Expression Value) : LoginSource
	{
		/// <summary><c>HASHED</c>, after the value: the value is a hash.</summary>
		public bool Hashed { get; init; }

		/// <summary><c>MUST_CHANGE</c>, after the value or after <c>HASHED</c>.</summary>
		public bool MustChange { get; init; }

		/// <summary><c>MUST_CHANGE HASHED</c>: the two words in the other order; set only where both are written.</summary>
		[Spelling] public bool MustChangeFirst { get; init; }
	}

	/// <summary><c>FROM WINDOWS</c>. Its settings, <c>WITH DEFAULT_DATABASE = d</c>, are the statement's.</summary>
	public sealed record Windows : LoginSource;

	/// <summary><c>FROM EXTERNAL PROVIDER</c>. Its settings, <c>WITH SID = 0x…</c>, are the statement's.</summary>
	public sealed record ExternalProvider : LoginSource;

	/// <summary><c>FROM CERTIFICATE c [WITH CREDENTIAL = cr]</c>.</summary>
	public sealed record Certificate(Identifier Name, Identifier? Credential) : LoginSource;

	/// <summary><c>FROM ASYMMETRIC KEY k [WITH CREDENTIAL = cr]</c>.</summary>
	public sealed record AsymmetricKey(Identifier Name, Identifier? Credential) : LoginSource;
}

/// <summary>T-SQL: ALTER LOGIN (Transact-SQL). What is done to the login.</summary>
public abstract record AlterLoginAction : ISqlNode
{
	/// <summary>Where in the text this action was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this action was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>ENABLE</c>.</summary>
	public sealed record Enable : AlterLoginAction;

	/// <summary><c>DISABLE</c>.</summary>
	public sealed record Disable : AlterLoginAction;

	/// <summary>
	/// <c>WITH setting, …</c>. A <c>PASSWORD</c> keeps the words after its value as its own options: <c>OLD_PASSWORD =
	/// 'q'</c>, or <c>HASHED</c>, <c>MUST_CHANGE</c> and <c>UNLOCK</c> in the order written.
	/// </summary>
	public sealed record With(SqlList<Option> Options) : AlterLoginAction;

	/// <summary><c>ADD CREDENTIAL credential_name</c>.</summary>
	public sealed record AddCredential(Identifier Name) : AlterLoginAction;

	/// <summary><c>DROP CREDENTIAL credential_name</c>.</summary>
	public sealed record DropCredential(Identifier Name) : AlterLoginAction;
}

/// <summary>T-SQL: CREATE USER (Transact-SQL). Where a user comes from.</summary>
public abstract record UserSource : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>{ FOR | FROM } LOGIN login_name</c>.</summary>
	public sealed record Login(Identifier Name) : UserSource
	{
		/// <summary><c>FROM</c> where <c>FOR</c> is the other word of the same clause.</summary>
		[Spelling] public bool From { get; init; }
	}

	/// <summary><c>{ FOR | FROM } CERTIFICATE cert_name</c>.</summary>
	public sealed record Certificate(Identifier Name) : UserSource
	{
		/// <summary><c>FROM</c> where <c>FOR</c> is the other word of the same clause.</summary>
		[Spelling] public bool From { get; init; }
	}

	/// <summary><c>{ FOR | FROM } ASYMMETRIC KEY key_name</c>.</summary>
	public sealed record AsymmetricKey(Identifier Name) : UserSource
	{
		/// <summary><c>FROM</c> where <c>FOR</c> is the other word of the same clause.</summary>
		[Spelling] public bool From { get; init; }
	}

	/// <summary><c>{ FOR | FROM } EXTERNAL PROVIDER</c>.</summary>
	public sealed record ExternalProvider : UserSource
	{
		/// <summary><c>FROM</c> where <c>FOR</c> is the other word of the same clause.</summary>
		[Spelling] public bool From { get; init; }
	}

	/// <summary><c>WITHOUT LOGIN</c>.</summary>
	public sealed record WithoutLogin : UserSource;
}
