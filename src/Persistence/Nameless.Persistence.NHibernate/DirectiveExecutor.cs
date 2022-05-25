using NHibernate;

namespace Nameless.Persistence.NHibernate {

    public sealed class DirectiveExecutor : IDirectiveExecutor {

        #region Private Read-Only Fields

        private readonly ISession _session;

        #endregion

        #region Public Constructors

        public DirectiveExecutor(ISession session) {
            Prevent.Null(session, nameof(session));

            _session = session;
        }

        #endregion

        #region IDirectiveExecutor Members

        public Task<TResult?> ExecuteDirectiveAsync<TResult, TDirective>(ParameterSet parameters, CancellationToken cancellationToken = default) where TDirective : IDirective<TResult?> {
            if (!typeof(TDirective).IsAssignableToGenericType(typeof(DirectiveBase<>))) {
                throw new InvalidOperationException($"Directive must inherit from \"{typeof(DirectiveBase<>)}\"");
            }

            return Activator.CreateInstance(typeof(TDirective), new object[] { _session }) is IDirective<TResult?> directive
                ? directive.ExecuteAsync(parameters, cancellationToken)
                : Task.FromResult<TResult?>(default);
        }

        #endregion
    }
}
