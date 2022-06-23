using FluentMigrator;

namespace Nameless.IdentityServer.Migrations {

    // Version format is yyyyMMddHHmmss
    [Migration(00000000000000)]
    public sealed class Initial : Migration {

        #region Public Override Methods

        public override void Up() {
            Create.Table("users")
                .InSchema("dbo")
                .WithColumn("id").AsGuid().NotNullable()
                .WithColumn("username").AsString(256).NotNullable()
                .WithColumn("first_name").AsString(256).Nullable()
                .WithColumn("last_name").AsString(256).Nullable()
                .WithColumn("email").AsString(256).NotNullable()
                .WithColumn("email_confirmed").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("phone_number").AsString(32).Nullable()
                .WithColumn("phone_number_confirmed").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("password_hash").AsString(512).NotNullable()
                .WithColumn("avatar_url").AsString(4096).Nullable()
                .WithColumn("two_factor_auth_enabled").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("lockout_end").AsDateTime().Nullable()
                .WithColumn("lockout_enabled").AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn("access_failure_counter").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("creation_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("modification_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("users_claims")
                .InSchema("dbo")
                .WithColumn("user_id").AsGuid().NotNullable()
                .WithColumn("type").AsString(256).NotNullable()
                .WithColumn("value").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("modification_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("users_logins")
                .InSchema("dbo")
                .WithColumn("user_id").AsGuid().NotNullable()
                .WithColumn("login_provider").AsString(256).NotNullable()
                .WithColumn("provider_key").AsString(256).NotNullable()
                .WithColumn("display_name").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("modification_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("users_tokens")
                .InSchema("dbo")
                .WithColumn("user_id").AsGuid().NotNullable()
                .WithColumn("login_provider").AsString(256).NotNullable()
                .WithColumn("name").AsString(256).NotNullable()
                .WithColumn("value").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("modification_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("roles")
                .InSchema("dbo")
                .WithColumn("id").AsGuid().NotNullable()
                .WithColumn("name").AsString(256).NotNullable()
                .WithColumn("version").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("creation_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("modification_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("roles_claims")
                .InSchema("dbo")
                .WithColumn("role_id").AsGuid().NotNullable()
                .WithColumn("type").AsString(256).NotNullable()
                .WithColumn("value").AsString(256).Nullable()
                .WithColumn("creation_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("modification_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("users_in_roles")
                .WithColumn("user_id").AsGuid().NotNullable()
                .WithColumn("role_id").AsGuid().NotNullable();

            Create.Table("refresh_tokens")
                .InSchema("dbo")
                .WithColumn("id").AsGuid().NotNullable()
                .WithColumn("user_id").AsGuid().NotNullable()
                .WithColumn("token").AsString(4096).NotNullable()
                .WithColumn("expires_date").AsDateTime().NotNullable()
                .WithColumn("created_by_ip").AsString(64).NotNullable()
                .WithColumn("created_date").AsDateTime().NotNullable()
                .WithColumn("revoked_by_ip").AsString(64).Nullable()
                .WithColumn("revoked_date").AsDateTime().Nullable()
                .WithColumn("replaced_by_token").AsString(4096).Nullable()
                .WithColumn("reason_revoked").AsString(2048).Nullable()
                .WithColumn("creation_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("modification_date").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);


            // Primary Keys, Foreign Keys and Constraints

            // users
            Create.PrimaryKey("pk_users").OnTable("users").WithSchema("dbo").Column("id");
            Create.UniqueConstraint("uq_users_email").OnTable("users").WithSchema("dbo").Column("email");
            Create.Index("ix_users_username").OnTable("users").OnColumn("username");
            Create.Index("ix_users_email").OnTable("users").OnColumn("email");

            // users_claims
            Create.PrimaryKey("pk_users_claims").OnTable("users_claims").WithSchema("dbo").Columns("user_id", "type");
            Create.Index("ix_users_claims_type").OnTable("users_claims").OnColumn("type");
            Create.ForeignKey("fk_users_claims_to_users").FromTable("users_claims").ForeignColumn("user_id").ToTable("users").PrimaryColumn("id");

            // users_logins
            Create.PrimaryKey("pk_users_logins").OnTable("users_logins").WithSchema("dbo").Columns("user_id", "login_provider");
            Create.Index("ix_users_logins_login_provider").OnTable("users_logins").OnColumn("login_provider");
            Create.ForeignKey("fk_users_logins_to_users").FromTable("users_logins").ForeignColumn("user_id").ToTable("users").PrimaryColumn("id");

            // users_tokens
            Create.PrimaryKey("pk_users_tokens").OnTable("users_tokens").WithSchema("dbo").Columns("user_id", "login_provider");
            Create.Index("ix_users_tokens_login_provider").OnTable("users_tokens").OnColumn("login_provider");
            Create.ForeignKey("fk_users_tokens_to_users").FromTable("users_tokens").ForeignColumn("user_id").ToTable("users").PrimaryColumn("id");

            // roles
            Create.PrimaryKey("pk_roles").OnTable("roles").WithSchema("dbo").Column("id");
            Create.UniqueConstraint("uq_roles_name").OnTable("roles").WithSchema("dbo").Column("name");
            Create.Index("ix_roles_name").OnTable("roles").OnColumn("name");

            // roles_claims
            Create.PrimaryKey("pk_roles_claims").OnTable("roles_claims").WithSchema("dbo").Columns("role_id", "type");
            Create.Index("ix_roles_claims_type").OnTable("roles_claims").OnColumn("type");
            Create.ForeignKey("fk_roles_claims_to_users").FromTable("roles_claims").ForeignColumn("role_id").ToTable("roles").PrimaryColumn("id");

            // users_in_roles
            Create.PrimaryKey("pk_users_in_roles").OnTable("users_in_roles").WithSchema("dbo").Columns("user_id", "role_id");
            Create.ForeignKey("fk_users_in_roles_to_users").FromTable("users_in_roles").ForeignColumn("user_id").ToTable("users").PrimaryColumn("id");
            Create.ForeignKey("fk_users_in_roles_to_roles").FromTable("users_in_roles").ForeignColumn("role_id").ToTable("roles").PrimaryColumn("id");

            // refresh_tokens
            Create.PrimaryKey("pk_refresh_tokens").OnTable("refresh_tokens").WithSchema("dbo").Column("id");
            Create.ForeignKey("fk_refresh_tokens_to_users").FromTable("refresh_tokens").ForeignColumn("user_id").ToTable("users").PrimaryColumn("id");
            Create.Index("ix_refresh_tokens_user_id").OnTable("refresh_tokens").OnColumn("user_id");
            Create.Index("ix_refresh_tokens_created_by_ip").OnTable("refresh_tokens").OnColumn("created_by_ip");
            Create.Index("ix_refresh_tokens_revoked_by_ip").OnTable("refresh_tokens").OnColumn("revoked_by_ip");
        }

        public override void Down() {
            Delete.Table("refresh_tokens");

            Delete.Table("users_in_roles");

            Delete.Table("roles_claims");
            Delete.Table("roles");

            Delete.Table("users_claims");
            Delete.Table("users_logins");
            Delete.Table("users_tokens");
            Delete.Table("users");
        }

        #endregion
    }
}
