using System.Linq.Expressions;

namespace Nameless.Persistence {

    /// <summary>
    /// Exposes methods to define a data writer.
    /// </summary>
    public interface IWriter {

        #region Methods

        Task<TEntity> SaveAsync<TEntity>(TEntity entity, Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) where TEntity : class;

        Task<bool> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class;

        #endregion
    }
}