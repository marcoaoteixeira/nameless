using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.WebApplication.Web.Entities.Mappings {

    public sealed class UserClassMapping : ClassMapping<User> {

        #region Public Constructors

        public UserClassMapping() {
            Table("users");

            Id(_ => _.Id, mapping => {
                mapping.Column("id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.Access(Accessor.Field);
            });

            Property(_ => _.Username, mapping => {
                mapping.Column("username");
                mapping.Length(255);
                mapping.NotNullable(true);
            });

            Property(_ => _.Email, mapping => {
                mapping.Column("email");
                mapping.Length(255);
                mapping.NotNullable(true);
            });

            Property(_ => _.PasswordHash, mapping => {
                mapping.Column("password_hash");
                mapping.Length(255);
                mapping.NotNullable(true);
            });

            Property(_ => _.Role, mapping => {
                mapping.Column("role");
                mapping.Length(255);
                mapping.NotNullable(true);
            });

            Property(_ => _.CreationDate, mapping => {
                mapping.Column("creation_date");
                mapping.Access(Accessor.Field);
                mapping.NotNullable(true);
            });

            Property(_ => _.ModificationDate, mapping => {
                mapping.Column("modification_date");
                mapping.Type(NHibernateUtil.DateTime);
            });
        }

        #endregion
    }
}
