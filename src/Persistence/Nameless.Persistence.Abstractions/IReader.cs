using System.Linq.Expressions;

namespace Nameless.Persistence {

    public interface IReader {

		#region Methods

		Task<IList<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>>? orderBy = null, bool orderDescending = false, CancellationToken cancellationToken = default) where TEntity : class;

		Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class;

		IQueryable<TEntity> Query<TEntity>() where TEntity : class;

		#endregion
	}
}