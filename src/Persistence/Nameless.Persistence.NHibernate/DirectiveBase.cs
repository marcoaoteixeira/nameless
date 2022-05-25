using NHibernate;

namespace Nameless.Persistence.NHibernate {

    public abstract class DirectiveBase<TResult> : IDirective<TResult?> {

        #region Protected Read-Only Properties

        protected ISession Session { get; }

        #endregion

        #region Protected Constructors

        protected DirectiveBase(ISession session) {
            Prevent.Null(session, nameof(session));

            Session = session;
        }

        #endregion

        #region IDirective<TResult> Members

        public abstract Task<TResult?> ExecuteAsync(ParameterSet parameters, CancellationToken cancellationToken = default);

        #endregion
    }
}
