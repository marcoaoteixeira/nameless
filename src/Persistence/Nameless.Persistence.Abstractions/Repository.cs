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
        public Task<int> SaveAsync<TEntity>(SaveInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class {
            return _writer.SaveAsync(instructions, cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> DeleteAsync<TEntity>(DeleteInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class {
            return _writer.DeleteAsync(instructions, cancellationToken);
        }

        /// <inheritdoc />
        public Task<IList<TEntity>> FindAsync<TEntity>(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>>? orderBy = null, bool orderDescending = false, CancellationToken cancellationToken = default) where TEntity : class {
            return _reader.FindAsync(filter, orderBy, orderDescending, cancellationToken);
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class {
            return _reader.ExistsAsync(filter, cancellationToken);
        }

        /// <inheritdoc />
        public IQueryable<TEntity> Query<TEntity>() where TEntity : class {
            return _reader.Query<TEntity>();
        }

        #endregion
    }
}