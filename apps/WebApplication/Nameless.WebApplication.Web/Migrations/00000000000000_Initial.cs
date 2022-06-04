using FluentMigrator;

namespace Nameless.WebApplication.Web.Migrations {

    [Migration(00000000000000)]
    public sealed class Initial : Migration {

        #region Public Override Methods

        public override void Up() {
            Create.Table("users")
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("username").AsString(256).NotNullable()
                .WithColumn("first_name").AsString(256).Nullable()
                .WithColumn("last_name").AsString(256).Nullable()
                .WithColumn("email").AsString(256).NotNullable().Unique()
                .WithColumn("email_confirmed").AsBoolean().Nullable()
                .WithColumn("phone_number").AsString(32).Nullable()
                .WithColumn("phone_number_confirmed").AsBoolean().Nullable()
                .WithColumn("avatar_url").AsString(2048).Nullable()
                .WithColumn("two_factor_enabled").AsBoolean().Nullable()
                .WithColumn("lockout_end").AsDateTime().Nullable()
                .WithColumn("lockout_enabled").AsBoolean().Nullable()
                .WithColumn("access_failure_counter").AsInt32().Nullable()
                .WithColumn("password_hash").AsString(512).NotNullable()
                .WithColumn("concurrency_stamp").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("modification_date").AsDateTime().Nullable();

            Create.Table("user_claims")
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("users", "id")
                .WithColumn("type").AsString(256).NotNullable()
                .WithColumn("value").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("modification_date").AsDateTime().Nullable();

            Create.Table("user_logins")
                .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("users", "id")
                .WithColumn("login_provider").AsString(256).NotNullable()
                .WithColumn("provider_key").AsString(256).NotNullable()
                .WithColumn("display_name").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("modification_date").AsDateTime().Nullable();

            Create.Table("user_tokens")
                .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("users", "id")
                .WithColumn("login_provider").AsString(256).NotNullable()
                .WithColumn("name").AsString(256).NotNullable()
                .WithColumn("value").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("modification_date").AsDateTime().Nullable();

            Create.Table("roles")
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("rolename").AsString(256).NotNullable()
                .WithColumn("concurrency_stamp").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("modification_date").AsDateTime().Nullable();

            Create.Table("role_claims")
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("role_id").AsGuid().NotNullable().ForeignKey("roles", "id")
                .WithColumn("type").AsString(256).NotNullable()
                .WithColumn("value").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("modification_date").AsDateTime().Nullable();

            Create.Table("users_in_roles")
                .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("users", "id")
                .WithColumn("role_id").AsGuid().NotNullable().ForeignKey("roles", "id");

            Create.Table("refresh_tokens")
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("user_id").AsGuid().NotNullable().ForeignKey("users", "id")
                .WithColumn("token").AsString(2048).NotNullable()
                .WithColumn("expires_date").AsDateTime().NotNullable()
                .WithColumn("created_date").AsDateTime().NotNullable()
                .WithColumn("created_by_ip").AsString(64).NotNullable()
                .WithColumn("revoked_date").AsDateTime().Nullable()
                .WithColumn("revoked_by_ip").AsString(64).Nullable()
                .WithColumn("replaced_by_token").AsString(2048).Nullable()
                .WithColumn("reason_revoked").AsString(2048).Nullable()
                .WithColumn("creation_date").AsDateTime().Nullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("modification_date").AsDateTime().Nullable();
        }

        public override void Down() {
            Delete.Table("refresh_tokens");
            Delete.Table("users_in_roles");
            Delete.Table("role_claims");
            Delete.Table("roles");
            Delete.Table("user_tokens");
            Delete.Table("user_logins");
            Delete.Table("user_claims");
            Delete.Table("users");
        }

        #endregion
    }
}
