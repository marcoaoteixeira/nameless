using Nameless.AspNetCore.Identity.Models;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.AspNetCore.Identity.NHibernate.Mappings {

    public sealed class UserClaimClassMapping : ClassMapping<UserClaim> {

        #region Public Constructors

        public UserClaimClassMapping() {
            Table("identity_user_claims");

            Id(_ => _.Id, mapping => {
                mapping.Column("id");
                mapping.Type(NHibernateUtil.Int32);
                mapping.Generator(Generators.Identity);
            });

            Property(_ => _.UserId, mapping => {
                mapping.Column("user_id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.NotNullable(true);
                mapping.Index("idx_identity_user_claims_user_id");
            });

            Property(_ => _.ClaimType, mapping => {
                mapping.Column("claim_type");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(512);
                mapping.Index("idx_identity_user_claims_claim_type");
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
