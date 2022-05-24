using Autofac;
using Autofac_Parameter = Autofac.Core.Parameter;

namespace Nameless.DependencyInjection.Autofac {

    /// <summary>
    /// Default implementation of <see cref="IServiceResolver"/> using Autofac (https://autofac.org/).
    /// </summary>
    public sealed class ServiceResolver : IServiceResolver {

        #region Private Fields

        private ILifetimeScope? _currentScope;
        private bool _disposed;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ServiceResolver"/>.
        /// </summary>
        /// <param name="scope">The lifetime scope.</param>
        /// <param name="isRoot">Whether is root scope or not.</param>
        public ServiceResolver(ILifetimeScope scope, bool isRoot = false) {
            _currentScope = scope ?? throw new ArgumentNullException(nameof(scope));

            IsRoot = isRoot;
        }

        #endregion

        #region Destructor

        ~ServiceResolver() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Static Methods

        private static Autofac_Parameter[] Convert(Parameter[] parameters) {
            var result = new List<Autofac_Parameter>();
            if (!parameters.IsNullOrEmpty()) {
                foreach (var parameter in parameters) {

                    if (parameter.Type != null) {
                        result.Add(new TypedParameter(parameter.Type, parameter.Value));
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(parameter.Name) && parameter.Value != null) {
                        result.Add(new NamedParameter(parameter.Name, parameter.Value));
                        continue;
                    }
                }
            }
            return result.ToArray();
        }

        #endregion

        #region Private Methods

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed || IsRoot) { return; }
            if (disposing) {
                if (_currentScope != null) {
                    _currentScope.Dispose();
                }
            }
            _currentScope = null;
            _disposed = true;
        }

        #endregion

        #region IResolver Members

        public bool IsRoot { get; }

        /// <inheritdoc />
        public object Get(Type serviceType, string? name = null, params Parameter[] parameters) {
            BlockAccessAfterDispose();

            var parametersList = Convert(parameters);

            return !string.IsNullOrWhiteSpace(name)
                ? _currentScope!.ResolveNamed(name, serviceType, parametersList)
                : _currentScope!.Resolve(serviceType, parametersList);
        }

        public object? GetOptional(Type serviceType, string? name = null, params Parameter[] parameters) {
            BlockAccessAfterDispose();

            var parametersList = Convert(parameters);

            if (string.IsNullOrWhiteSpace(name)) {
                return _currentScope!.ResolveOptional(serviceType, parametersList);
            }

            var method = typeof(ResolutionExtensions).GetMethod(nameof(ResolutionExtensions.ResolveOptionalNamed));
            if (method == null) { return default; }
            var generic = method.MakeGenericMethod(serviceType);

            return generic.Invoke(obj: null, new object[] {
                _currentScope!,
                name,
                parametersList
            });
        }

        /// <inheritdoc />
        public IServiceResolver GetScoped() {
            BlockAccessAfterDispose();

            var scope = _currentScope!.BeginLifetimeScope();
            return new ServiceResolver(scope, isRoot: false);
        }

        /// <inheritdoc />
        public IServiceResolver GetScoped(Action<IServiceComposer> compose) {
            BlockAccessAfterDispose();

            var scope = _currentScope!.BeginLifetimeScope(builder => {
                var composer = new ServiceComposer(builder);
                compose(composer);
            });

            return new ServiceResolver(scope, isRoot: false);
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
            if (!IsRoot) {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        #endregion
    }
}