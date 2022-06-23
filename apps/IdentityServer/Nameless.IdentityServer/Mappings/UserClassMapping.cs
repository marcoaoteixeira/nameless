using Nameless.IdentityServer.Entities;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.IdentityServer.Mappings {

    public class UserClassMapping : ClassMapping<User> {

        #region Public Constructors

        public UserClassMapping() {

            Table("users");

            Id(_ => _.ID, mapping => {
                mapping.Column("id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.Access(Accessor.Field);
            });

            Property(_ => _.Username, mapping => {
                mapping.Column("username");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: true);
                mapping.Index("ix_users_username");
            });

            Property(_ => _.FirstName, mapping => {
                mapping.Column("first_name");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.LastName, mapping => {
                mapping.Column("last_name");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.Email, mapping => {
                mapping.Column("email");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: true);
                mapping.UniqueKey("uq_users_email");
                mapping.Index("ix_users_email");
            });

            Property(_ => _.EmailConfirmed, mapping => {
                mapping.Column("email_confirmed");
                mapping.Type(NHibernateUtil.Boolean);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.PhoneNumber, mapping => {
                mapping.Column("phone_number");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(32);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.PhoneNumberConfirmed, mapping => {
                mapping.Column("phone_number_confirmed");
                mapping.Type(NHibernateUtil.Boolean);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.PasswordHash, mapping => {
                mapping.Column("password_hash");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(512);
                mapping.NotNullable(notnull: true);
            });

            Property(_ => _.AvatarUrl, mapping => {
                mapping.Column("avatar_url");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(4096);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.TwoFactorAuthEnabled, mapping => {
                mapping.Column("two_factor_auth_enabled");
                mapping.Type(NHibernateUtil.Boolean);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.LockoutEnd, mapping => {
                mapping.Column("lockout_end");
                mapping.Type(NHibernateUtil.DateTime);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.LockoutEnabled, mapping => {
                mapping.Column("lockout_enabled");
                mapping.Type(NHibernateUtil.Boolean);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.AccessFailureCounter, mapping => {
                mapping.Column("access_failure_counter");
                mapping.Type(NHibernateUtil.Int32);
                mapping.NotNullable(notnull: false);
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
