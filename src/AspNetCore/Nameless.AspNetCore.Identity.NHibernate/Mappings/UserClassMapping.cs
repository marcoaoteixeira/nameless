using Nameless.AspNetCore.Identity.Models;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.AspNetCore.Identity.NHibernate.Mappings {

    public sealed class UserClassMapping : ClassMapping<User> {

        #region Public Constructors

        public UserClassMapping() {
            Table("identity_users");

            OptimisticLock(OptimisticLockMode.Dirty);
            DynamicUpdate(true);

            Id(_ => _.Id, mapping => {
                mapping.Column("id");
                mapping.Type(NHibernateUtil.Guid);
            });

            Property(_ => _.FirstName, mapping => {
                mapping.Column("first_name");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(false);
                mapping.Length(256);
                mapping.Index("idx_identity_users_first_name");
            });

            Property(_ => _.LastName, mapping => {
                mapping.Column("last_name");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(false);
                mapping.Length(256);
                mapping.Index("idx_identity_users_last_name");
            });

            Property(_ => _.UserName, mapping => {
                mapping.Column("user_name");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(256);
                mapping.Index("idx_identity_users_user_name");
            });

            Property(_ => _.NormalizedUserName, mapping => {
                mapping.Column("normalized_user_name");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(256);
                mapping.Index("idx_identity_users_normalized_user_name");
            });

            Property(_ => _.Email, mapping => {
                mapping.Column("email");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(256);
                mapping.Index("idx_identity_users_email");
            });

            Property(_ => _.NormalizedEmail, mapping => {
                mapping.Column("normalized_email");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(256);
                mapping.Index("idx_identity_users_normalized_email");
            });

            Property(_ => _.EmailConfirmed, mapping => {
                mapping.Column("email_confirmed");
                mapping.Type(NHibernateUtil.Boolean);
                mapping.NotNullable(false);
            });

            Property(_ => _.PhoneNumber, mapping => {
                mapping.Column("phone_number");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(false);
                mapping.Length(64);
                mapping.Index("idx_identity_users_phone_number");
            });

            Property(_ => _.PhoneNumberConfirmed, mapping => {
                mapping.Column("phone_number_confirmed");
                mapping.Type(NHibernateUtil.Boolean);
                mapping.NotNullable(false);
            });

            Property(_ => _.AvatarUrl, mapping => {
                mapping.Column("avatar_url");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(false);
                mapping.Length(4096);
            });

            Property(_ => _.PasswordHash, mapping => {
                mapping.Column("password_hash");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(512);
            });

            Property(_ => _.SecurityStamp, mapping => {
                mapping.Column("security_stamp");
                mapping.Type(NHibernateUtil.String);
                mapping.NotNullable(true);
                mapping.Length(512);
            });

            Property(_ => _.LockoutEnabled, mapping => {
                mapping.Column("lockout_enabled");
                mapping.Type(NHibernateUtil.Boolean);
                mapping.NotNullable(false);
            });

            Property(_ => _.LockoutEnd, mapping => {
                mapping.Column("lockout_end");
                mapping.Type(NHibernateUtil.DateTimeOffset);
                mapping.NotNullable(false);
            });

            Property(_ => _.TwoFactorEnabled, mapping => {
                mapping.Column("two_factor_enabled");
                mapping.Type(NHibernateUtil.Boolean);
                mapping.NotNullable(false);
            });

            Property(_ => _.AccessFailedCount, mapping => {
                mapping.Column("access_failed_count");
                mapping.Type(NHibernateUtil.Int32);
                mapping.NotNullable(false);
            });

            Version(_ => _.ConcurrencyStamp, mapping => {
                mapping.Column("concurrency_stamp");
                mapping.Columns(columns => {

                });
                mapping.Type(NHibernateUtil.DateTimeOffset);
                mapping.Generated(VersionGeneration.Always);
            });
        }

        #endregion
    }
}