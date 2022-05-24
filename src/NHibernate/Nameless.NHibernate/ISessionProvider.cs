using System;
using NHibernate;

namespace Nameless.NHibernate {

    public interface ISessionProvider : IDisposable {

        #region Methods

        ISession GetSession();

        #endregion
    }
}
