using Autofac;

namespace Nameless.DependencyInjection.Autofac {

    /// <summary>
    /// Default implementation of <see cref="ICompositionRoot"/> using Autofac (https://autofac.org/).
    /// </summary>
    public sealed class CompositionRoot : ICompositionRoot, IDisposable {

        #region Private Fields

        private ServiceComposer? _serviceComposer;
        private bool _disposed;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the Autofac container.
        /// </summary>
        public IContainer? Container { get; private set; }

        #endregion

        #region Public Constructors

        public CompositionRoot() {
            _serviceComposer = new ServiceComposer();
        }

        #endregion

        #region Destructor

        /// <summary>
        /// Destructor.
        /// </summary>
        ~CompositionRoot() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) { TearDown(); }
            _disposed = true;
        }

        #endregion

        #region ICompositionRoot Members

        /// <inheritdoc/>
        public void Compose(Action<IServiceComposer> action) => action(_serviceComposer!);

        /// <inheritdoc/>
        public void StartUp() {
            if (Container != null) {
                throw new InvalidOperationException("Composition root already started.");
            }

            // Register Service Resolver
            _serviceComposer!.Builder
                .Register(ctx => new ServiceResolver(ctx.Resolve<ILifetimeScope>(), isRoot: true))
                .As<IServiceResolver>()
                .PreserveExistingDefaults()
                .SetLifetimeScope(LifetimeScopeType.Singleton);

            Container = _serviceComposer.Builder.Build();
        }

        /// <inheritdoc/>
        public void TearDown() {
            if (Container != null) {
                Container.Dispose();
            }
            Container = null;
            _serviceComposer = null;
        }

        #endregion

        #region IDisposable Members

        /// <inheritdoc/>
        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(obj: this);
        }

        #endregion
    }
}