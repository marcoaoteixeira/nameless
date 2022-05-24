using System.Linq.Expressions;
using NHibernate;

namespace Nameless.Persistence.NHibernate {

    public sealed class Reader : IReader {

        #region Private Read-Only Fields

        private readonly ISession _session;

        #endregion

        #region Public Constructors

        public Reader(ISession session) {
            Ensure.NotNull(session, nameof(session));

            _session = session;
        }

        #endregion

        #region IQuerier Members

        public IAsyncEnumerable<TEntity> FindAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class {
            return _session.Query<TEntity>().Where(filter).AsAsyncEnumerable();
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class {
            return _session.Query<TEntity>();
        }

        #endregion

    }
}
