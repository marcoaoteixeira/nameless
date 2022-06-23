using Nameless.AspNetCore.Identity.Models;
using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.AspNetCore.Identity.NHibernate.Mappings {

    public sealed class UserInRoleClassMapping : ClassMapping<UserInRole> {

        #region Public Constructors

        public UserInRoleClassMapping() {
            Table("identity_users_in_roles");

            Property(_ => _.UserId, mapping => {
                mapping.Column("user_id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.NotNullable(true);
            });

            Property(_ => _.RoleId, mapping => {
                mapping.Column("role_id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.NotNullable(true);
            });
        }

        #endregion
    }
}
