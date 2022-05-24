using System.Linq.Expressions;

namespace Nameless.Persistence {

    public sealed class Repository : IRepository {

        #region Private Read-Only Fields

        private readonly IDirectiveExecutor _directiveExecutor;
        private readonly IWriter _writer;
        private readonly IReader _reader;

        #endregion Private Read-Only Fields

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Repository" />
        /// </summary>
        /// <param name="directiveExecutor">The directive executor.</param>
        /// <param name="reader">The querier.</param>
        /// <param name="writer">The persister.</param>
        public Repository(IDirectiveExecutor directiveExecutor, IReader reader, IWriter writer) {
            Ensure.NotNull(directiveExecutor, nameof(directiveExecutor));
            Ensure.NotNull(reader, nameof(reader));
            Ensure.NotNull(writer, nameof(writer));

            _directiveExecutor = directiveExecutor;
            _reader = reader;
            _writer = writer;
        }

        #endregion Public Constructors

        #region IRepository Members

        /// <inheritdoc />
        public Task<TResult?> ExecuteDirectiveAsync<TResult, TDirective>(ParameterSet parameters, CancellationToken cancellationToken = default) where TDirective : IDirective<TResult?> {
            return _directiveExecutor.ExecuteDirectiveAsync<TResult?, TDirective>(parameters, cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<TEntity> FindAsync<TEntity>(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class {
            return _reader.FindAsync(filter, cancellationToken);
        }

        /// <inheritdoc />
        public IQueryable<TEntity> Query<TEntity>() where TEntity : class {
            return _reader.Query<TEntity>();
        }

        /// <inheritdoc />
        public Task SaveAsync<TEntity>(SaveInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class {
            return _writer.SaveAsync(instructions, cancellationToken);
        }

        /// <inheritdoc />
        public Task DeleteAsync<TEntity>(DeleteInstructionCollection<TEntity> instructions, CancellationToken cancellationToken = default) where TEntity : class {
            return _writer.DeleteAsync(instructions, cancellationToken);
        }

        #endregion IRepository Members
    }
}