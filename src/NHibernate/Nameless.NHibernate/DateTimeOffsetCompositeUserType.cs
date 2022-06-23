using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.UserTypes;

namespace Nameless.NHibernate {

    public sealed class DateTimeOffsetCompositeUserType : ICompositeUserType {

        #region ICompositeUserType Members

        public string[] PropertyNames => throw new NotImplementedException();

        public IType[] PropertyTypes => throw new NotImplementedException();

        public Type ReturnedClass => throw new NotImplementedException();

        public bool IsMutable => throw new NotImplementedException();

        public object Assemble(object cached, ISessionImplementor session, object owner) {
            throw new NotImplementedException();
        }

        public object DeepCopy(object value) {
            throw new NotImplementedException();
        }

        public object Disassemble(object value, ISessionImplementor session) {
            throw new NotImplementedException();
        }

        public new bool Equals(object x, object y) {
            throw new NotImplementedException();
        }

        public int GetHashCode(object x) {
            throw new NotImplementedException();
        }

        public object GetPropertyValue(object component, int property) {
            throw new NotImplementedException();
        }

        public object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner) {
            throw new NotImplementedException();
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session) {
            throw new NotImplementedException();
        }

        public object Replace(object original, object target, ISessionImplementor session, object owner) {
            throw new NotImplementedException();
        }

        public void SetPropertyValue(object component, int property, object value) {
            throw new NotImplementedException();
        }

        #endregion
    }
}
