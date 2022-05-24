using System.Linq.Expressions;
using Nameless.WebApplication.Web.Entities;

namespace Nameless.WebApplication.Web.Persistence {

    public interface IRepository : IDisposable {

        #region Methods

        Task<TEntity?> SaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : EntityBase;
        Task<bool> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : EntityBase;
        Task DeleteAllAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : EntityBase;
        Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : EntityBase;
        IQueryable<TEntity> Query<TEntity>() where TEntity : EntityBase;

        #endregion
    }
}
