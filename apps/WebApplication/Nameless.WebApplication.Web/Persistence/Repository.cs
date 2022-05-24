using System.Linq.Expressions;
using Nameless.NHibernate;
using Nameless.WebApplication.Web.Entities;
using NHibernate.Linq;
using NH_ISession = NHibernate.ISession;

namespace Nameless.WebApplication.Web.Persistence {

    public sealed class Repository : IRepository {

        #region Private Fields

        private NH_ISession? _session;
        private bool _disposed;

        #endregion

        #region Public Constructors

        public Repository(ISessionProvider sessionProvider) {
            Ensure.NotNull(sessionProvider, nameof(sessionProvider));

            _session = sessionProvider.GetSession();
        }

        #endregion

        #region Destructor

        ~Repository() {
            Dispose(disposing: false);
        }

        #endregion

        #region Private Methods

        private void BlockAccessAfterDispose() {
            if (_disposed) {
                throw new ObjectDisposedException(nameof(Repository));
            }
        }

        private void Dispose(bool disposing) {
            if (_disposed) { return; }
            if (disposing) {
                _session?.Dispose();
            }

            _session = null;
            _disposed = true;
        }

        #endregion

        #region IRepository<User> Members

        public Task<TEntity?> SaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : EntityBase {
            BlockAccessAfterDispose();

            Ensure.NotNull(entity, nameof(entity));

            using var transaction = _session!.BeginTransaction();
            try {
                _session.SaveOrUpdate(entity);
                transaction.Commit();
            }
            catch {
                transaction.Rollback();
                throw;
            }

            var current = _session!.Get<TEntity>(entity.Id);

            return Task.FromResult<TEntity?>(current);
        }

        public Task<bool> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : EntityBase {
            BlockAccessAfterDispose();

            Ensure.NotNull(entity, nameof(entity));

            return _session!
                .DeleteAsync(entity, cancellationToken)
                .ContinueWith(antecedent =>
                    antecedent.CanContinue() &&
                    _session.Query<TEntity>().Count(_ => _.Id == entity.Id) == 0
                );
        }

        public Task DeleteAllAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : EntityBase {
            BlockAccessAfterDispose();

            Ensure.NotNull(filter, nameof(filter));

            return _session!.Query<TEntity>().Where(filter).DeleteAsync(cancellationToken);
        }

        public Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : EntityBase {
            BlockAccessAfterDispose();

            Ensure.NotNull(filter, nameof(filter));

            var result = _session!.Query<TEntity>().Count(filter) > 0;

            return Task.FromResult(result);
        }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : EntityBase {
            BlockAccessAfterDispose();

            return _session!.Query<TEntity>();
        }

        #endregion

            #region IDisposable Members

        public void Dispose() {
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
