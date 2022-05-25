using System.Linq.Expressions;
using NHibernate;

namespace Nameless.Persistence.NHibernate {

    public sealed class Reader : IReader {

        #region Private Read-Only Fields

        private readonly ISession _session;

        #endregion

        #region Public Constructors

        public Reader(ISession session) {
            Prevent.Null(session, nameof(session));

            _session = session;
        }

        #endregion

        #region IQuerier Members

        public Task<IList<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>>? orderBy = null, bool orderDescending = false, CancellationToken cancellationToken = default) where TEntity : class {
            Prevent.Null(filter, nameof(filter));

            var query = _session.Query<TEntity>();

            if (orderBy != null) {
                query = orderDescending
                    ? query.OrderBy(orderBy)
                    : query.OrderByDescending(orderBy);
            }

            var result = query
                .Where(filter)
                .ToList() as IList<TEntity>;

            return Task.FromResult(result);
        }

        public Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class {
            Prevent.Null(filter, nameof(filter));

            var counter = _session.Query<TEntity>().Count(filter);

            return Task.FromResult(counter > 0);
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class {
            return _session.Query<TEntity>();
        }

        #endregion
    }
}
