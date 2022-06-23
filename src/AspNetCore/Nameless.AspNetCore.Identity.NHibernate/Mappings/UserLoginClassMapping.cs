using Nameless.AspNetCore.Identity.Models;
using NHibernate;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.AspNetCore.Identity.NHibernate.Mappings {

    public sealed class UserLoginClassMapping : ClassMapping<UserLogin> {

        #region Public Constructors

        public UserLoginClassMapping() {
            Table("identity_user_logins");

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

            Property(_ => _.ProviderKey, mapping => {
                mapping.Column("provider_key");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(512);
            });

            Property(_ => _.ProviderDisplayName, mapping => {
                mapping.Column("provider_display_name");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(false);
                mapping.Length(256);
            });
        }

        #endregion
    }
}
