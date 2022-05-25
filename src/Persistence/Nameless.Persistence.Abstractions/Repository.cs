using System.Linq.Expressions;

namespace Nameless.Persistence {

    public sealed class Repository : IRepository {

        #region Private Read-Only Fields

        private readonly IWriter _writer;
        private readonly IReader _reader;

        #endregion Private Read-Only Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Repository" />
        /// </summary>
        /// <param name="writer">The persister.</param>
        /// <param name="reader">The querier.</param>
        public Repository(IWriter writer, IReader reader) {
            Prevent.Null(writer, nameof(writer));
            Prevent.Null(reader, nameof(reader));

            _writer = writer;
            _reader = reader;
        }

        #endregion Public Constructors

        #region IRepository Members

        /// <inheritdoc />
        public Task<TEntity> SaveAsync<TEntity>(TEntity entity, Expression<Func<TEntity, bool>>? filter = null, CancellationToken cancellationToken = default) where TEntity : class {
            return _writer.SaveAsync(entity, filter, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class {
            return _writer.DeleteAsync(filter, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>>? orderBy = null, bool orderDescending = false, CancellationToken cancellationToken = default) where TEntity : class {
            return _reader.FindAsync(filter, orderBy, orderDescending, cancellationToken);
        }

        /// <inheritdoc />
        public IQueryable<TEntity> Query<TEntity>() where TEntity : class {
            return _reader.Query<TEntity>();
        }

        #endregion IRepository Members
    }
}