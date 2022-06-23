using Nameless.IdentityServer.Entities;
using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.IdentityServer.Mappings {

    public class UserClaimClassMapping : ClassMapping<UserClaim> {

        #region Public Constructors

        public UserClaimClassMapping() {

            Table("users_claims");

            ComposedId(compose => {
                compose.Property(_ => _.UserID, mapping => {
                    mapping.Column(column => {
                        column.Name("user_id");
                    });
                    mapping.Type(NHibernateUtil.Guid);
                    mapping.NotNullable(notnull: true);
                });

                compose.Property(_ => _.Type, mapping => {
                    mapping.Column("type");
                    mapping.Type(NHibernateUtil.String);
                    mapping.Length(256);
                    mapping.NotNullable(notnull: true);
                });
            });

            //Property(_ => _.UserID, mapping => {
            //    mapping.Column("user_id");
            //    mapping.Type(NHibernateUtil.Guid);
            //    mapping.NotNullable(notnull: true);
            //    mapping.Index("ix_user_claims_user_id");
            //});

            //Property(_ => _.Type, mapping => {
            //    mapping.Column("type");
            //    mapping.Type(NHibernateUtil.String);
            //    mapping.Length(256);
            //    mapping.NotNullable(notnull: true);
            //    mapping.Index("ix_user_claims_type");
            //});

            Property(_ => _.Value, mapping => {
                mapping.Column("value");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: false);
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
