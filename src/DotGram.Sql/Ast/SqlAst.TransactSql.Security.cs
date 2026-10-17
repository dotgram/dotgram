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

	/// <summary>
	/// T-SQL: CREATE ASYMMETRIC KEY (Transact-SQL). <c>CREATE ASYMMETRIC KEY name [AUTHORIZATION owner] { FROM source | WITH
	/// ALGORITHM = a } [ENCRYPTION BY PASSWORD = 'p']</c>.
	/// </summary>
	public sealed record CreateAsymmetricKey(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE ASYMMETRIC KEY (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }

		/// <summary>T-SQL: CREATE ASYMMETRIC KEY (Transact-SQL). Where the key is read from; null for a key made here.</summary>
		public AsymmetricKeySource? Source { get; init; }

		/// <summary>
		/// T-SQL: CREATE ASYMMETRIC KEY (Transact-SQL). The <c>WITH</c> settings: <c>ALGORITHM</c> of a key made here, and
		/// of a key a provider makes also <c>PROVIDER_KEY_NAME</c> and <c>CREATION_DISPOSITION</c>.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: CREATE ASYMMETRIC KEY (Transact-SQL). <c>ENCRYPTION BY PASSWORD = 'p'</c>; null where none is written.</summary>
		public KeyEncryptor.Password? EncryptedBy { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER ASYMMETRIC KEY (Transact-SQL). <c>ALTER ASYMMETRIC KEY name { REMOVE PRIVATE KEY | WITH PRIVATE KEY (…) |
	/// ATTESTED BY 'a' | REMOVE ATTESTED OPTION }</c>.
	/// </summary>
	public sealed record AlterAsymmetricKey(Identifier Name, AsymmetricKeyChange Change) : Statement;

	/// <summary>
	/// T-SQL: CREATE SYMMETRIC KEY (Transact-SQL). <c>CREATE SYMMETRIC KEY name [AUTHORIZATION owner] { FROM PROVIDER p [WITH
	/// settings] | WITH settings } [ENCRYPTION BY encryptor, …]</c>.
	/// </summary>
	public sealed record CreateSymmetricKey(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE SYMMETRIC KEY (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }

		/// <summary>T-SQL: CREATE SYMMETRIC KEY (Transact-SQL). <c>FROM PROVIDER p</c>; null for a key made here.</summary>
		public Identifier? Provider { get; init; }

		/// <summary>
		/// T-SQL: CREATE SYMMETRIC KEY (Transact-SQL). The <c>WITH</c> settings: <c>KEY_SOURCE</c>, <c>ALGORITHM</c>,
		/// <c>IDENTITY_VALUE</c>, <c>PROVIDER_KEY_NAME</c> and <c>CREATION_DISPOSITION</c>.
		/// </summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: CREATE SYMMETRIC KEY (Transact-SQL). <c>ENCRYPTION BY</c> what locks the key, one at least for a key made here.</summary>
		public SqlList<KeyEncryptor> EncryptedBy { get; init; } = [];
	}

	/// <summary>
	/// T-SQL: ALTER SYMMETRIC KEY (Transact-SQL). <c>ALTER SYMMETRIC KEY name { ADD | DROP } ENCRYPTION BY encryptor, …</c>.
	/// </summary>
	public sealed record AlterSymmetricKey(Identifier Name, SymmetricKeyChange Change) : Statement;

	/// <summary>
	/// T-SQL: CREATE CERTIFICATE (Transact-SQL). <c>CREATE CERTIFICATE name [AUTHORIZATION owner] { FROM source | [ENCRYPTION BY
	/// PASSWORD = 'p'] WITH settings } [ACTIVE FOR BEGIN_DIALOG = { ON | OFF }]</c>.
	/// </summary>
	public sealed record CreateCertificate(Identifier Name) : Statement
	{
		/// <summary>T-SQL: CREATE CERTIFICATE (Transact-SQL). <c>AUTHORIZATION owner</c>; null where none is written.</summary>
		public Identifier? Authorization { get; init; }

		/// <summary>T-SQL: CREATE CERTIFICATE (Transact-SQL). Where the certificate is read from; null for one made here.</summary>
		public CertificateSource? Source { get; init; }

		/// <summary>T-SQL: CREATE CERTIFICATE (Transact-SQL). The <c>WITH</c> settings of a certificate made here: <c>SUBJECT</c>, <c>START_DATE</c> and <c>EXPIRY_DATE</c>.</summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: CREATE CERTIFICATE (Transact-SQL). <c>ENCRYPTION BY PASSWORD = 'p'</c>, before the settings of a certificate made here.</summary>
		public KeyEncryptor.Password? EncryptedBy { get; init; }

		/// <summary>T-SQL: CREATE CERTIFICATE (Transact-SQL). <c>ACTIVE FOR BEGIN_DIALOG = ON</c> (true) or <c>OFF</c> (false); null where it is not written.</summary>
		public bool? ActiveForBeginDialog { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER CERTIFICATE (Transact-SQL). <c>ALTER CERTIFICATE name { REMOVE PRIVATE KEY | WITH PRIVATE KEY (…) | WITH
	/// ACTIVE FOR BEGIN_DIALOG = { ON | OFF } | ATTESTED BY 'a' | REMOVE ATTESTED OPTION }</c>.
	/// </summary>
	public sealed record AlterCertificate(Identifier Name, CertificateChange Change) : Statement;

	/// <summary>
	/// T-SQL: CREATE MASTER KEY (Transact-SQL). <c>CREATE MASTER KEY [ENCRYPTION BY PASSWORD = 'p']</c>.
	/// </summary>
	public sealed record CreateMasterKey : Statement
	{
		/// <summary>T-SQL: CREATE MASTER KEY (Transact-SQL). <c>ENCRYPTION BY PASSWORD = 'p'</c>; null where none is written.</summary>
		public KeyEncryptor.Password? EncryptedBy { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER MASTER KEY (Transact-SQL). <c>ALTER MASTER KEY { [FORCE] REGENERATE WITH ENCRYPTION BY PASSWORD = 'p' | { ADD
	/// | DROP } ENCRYPTION BY { SERVICE MASTER KEY | PASSWORD = 'p' } }</c>.
	/// </summary>
	public sealed record AlterMasterKey(MasterKeyChange Change) : Statement;

	/// <summary>
	/// T-SQL: ALTER SERVICE MASTER KEY (Transact-SQL). <c>ALTER SERVICE MASTER KEY { [FORCE] REGENERATE | WITH OLD_ACCOUNT = 'a',
	/// OLD_PASSWORD = 'p' | WITH NEW_ACCOUNT = 'a', NEW_PASSWORD = 'p' }</c>.
	/// </summary>
	public sealed record AlterServiceMasterKey(ServiceMasterKeyChange Change) : Statement;

	/// <summary>
	/// T-SQL: CREATE DATABASE ENCRYPTION KEY (Transact-SQL). <c>CREATE DATABASE ENCRYPTION KEY WITH ALGORITHM = a ENCRYPTION BY
	/// SERVER { CERTIFICATE | ASYMMETRIC KEY } name</c>: <paramref name="By"/> is the server's certificate or asymmetric key, and
	/// <c>SERVER</c> is always written before it.
	/// </summary>
	public sealed record CreateDatabaseEncryptionKey(DatabaseKeyAlgorithm Algorithm, KeyEncryptor By) : Statement;

	/// <summary>
	/// T-SQL: ALTER DATABASE ENCRYPTION KEY (Transact-SQL). <c>ALTER DATABASE ENCRYPTION KEY { REGENERATE WITH ALGORITHM = a |
	/// ENCRYPTION BY SERVER { CERTIFICATE | ASYMMETRIC KEY } name }</c>.
	/// </summary>
	public sealed record AlterDatabaseEncryptionKey(DatabaseEncryptionKeyChange Change) : Statement;

	/// <summary>
	/// T-SQL: CREATE COLUMN ENCRYPTION KEY (Transact-SQL). <c>CREATE COLUMN ENCRYPTION KEY name WITH VALUES (…), (…)</c>: one value
	/// at least.
	/// </summary>
	public sealed record CreateColumnEncryptionKey(Identifier Name, SqlList<ColumnKeyValue> Values) : Statement;

	/// <summary>
	/// T-SQL: ALTER COLUMN ENCRYPTION KEY (Transact-SQL). <c>ALTER COLUMN ENCRYPTION KEY name { ADD VALUE (…) | DROP VALUE (…) }</c>.
	/// </summary>
	public sealed record AlterColumnEncryptionKey(Identifier Name, ColumnKeyChange Change) : Statement;

	/// <summary>
	/// T-SQL: CREATE COLUMN MASTER KEY (Transact-SQL). <c>CREATE COLUMN MASTER KEY name WITH (KEY_STORE_PROVIDER_NAME = 'p',
	/// KEY_PATH = 'x' [, ENCLAVE_COMPUTATIONS (SIGNATURE = 0x…)])</c>.
	/// </summary>
	public sealed record CreateColumnMasterKey(Identifier Name, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: CREATE CREDENTIAL (Transact-SQL). <c>CREATE CREDENTIAL name WITH IDENTITY = 'i' [, SECRET = 's'] [FOR CRYPTOGRAPHIC
	/// PROVIDER p]</c>.
	/// </summary>
	public sealed record CreateCredential(Identifier Name, LiteralValue.String Identity) : Statement
	{
		/// <summary>T-SQL: CREATE CREDENTIAL (Transact-SQL). <c>SECRET = 's'</c>; null where none is written.</summary>
		public LiteralValue.String? Secret { get; init; }

		/// <summary>T-SQL: CREATE CREDENTIAL (Transact-SQL). <c>FOR CRYPTOGRAPHIC PROVIDER p</c>; null where none is written.</summary>
		public Identifier? Provider { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER CREDENTIAL (Transact-SQL). <c>ALTER CREDENTIAL name WITH IDENTITY = 'i' [, SECRET = 's']</c>.
	/// </summary>
	public sealed record AlterCredential(Identifier Name, LiteralValue.String Identity) : Statement
	{
		/// <summary>T-SQL: ALTER CREDENTIAL (Transact-SQL). <c>SECRET = 's'</c>; null where none is written.</summary>
		public LiteralValue.String? Secret { get; init; }

		/// <summary>T-SQL: ALTER CREDENTIAL (Transact-SQL). <c>FOR CRYPTOGRAPHIC PROVIDER p</c>, which the grammar reads here as in CREATE; null where none is written.</summary>
		public Identifier? Provider { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE DATABASE SCOPED CREDENTIAL (Transact-SQL). <c>CREATE DATABASE SCOPED CREDENTIAL name WITH IDENTITY = 'i' [,
	/// SECRET = 's']</c>.
	/// </summary>
	public sealed record CreateDatabaseScopedCredential(Identifier Name, LiteralValue.String Identity) : Statement
	{
		/// <summary>T-SQL: CREATE DATABASE SCOPED CREDENTIAL (Transact-SQL). <c>SECRET = 's'</c>; null where none is written.</summary>
		public LiteralValue.String? Secret { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER DATABASE SCOPED CREDENTIAL (Transact-SQL). <c>ALTER DATABASE SCOPED CREDENTIAL name WITH IDENTITY = 'i' [, SECRET
	/// = 's']</c>.
	/// </summary>
	public sealed record AlterDatabaseScopedCredential(Identifier Name, LiteralValue.String Identity) : Statement
	{
		/// <summary>T-SQL: ALTER DATABASE SCOPED CREDENTIAL (Transact-SQL). <c>SECRET = 's'</c>; null where none is written.</summary>
		public LiteralValue.String? Secret { get; init; }
	}

	/// <summary>
	/// T-SQL: CREATE SECURITY POLICY (Transact-SQL). <c>CREATE SECURITY POLICY name [predicate, …] [WITH (STATE = …, SCHEMABINDING =
	/// …)] [NOT FOR REPLICATION]</c>.
	/// </summary>
	public sealed record CreateSecurityPolicy(QualifiedName Name) : Statement
	{
		/// <summary>T-SQL: CREATE SECURITY POLICY (Transact-SQL). The predicates, <c>ADD FILTER PREDICATE f(c) ON t</c>, in the order written.</summary>
		public SqlList<PolicyPredicate> Predicates { get; init; } = [];

		/// <summary>T-SQL: CREATE SECURITY POLICY (Transact-SQL). <c>WITH (STATE = ON, SCHEMABINDING = OFF)</c>: the settings, in the order written.</summary>
		public SqlList<Option> Options { get; init; } = [];

		/// <summary>T-SQL: CREATE SECURITY POLICY (Transact-SQL). <c>NOT FOR REPLICATION</c>.</summary>
		public bool NotForReplication { get; init; }
	}

	/// <summary>
	/// T-SQL: ALTER SECURITY POLICY (Transact-SQL). <c>ALTER SECURITY POLICY name { predicate, … | WITH (STATE = …) | { ADD | DROP }
	/// NOT FOR REPLICATION }</c>.
	/// </summary>
	public sealed record AlterSecurityPolicy(QualifiedName Name, SecurityPolicyChange Change) : Statement;

	/// <summary>
	/// T-SQL: CREATE CRYPTOGRAPHIC PROVIDER (Transact-SQL). <c>CREATE CRYPTOGRAPHIC PROVIDER name FROM FILE = 'path'</c>.
	/// </summary>
	public sealed record CreateCryptographicProvider(Identifier Name, LiteralValue.String File) : Statement;

	/// <summary>
	/// T-SQL: ALTER CRYPTOGRAPHIC PROVIDER (Transact-SQL). <c>ALTER CRYPTOGRAPHIC PROVIDER name { ENABLE | DISABLE | FROM FILE = 'path' }</c>.
	/// </summary>
	public sealed record AlterCryptographicProvider(Identifier Name, CryptographicProviderChange Change) : Statement;

	/// <summary>
	/// T-SQL: OPEN SYMMETRIC KEY (Transact-SQL). <c>OPEN SYMMETRIC KEY name DECRYPTION BY decryptor</c>: a certificate or an
	/// asymmetric key, with its password, a symmetric key, or a password.
	/// </summary>
	public sealed record OpenSymmetricKey(Identifier Name, KeyEncryptor DecryptedBy) : Statement;

	/// <summary>T-SQL: OPEN MASTER KEY (Transact-SQL). <c>OPEN MASTER KEY DECRYPTION BY PASSWORD = 'p'</c>.</summary>
	public sealed record OpenMasterKey(KeyEncryptor.Password DecryptedBy) : Statement;

	/// <summary>T-SQL: CLOSE SYMMETRIC KEY (Transact-SQL). <c>CLOSE SYMMETRIC KEY name</c>.</summary>
	public sealed record CloseSymmetricKey(Identifier Name) : Statement;

	/// <summary>T-SQL: CLOSE SYMMETRIC KEY (Transact-SQL). <c>CLOSE ALL SYMMETRIC KEYS</c>.</summary>
	public sealed record CloseAllSymmetricKeys : Statement;

	/// <summary>T-SQL: CLOSE MASTER KEY (Transact-SQL). <c>CLOSE MASTER KEY</c>.</summary>
	public sealed record CloseMasterKey : Statement;

	/// <summary>
	/// T-SQL: ADD SENSITIVITY CLASSIFICATION (Transact-SQL). <c>ADD SENSITIVITY CLASSIFICATION TO column, … WITH (LABEL = 'l', …)</c>.
	/// </summary>
	public sealed record AddSensitivityClassification(SqlList<QualifiedName> Columns, SqlList<Option> Options) : Statement;

	/// <summary>
	/// T-SQL: ADD SIGNATURE (Transact-SQL). <c>ADD [COUNTER] SIGNATURE TO [class ::] module BY signer, …</c>.
	/// </summary>
	public sealed record AddSignature(bool Counter, QualifiedName Module, SqlList<SignedBy> By) : Statement
	{
		/// <summary>T-SQL: ADD SIGNATURE (Transact-SQL). The class written before the module, <c>OBJECT::</c>, <c>ASSEMBLY::</c> or <c>DATABASE::</c>; null where the module is named alone.</summary>
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

/// <summary>T-SQL: CREATE SYMMETRIC KEY (Transact-SQL), OPEN SYMMETRIC KEY (Transact-SQL), CREATE MASTER KEY (Transact-SQL) and the other key
/// statements. What a key is locked with, or opened by: a password, a certificate, a symmetric key or an asymmetric key. The
/// direction, <c>ENCRYPTION BY</c> or <c>DECRYPTION BY</c>, is the place's.</summary>
public abstract record KeyEncryptor : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>PASSWORD = 'p'</c>: a string, never a variable.</summary>
	public sealed record Password(LiteralValue.String Value) : KeyEncryptor;

	/// <summary><c>CERTIFICATE c</c>.</summary>
	public sealed record Certificate(QualifiedName Name) : KeyEncryptor
	{
		/// <summary>T-SQL: OPEN SYMMETRIC KEY (Transact-SQL). <c>WITH PASSWORD = 'p'</c> after a certificate that opens a key; null where none is written.</summary>
		public LiteralValue.String? WithPassword { get; init; }
	}

	/// <summary><c>SYMMETRIC KEY k</c>.</summary>
	public sealed record SymmetricKey(QualifiedName Name) : KeyEncryptor;

	/// <summary><c>ASYMMETRIC KEY k</c>.</summary>
	public sealed record AsymmetricKey(QualifiedName Name) : KeyEncryptor
	{
		/// <summary>T-SQL: OPEN SYMMETRIC KEY (Transact-SQL). <c>WITH PASSWORD = 'p'</c> after an asymmetric key that opens a key; null where none is written.</summary>
		public LiteralValue.String? WithPassword { get; init; }
	}

	/// <summary>T-SQL: ALTER MASTER KEY (Transact-SQL). <c>SERVICE MASTER KEY</c>.</summary>
	public sealed record ServiceMasterKey : KeyEncryptor;
}

/// <summary>T-SQL: CREATE ASYMMETRIC KEY (Transact-SQL). Where an asymmetric key is read from.</summary>
public abstract record AsymmetricKeySource : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>FROM PROVIDER p</c>. Its settings are the statement's <see cref="Statement.CreateAsymmetricKey.Options"/>.</summary>
	public sealed record Provider(Identifier Name) : AsymmetricKeySource;

	/// <summary><c>FROM [EXECUTABLE] FILE = 'path'</c>.</summary>
	public sealed record File(LiteralValue.String Path) : AsymmetricKeySource
	{
		/// <summary><c>EXECUTABLE</c> before <c>FILE</c>: the file is an executable, which holds the key's public part.</summary>
		public bool Executable { get; init; }
	}

	/// <summary><c>FROM ASSEMBLY name</c>.</summary>
	public sealed record Assembly(QualifiedName Name) : AsymmetricKeySource;
}

/// <summary>T-SQL: CREATE CERTIFICATE (Transact-SQL). Where a certificate is read from.</summary>
public abstract record CertificateSource : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>FROM ASSEMBLY name</c>.</summary>
	public sealed record Assembly(QualifiedName Name) : CertificateSource;

	/// <summary><c>FROM [EXECUTABLE] FILE = 'path' [WITH [FORMAT = 'f',] PRIVATE KEY (…)]</c>.</summary>
	public sealed record File(LiteralValue.String Path) : CertificateSource
	{
		/// <summary><c>EXECUTABLE</c> before <c>FILE</c>: the file is an executable, which holds the certificate.</summary>
		public bool Executable { get; init; }

		/// <summary><c>FORMAT = 'f'</c>, before the private key; null where none is written.</summary>
		public LiteralValue.String? Format { get; init; }

		/// <summary><c>PRIVATE KEY (FILE = 'k', DECRYPTION BY PASSWORD = 'p', …)</c>: its settings; empty where no private key is written.</summary>
		public SqlList<Option> PrivateKey { get; init; } = [];
	}

	/// <summary><c>FROM BINARY = 0x… [WITH PRIVATE KEY (…)]</c>.</summary>
	public sealed record Binary(LiteralValue.Binary Value) : CertificateSource
	{
		/// <summary><c>PRIVATE KEY (BINARY = 0x…, DECRYPTION BY PASSWORD = 'p', …)</c>: its settings; empty where no private key is written.</summary>
		public SqlList<Option> PrivateKey { get; init; } = [];
	}
}

/// <summary>T-SQL: ALTER ASYMMETRIC KEY (Transact-SQL). What is done to the key.</summary>
public abstract record AsymmetricKeyChange : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>REMOVE PRIVATE KEY</c>.</summary>
	public sealed record RemovePrivateKey : AsymmetricKeyChange;

	/// <summary><c>WITH PRIVATE KEY (DECRYPTION BY PASSWORD = 'p', ENCRYPTION BY PASSWORD = 'q')</c>: its settings, one at least.</summary>
	public sealed record WithPrivateKey(SqlList<Option> Options) : AsymmetricKeyChange;

	/// <summary><c>ATTESTED BY 'a'</c>.</summary>
	public sealed record AttestedBy(LiteralValue.String Attestation) : AsymmetricKeyChange;

	/// <summary><c>REMOVE ATTESTED OPTION</c>.</summary>
	public sealed record RemoveAttestedOption : AsymmetricKeyChange;
}

/// <summary>T-SQL: ALTER CERTIFICATE (Transact-SQL). What is done to the certificate.</summary>
public abstract record CertificateChange : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>REMOVE PRIVATE KEY</c>.</summary>
	public sealed record RemovePrivateKey : CertificateChange;

	/// <summary><c>WITH PRIVATE KEY (FILE = 'k', DECRYPTION BY PASSWORD = 'p', …)</c>: its settings, one at least.</summary>
	public sealed record WithPrivateKey(SqlList<Option> Options) : CertificateChange;

	/// <summary><c>WITH ACTIVE FOR BEGIN_DIALOG = ON</c> (true) or <c>OFF</c> (false).</summary>
	public sealed record ActiveForBeginDialog(bool On) : CertificateChange;

	/// <summary><c>ATTESTED BY 'a'</c>.</summary>
	public sealed record AttestedBy(LiteralValue.String Attestation) : CertificateChange;

	/// <summary><c>REMOVE ATTESTED OPTION</c>.</summary>
	public sealed record RemoveAttestedOption : CertificateChange;
}

/// <summary>T-SQL: ALTER SYMMETRIC KEY (Transact-SQL). What is done to the key.</summary>
public abstract record SymmetricKeyChange : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>ADD ENCRYPTION BY encryptor, …</c>: one at least.</summary>
	public sealed record AddEncryption(SqlList<KeyEncryptor> By) : SymmetricKeyChange;

	/// <summary><c>DROP ENCRYPTION BY encryptor, …</c>: one at least.</summary>
	public sealed record DropEncryption(SqlList<KeyEncryptor> By) : SymmetricKeyChange;
}

/// <summary>T-SQL: ALTER MASTER KEY (Transact-SQL). What is done to the key.</summary>
public abstract record MasterKeyChange : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>[FORCE] REGENERATE WITH ENCRYPTION BY PASSWORD = 'p'</c>.</summary>
	public sealed record Regenerate(KeyEncryptor.Password By) : MasterKeyChange
	{
		/// <summary><c>FORCE</c> before <c>REGENERATE</c>.</summary>
		public bool Force { get; init; }
	}

	/// <summary><c>ADD ENCRYPTION BY { SERVICE MASTER KEY | PASSWORD = 'p' }</c>.</summary>
	public sealed record AddEncryption(KeyEncryptor By) : MasterKeyChange;

	/// <summary><c>DROP ENCRYPTION BY { SERVICE MASTER KEY | PASSWORD = 'p' }</c>.</summary>
	public sealed record DropEncryption(KeyEncryptor By) : MasterKeyChange;
}

/// <summary>T-SQL: ALTER SERVICE MASTER KEY (Transact-SQL). What is done to the key.</summary>
public abstract record ServiceMasterKeyChange : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>[FORCE] REGENERATE</c>.</summary>
	public sealed record Regenerate : ServiceMasterKeyChange
	{
		/// <summary><c>FORCE</c> before <c>REGENERATE</c>.</summary>
		public bool Force { get; init; }
	}

	/// <summary><c>WITH OLD_ACCOUNT = 'a', OLD_PASSWORD = 'p'</c>.</summary>
	public sealed record OldAccount(LiteralValue.String Account, LiteralValue.String Password) : ServiceMasterKeyChange;

	/// <summary><c>WITH NEW_ACCOUNT = 'a', NEW_PASSWORD = 'p'</c>.</summary>
	public sealed record NewAccount(LiteralValue.String Account, LiteralValue.String Password) : ServiceMasterKeyChange;
}

/// <summary>T-SQL: ALTER DATABASE ENCRYPTION KEY (Transact-SQL). What is done to the key.</summary>
public abstract record DatabaseEncryptionKeyChange : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>REGENERATE WITH ALGORITHM = a</c>.</summary>
	public sealed record Regenerate(DatabaseKeyAlgorithm Algorithm) : DatabaseEncryptionKeyChange;

	/// <summary>
	/// <c>ENCRYPTION BY SERVER { CERTIFICATE | ASYMMETRIC KEY } name</c>: <paramref name="By"/> is the server's certificate or
	/// asymmetric key, and <c>SERVER</c> is always written before it.
	/// </summary>
	public sealed record EncryptedBy(KeyEncryptor By) : DatabaseEncryptionKeyChange;
}

/// <summary>T-SQL: CREATE DATABASE ENCRYPTION KEY (Transact-SQL). The algorithm the database encryption key uses.</summary>
public enum DatabaseKeyAlgorithm { Aes128, Aes192, Aes256, TripleDes3Key }

/// <summary>
/// T-SQL: CREATE COLUMN ENCRYPTION KEY (Transact-SQL), ALTER COLUMN ENCRYPTION KEY (Transact-SQL). One value of the key, in brackets:
/// <c>(COLUMN_MASTER_KEY = m, ALGORITHM = 'RSA_OAEP', ENCRYPTED_VALUE = 0x…)</c>, the settings in the order written.
/// </summary>
public sealed record ColumnKeyValue(SqlList<Option> Settings) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: ALTER COLUMN ENCRYPTION KEY (Transact-SQL). What is done to the key.</summary>
public abstract record ColumnKeyChange : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>ADD VALUE (…)</c>.</summary>
	public sealed record AddValue(ColumnKeyValue Value) : ColumnKeyChange;

	/// <summary><c>DROP VALUE (COLUMN_MASTER_KEY = m)</c>.</summary>
	public sealed record DropValue(ColumnKeyValue Value) : ColumnKeyChange;
}

/// <summary>T-SQL: ALTER CRYPTOGRAPHIC PROVIDER (Transact-SQL). What is done to the provider.</summary>
public abstract record CryptographicProviderChange : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>ENABLE</c>.</summary>
	public sealed record Enable : CryptographicProviderChange;

	/// <summary><c>DISABLE</c>.</summary>
	public sealed record Disable : CryptographicProviderChange;

	/// <summary><c>FROM FILE = 'path'</c>.</summary>
	public sealed record FromFile(LiteralValue.String Path) : CryptographicProviderChange;
}

/// <summary>T-SQL: ALTER SECURITY POLICY (Transact-SQL). What is done to the policy.</summary>
public abstract record SecurityPolicyChange : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary>The predicates, <c>ADD FILTER PREDICATE f(c) ON t, DROP BLOCK PREDICATE ON u</c>: one at least.</summary>
	public sealed record Predicates(SqlList<PolicyPredicate> Items) : SecurityPolicyChange;

	/// <summary><c>WITH (STATE = ON)</c> (true) or <c>OFF</c> (false).</summary>
	public sealed record State(bool On) : SecurityPolicyChange;

	/// <summary><c>ADD NOT FOR REPLICATION</c>.</summary>
	public sealed record AddNotForReplication : SecurityPolicyChange;

	/// <summary><c>DROP NOT FOR REPLICATION</c>.</summary>
	public sealed record DropNotForReplication : SecurityPolicyChange;
}

/// <summary>T-SQL: CREATE SECURITY POLICY (Transact-SQL), ALTER SECURITY POLICY (Transact-SQL). A predicate added, altered or dropped.</summary>
public abstract record PolicyPredicate : ISqlNode
{
	/// <summary>Where in the text this was written.</summary>
	SqlLocation _location;
	public SqlSpan Span => _location.Span;

	/// <summary>Records where this was written.</summary>
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}

	/// <summary><c>ADD FILTER PREDICATE f(c) ON t</c>.</summary>
	public sealed record AddFilter(PolicyFunction Function, QualifiedName On) : PolicyPredicate;

	/// <summary><c>ALTER FILTER PREDICATE f(c) ON t</c>.</summary>
	public sealed record AlterFilter(PolicyFunction Function, QualifiedName On) : PolicyPredicate;

	/// <summary><c>DROP FILTER PREDICATE ON t</c>.</summary>
	public sealed record DropFilter(QualifiedName On) : PolicyPredicate;

	/// <summary><c>ADD BLOCK PREDICATE f(c) ON t [AFTER INSERT]</c>.</summary>
	public sealed record AddBlock(PolicyFunction Function, QualifiedName On, BlockOperation? When) : PolicyPredicate;

	/// <summary><c>ALTER BLOCK PREDICATE f(c) ON t [AFTER INSERT]</c>.</summary>
	public sealed record AlterBlock(PolicyFunction Function, QualifiedName On, BlockOperation? When) : PolicyPredicate;

	/// <summary><c>DROP BLOCK PREDICATE ON t [AFTER INSERT]</c>.</summary>
	public sealed record DropBlock(QualifiedName On, BlockOperation? When) : PolicyPredicate;
}

/// <summary>
/// T-SQL: CREATE SECURITY POLICY (Transact-SQL). The function of a predicate, <c>dbo.f(c, @@ROWCOUNT)</c>: a name and the values it is
/// called with, no variable among them.
/// </summary>
public sealed record PolicyFunction(QualifiedName Name, SqlList<Expression> Arguments) : ISqlNode { SqlLocation _location; Type EqualityContract => SqlLocation.Guard(GetType()); public SqlSpan Span => _location.Span;
	public void Locate(int at, int length)
	{
		_location = new SqlLocation(new SqlSpan(at, length));
	}
}

/// <summary>T-SQL: CREATE SECURITY POLICY (Transact-SQL). The change a block predicate blocks.</summary>
public enum BlockOperation { AfterInsert, AfterUpdate, BeforeUpdate, BeforeDelete }
