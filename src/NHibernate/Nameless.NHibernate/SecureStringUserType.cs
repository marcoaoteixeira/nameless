using System.Data.Common;
using Nameless.Security.Cryptography;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Nameless.NHibernate {

    public sealed class SecureStringUserType : IUserType {

        #region IUserType Members

        public SqlType[] SqlTypes => new[] { new StringSqlType() };

        public Type ReturnedType => typeof(string);

        public bool IsMutable => false;

        public object Assemble(object cached, object owner) => cached;

        public object DeepCopy(object value) => value;

        public object Disassemble(object value) => value;

        public new bool Equals(object x, object y) => object.Equals(x, y);

        public int GetHashCode(object x) => x != null ? x.GetHashCode() : 0;

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner) {
            var value = rs[names.First()];
            if (value == DBNull.Value) { return default!; }

            var result = AesCryptoProvider.Instance.Decrypt((string)value);

            return result!;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session) {
            object? parameterValue = DBNull.Value;

            if (value != null) {
                parameterValue = AesCryptoProvider.Instance.Encrypt((string)value);
            }

            cmd.Parameters[index].Value = parameterValue;
        }

        public object Replace(object original, object target, object owner) => original;

        #endregion
    }
}
