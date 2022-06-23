using Nameless.AspNetCore.Identity.Models;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.AspNetCore.Identity.NHibernate.Mappings {

    public sealed class RoleClaimClassMapping : ClassMapping<RoleClaim> {

        #region Public Constructors

        public RoleClaimClassMapping() {
            Table("identity_role_claims");

            Id(_ => _.Id, mapping => {
                mapping.Column("id");
                mapping.Type(NHibernateUtil.Int32);
                mapping.Generator(Generators.Identity);
            });

            Property(_ => _.RoleId, mapping => {
                mapping.Column("role_id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.NotNullable(true);
                mapping.Index("idx_identity_role_claims_role_id");
            });

            Property(_ => _.ClaimType, mapping => {
                mapping.Column("claim_type");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(512);
                mapping.Index("idx_identity_role_claims_claim_type");
            });

            Property(_ => _.ClaimValue, mapping => {
                mapping.Column("claim_value");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(512);
            });
        }

        #endregion
    }
}
