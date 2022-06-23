using Nameless.IdentityServer.Entities;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.IdentityServer.Mappings {

    public class RoleClassMapping : ClassMapping<Role> {

        #region Public Constructors

        public RoleClassMapping() {

            Table("roles");
            
            Id(_ => _.ID, mapping => {
                mapping.Column("id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.Access(Accessor.Field);
            });

            Property(_ => _.Name, mapping => {
                mapping.Column("name");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: true);
                mapping.UniqueKey("uq_roles_name");
                mapping.Index("ix_roles_name");
            });

            Version(_ => _.Version, mapping => {
                mapping.Column("version");
                mapping.Access(Accessor.Field);
                mapping.Type(NHibernateUtil.Int32);
                mapping.Generated(VersionGeneration.Always);
            });
            OptimisticLock(OptimisticLockMode.Version);
            DynamicUpdate(true);

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
