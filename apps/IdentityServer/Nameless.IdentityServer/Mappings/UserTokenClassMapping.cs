using Nameless.IdentityServer.Entities;
using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.IdentityServer.Mappings {

    public class UserTokenClassMapping : ClassMapping<UserToken> {

        #region Public Constructors

        public UserTokenClassMapping() {

            Table("users_tokens");

            ComposedId(compose => {
                compose.Property(_ => _.UserID, mapping => {
                    mapping.Column("user_id");
                    mapping.Type(NHibernateUtil.Guid);
                    mapping.NotNullable(notnull: true);
                });

                compose.Property(_ => _.LoginProvider, mapping => {
                    mapping.Column("login_provider");
                    mapping.Type(NHibernateUtil.String);
                    mapping.Length(256);
                    mapping.NotNullable(notnull: true);
                });
            });

            //Property(_ => _.UserID, mapping => {
            //    mapping.Column("user_id");
            //    mapping.Type(NHibernateUtil.Guid);
            //    mapping.NotNullable(notnull: true);
            //});

            //Property(_ => _.LoginProvider, mapping => {
            //    mapping.Column("login_provider");
            //    mapping.Type(NHibernateUtil.String);
            //    mapping.Length(256);
            //    mapping.NotNullable(notnull: true);
            //});

            Property(_ => _.Name, mapping => {
                mapping.Column("name");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: true);
            });

            Property(_ => _.Value, mapping => {
                mapping.Column("value");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: true);
            });

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
