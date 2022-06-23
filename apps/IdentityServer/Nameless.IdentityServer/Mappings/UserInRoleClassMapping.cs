using Nameless.IdentityServer.Entities;
using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.IdentityServer.Mappings {

    public class UserInRoleClassMapping : ClassMapping<UserInRole> {

        #region Public Constructors

        public UserInRoleClassMapping() {

            Table("users_in_roles");

            ComposedId(compose => {
                compose.Property(_ => _.UserID, mapping => {
                    mapping.Column("user_id");
                    mapping.Type(NHibernateUtil.Guid);
                    mapping.NotNullable(notnull: true);
                });

                compose.Property(_ => _.RoleID, mapping => {
                    mapping.Column("role_id");
                    mapping.Type(NHibernateUtil.Guid);
                    mapping.NotNullable(notnull: true);
                });
            });

            //Property(_ => _.UserID, mapping => {
            //    mapping.Column("user_id");
            //    mapping.Type(NHibernateUtil.Guid);
            //    mapping.NotNullable(notnull: true);
            //});

            //Property(_ => _.RoleID, mapping => {
            //    mapping.Column("role_id");
            //    mapping.Type(NHibernateUtil.Guid);
            //    mapping.NotNullable(notnull: true);
            //});

            Property(_ => _.CreationDate, mapping => {
                mapping.Column(column => {
                    column.Name("creation_date");
                    column.Default(DateTime.UtcNow);
                });
                mapping.Type(NHibernateUtil.DateTime);
                mapping.NotNullable(notnull: true);
            });

            Property(_ => _.ModificationDate, mapping => {
                mapping.Column(column => {
                    column.Name("modification_date");
                    column.Default(DateTime.UtcNow);
                });
                mapping.Type(NHibernateUtil.DateTime);
                mapping.NotNullable(notnull: false);
            });
        }

        #endregion
    }
}
