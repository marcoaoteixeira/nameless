using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nameless.WebApplication.Web.Entities.Mappings {

    public sealed class RefreshTokenClassMapping : ClassMapping<RefreshToken> {

        #region Public Constructors

        public RefreshTokenClassMapping() {
            Table("refresh_tokens");

            Id(_ => _.Id, mapping => {
                mapping.Column("id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.Access(Accessor.Field);
            });

            Property(_ => _.UserId, mapping => {
                mapping.Column("user_id");
                mapping.Type(NHibernateUtil.Guid);
                mapping.NotNullable(true);
            });

            Property(_ => _.Token, mapping => {
                mapping.Column("token");
                mapping.Length(2048);
                mapping.NotNullable(true);
            });

            Property(_ => _.ExpiresDate, mapping => {
                mapping.Column("expires_date");
                mapping.Type(NHibernateUtil.DateTime);
            });

            Property(_ => _.CreatedByIp, mapping => {
                mapping.Column("created_by_ip");
                mapping.Length(255);
                mapping.NotNullable(true);
            });

            Property(_ => _.RevokedDate, mapping => {
                mapping.Column("revoked_date");
                mapping.Type(NHibernateUtil.DateTime);
            });

            Property(_ => _.RevokedByIp, mapping => {
                mapping.Column("revoked_by_ip");
                mapping.Length(255);
            });

            Property(_ => _.ReplacedByToken, mapping => {
                mapping.Column("replaced_by_token");
                mapping.Length(2048);
            });

            Property(_ => _.ReasonRevoked, mapping => {
                mapping.Column("reason_revoked");
                mapping.Length(2048);
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
