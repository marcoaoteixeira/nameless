using System.Linq.Expressions;

namespace Nameless.Persistence {

    public interface IReader {

		#region Methods

		IAsyncEnumerable<TEntity> FindAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class;

		IQueryable<TEntity> Query<TEntity>() where TEntity : class;

		#endregion
	}
}