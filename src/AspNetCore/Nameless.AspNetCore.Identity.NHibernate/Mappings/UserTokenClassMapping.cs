using Nameless.AspNetCore.Identity.Models;
using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.AspNetCore.Identity.NHibernate.Mappings {

    public sealed class UserTokenClassMapping : ClassMapping<UserToken> {

        #region Public Constructors

        public UserTokenClassMapping() {
            Table("identity_user_tokens");

            Property(_ => _.UserId, mapping => {
                mapping.Column("user_id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.NotNullable(true);
            });

            Property(_ => _.LoginProvider, mapping => {
                mapping.Column("login_provider");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(256);
            });

            Property(_ => _.Name, mapping => {
                mapping.Column("name");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(256);
            });

            Property(_ => _.Value, mapping => {
                mapping.Column("value");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(false);
                mapping.Length(512);
            });
        }

        #endregion
    }
}
