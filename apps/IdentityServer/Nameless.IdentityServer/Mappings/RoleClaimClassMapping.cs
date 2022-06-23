using Nameless.IdentityServer.Entities;
using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.IdentityServer.Mappings {

    public sealed class RoleClaimClassMapping : ClassMapping<RoleClaim> {

        #region Public Constructors

        public RoleClaimClassMapping() {

            Table("roles_claims");

            ComposedId(compose => {
                compose.Property(_ => _.RoleID, mapping => {
                    mapping.Column("role_id");
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

            //Property(_ => _.RoleID, mapping => {
            //    mapping.Column("role_id");
            //    mapping.Type(NHibernateUtil.Guid);
            //    mapping.NotNullable(notnull: true);
            //    mapping.Index("ix_role_claims_role_id");
            //});

            //Property(_ => _.Type, mapping => {
            //    mapping.Column("type");
            //    mapping.Type(NHibernateUtil.String);
            //    mapping.Length(256);
            //    mapping.NotNullable(notnull: true);
            //    mapping.Index("ix_role_claim_type");
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
