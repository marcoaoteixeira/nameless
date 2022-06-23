using Nameless.IdentityServer.Entities;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.IdentityServer.Mappings {

    public class RefreshTokenClassMapping : ClassMapping<RefreshToken> {

        #region Public Constructors

        public RefreshTokenClassMapping() {

            Table("refresh_tokens");

            Id(_ => _.ID, mapping => {
                mapping.Column("id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.Access(Accessor.Field);
            });

            Property(_ => _.UserID, mapping => {
                mapping.Column("user_id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.NotNullable(notnull: true);
                mapping.Index("ix_refresh_tokens_user_id");
            });

            Property(_ => _.Token, mapping => {
                mapping.Column("token");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(4096);
                mapping.NotNullable(notnull: true);
            });

            Property(_ => _.ExpiresDate, mapping => {
                mapping.Column("expires_date");
                mapping.Type(NHibernateUtil.DateTime);
                mapping.NotNullable(notnull: true);
            });

            Property(_ => _.CreatedByIp, mapping => {
                mapping.Column("created_by_ip");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: true);
                mapping.Index("ix_refresh_tokens_created_by_ip");
            });

            Property(_ => _.CreatedDate, mapping => {
                mapping.Column(column => {
                    column.Name("created_date");
                    column.Default(DateTime.UtcNow);
                });
                mapping.Type(NHibernateUtil.DateTime);
                mapping.NotNullable(notnull: true);
            });

            Property(_ => _.RevokedByIp, mapping => {
                mapping.Column("revoked_by_ip");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(256);
                mapping.NotNullable(notnull: false);
                mapping.Index("ix_refresh_tokens_revoked_by_ip");
            });

            Property(_ => _.RevokedDate, mapping => {
                mapping.Column(column => {
                    column.Name("revoked_date");
                    column.Default(DateTime.UtcNow);
                });
                mapping.Type(NHibernateUtil.DateTime);
                mapping.NotNullable(notnull: true);
            });

            Property(_ => _.ReplacedByToken, mapping => {
                mapping.Column("replaced_by_token");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(4096);
                mapping.NotNullable(notnull: false);
            });

            Property(_ => _.ReasonRevoked, mapping => {
                mapping.Column("reason_revoked");
                mapping.Type(NHibernateUtil.String);
                mapping.Length(2048);
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
