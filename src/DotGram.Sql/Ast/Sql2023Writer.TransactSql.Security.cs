using System;

using DotGram.Sql.TransactSql;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// T-SQL's Security family: writes the nodes it owns and answers false for every other.
		/// </summary>
		bool TryPutSecurity(ISqlNode node)
		{
			return false;
		}

		/// <summary>
		/// What the standard's GRANT, REVOKE and the rest write for a statement, held to what T-SQL's grammar reads in the
		/// same place (<c>PermissionStatement</c>): the standard's arm calls it first, and in the standard's own reading
		/// it asks nothing.
		/// </summary>
		void CheckSecurity(ISqlNode node)
		{
			if (!transactSql)
				return;

			switch (node)
			{
				case Statement.Grant { Body: GrantBody.Privileges privileges } grant:
					CheckPermissions(grant, privileges.Items, privileges.Object, privileges.Grantees);

					if (privileges.HierarchyOption)
						throw Misplaced(grant, "HierarchyOption", "in T-SQL's GRANT, which reads only WITH GRANT OPTION");

					if (privileges.GrantedBy is not null)
						throw Misplaced(grant, "GrantedBy", "in T-SQL's GRANT, which reads only AS principal");

					break;

				case Statement.Grant grant:
					throw Misplaced(grant, "role names", "in T-SQL's GRANT, which grants permissions and reads no role (PermissionStatement)");

				case Statement.Deny deny:
					CheckPermissions(deny, deny.Items, deny.Object, deny.Grantees);
					break;

				case Statement.Revoke { Body: RevokeBody.Privileges revoked } revoke:
					CheckPermissions(revoke, revoked.Items, revoked.Object, revoked.Grantees);

					if (revoked.Option == RevokeOption.HierarchyOptionFor)
						throw Misplaced(revoke, "HierarchyOptionFor", "in T-SQL's REVOKE, which reads only GRANT OPTION FOR");

					if (revoked.GrantedBy is not null)
						throw Misplaced(revoke, "GrantedBy", "in T-SQL's REVOKE, which reads only AS principal");

					if (revoked.Behavior == DropBehavior.Restrict)
						throw Misplaced(revoke, "RESTRICT", "in T-SQL's REVOKE, which reads CASCADE or nothing");

					break;

				case Statement.Revoke revoke:
					throw Misplaced(revoke, "role names", "in T-SQL's REVOKE, which takes back permissions and reads no role (PermissionStatement)");
			}
		}

		/// <summary>
		/// The parts GRANT, DENY and REVOKE share in T-SQL: permissions the catalogue knows, of the securable's class, to
		/// principals, as <c>PermissionStatement</c> reads them.
		/// </summary>
		void CheckPermissions(ISqlNode statement, SqlList<Privilege> items, PrivilegeObject? on, SqlList<Grantee> grantees)
		{
			if (items.Count == 0)
				throw Empty(statement, "Items");

			if (grantees.Count == 0)
				throw Empty(statement, "Grantees");

			string? named = null;

			switch (on)
			{
				case null:
					break;

				// `ON TABLE t`, `ON DOMAIN d`, a routine's designator: the standard's words, which T-SQL's PermissionOn does not read.
				case PrivilegeObject.Named { Kind: not PrivilegeObjectKind.Table } or PrivilegeObject.Named { TableKeyword: true } or PrivilegeObject.Named { Routine: not null }:
					throw Misplaced(statement, "the standard's securable", "in T-SQL, whose ON reads a class and a name (PermissionOn)");

				// T-SQL reads a name with no class as OBJECT.
				case PrivilegeObject.Named:
					named = "OBJECT";
					break;

				case PrivilegeObject.Classed classed:
					named = PermissionWords.Text(classed.Class) ?? throw NoText(classed.Class);
					break;

				default:
					throw Unwritten(on);
			}

			foreach (var privilege in items)
			{
				if (privilege.Methods.Count > 0)
					throw Misplaced(privilege, "Methods", "in T-SQL, whose permission takes a column list and no routine (Permission)");

				// The engine reads the pair while parsing: `GRANT SELECT ON ENDPOINT::x` is Msg 102 (the guard of PermissionStatement).
				if (named is not null && !PermissionCatalogue.Allows(named, Spelled(privilege)))
					throw Misplaced(privilege, "Kind", $"on a {named}, whose permissions do not include it (Msg 102 at the permission)");
			}
		}
	}
}
