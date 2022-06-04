using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Nameless.Persistence {

    public sealed class DeleteInstruction<TEntity> where TEntity : class {

        #region Public Properties

        public Expression<Func<TEntity, bool>> Filter { get; }

        #endregion

        #region Private Constructors

        private DeleteInstruction(Expression<Func<TEntity, bool>> filter) {
            Prevent.Null(filter, nameof(filter));

            Filter = filter;
        }

        #endregion

        #region Public Static Methods

        public static DeleteInstruction<TEntity> Create(Expression<Func<TEntity, bool>> filter) {
            Prevent.Null(filter, nameof(filter));

            return new DeleteInstruction<TEntity>(filter);
        }

        #endregion
    }

    public sealed class DeleteInstructionCollection<TEntity> : Collection<DeleteInstruction<TEntity>> where TEntity : class {

        #region Public Methods

        public DeleteInstructionCollection<TEntity> Add(Expression<Func<TEntity, bool>> filter) {
            Add(DeleteInstruction<TEntity>.Create(filter));
            return this;
        }

        #endregion
    }
}
