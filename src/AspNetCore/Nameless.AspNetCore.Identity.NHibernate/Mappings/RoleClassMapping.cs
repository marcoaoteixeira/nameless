using Nameless.AspNetCore.Identity.Models;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.AspNetCore.Identity.NHibernate.Mappings {

    public sealed class RoleClassMapping : ClassMapping<Role> {

        #region Public Constructors

        public RoleClassMapping() {
            Table("identity_roles");

            OptimisticLock(OptimisticLockMode.Dirty);
            DynamicUpdate(true);

            Id(_ => _.Id, mapping => {
                mapping.Column("id");
                mapping.Type(NHibernateUtil.Guid);
            });

            Property(_ => _.Name, mapping => {
                mapping.Column("name");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(256);
                mapping.Index("idx_identity_roles_name");
            });

            Property(_ => _.NormalizedName, mapping => {
                mapping.Column("normalized_name");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(256);
                mapping.Index("idx_identity_roles_normalized_name");
            });

            Property(_ => _.ConcurrencyStamp, mapping => {
                mapping.Column("concurrency_stamp");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
            });
        }

        #endregion
    }
}
