using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;

namespace Nameless.Persistence.NHibernate {

    public sealed class Writer : IWriter {

        #region Private Read-Only Fields

        private readonly ISession _session;

        #endregion

        #region Public Constructors

        public Writer(ISession session) {
            Prevent.Null(session, nameof(session));

            _session = session;
        }

        #endregion

        #region IWriter Members

        public Task<TEntity> SaveAsync<TEntity>(TEntity entity, Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) where TEntity : class {
            Prevent.Null(entity, nameof(entity));

            using var transaction = _session.BeginTransaction();
            try {
                var counter = filter != null ? _session.Query<TEntity>().Count(filter) : -1;

                /*
                 counter = -1 => SaveOrUpdate
                 counter = 0 => Save
                 counter > 0 => Update
                 */

                switch (counter) {
                    case 0: _session.Save(entity); break;
                    case -1: _session.SaveOrUpdate(entity); break;
                    default: _session.Query<TEntity>().Where(filter!).Update(_ => entity); break;
                }

                transaction.Commit();
            } catch { transaction.Rollback(); throw; }

            return Task.FromResult(entity);
        }

        public Task<bool> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class {
            Prevent.Null(filter, nameof(filter));

            int counter;
            using var transaction = _session.BeginTransaction();
            try {
                counter = _session.Query<TEntity>().Where(filter).Delete();
                transaction.Commit();
            } catch { transaction.Rollback(); throw; }

            return Task.FromResult(counter > 0);
        }

        #endregion
    }
}
