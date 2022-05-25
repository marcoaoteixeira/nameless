using NHibernate;

namespace Nameless.NHibernate {

    public sealed class SessionProvider : ISessionProvider {

        #region Private Read-Only Fields

        private readonly IConfigurationBuilder _configurationBuilder;
        private readonly NHibernateOptions _options;

        #endregion

        #region Private Fields

        private ISessionFactory? _sessionFactory;
        private bool _disposed;

        #endregion

        #region Public Constructors

        public SessionProvider(IConfigurationBuilder configurationBuilder, NHibernateOptions? options = null) {
            Prevent.Null(configurationBuilder, nameof(configurationBuilder));

            _configurationBuilder = configurationBuilder;
            _options = options ?? new();
        }

        #endregion

        #region Destructor

        ~SessionProvider() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(SessionProvider));
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                _sessionFactory?.Dispose();
            }

            _sessionFactory = null;
            _disposed = true;
        }

        #endregion

        #region ISessionProvider Members

        public ISession GetSession() {
            BlockAccessAfterDispose();

            if (_sessionFactory == null) {
                _sessionFactory = _configurationBuilder
                    .Build(_options)
                    .BuildSessionFactory();
            }

            return _sessionFactory.OpenSession();
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
