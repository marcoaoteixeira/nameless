using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Nameless.Persistence {

    public sealed class DeleteInstruction<TEntity> where TEntity : class {

        #region Public Properties

        public Expression<Func<TEntity, bool>> Filter { get; }

        #endregion

        #region Public Constructors

        public DeleteInstruction(Expression<Func<TEntity, bool>> filter) {
            Ensure.NotNull(filter, nameof(filter));

            Filter = filter;
        }

        #endregion
    }

    public sealed class DeleteInstructionCollection<TEntity> : Collection<DeleteInstruction<TEntity>> where TEntity : class {

        #region Public Constructors

        public DeleteInstructionCollection(DeleteInstruction<TEntity>[]? instructions = null)
            : base(instructions ?? Array.Empty<DeleteInstruction<TEntity>>()) { }

        #endregion

        #region Public Static Methods

        public static DeleteInstructionCollection<TEntity> Create(params DeleteInstruction<TEntity>[] instructions) {
            return new DeleteInstructionCollection<TEntity>(instructions);
        }

        #endregion

        #region Public Methods

        public void Add(Expression<Func<TEntity, bool>> filter) {
            Add(new DeleteInstruction<TEntity>(filter));
        }

        #endregion
    }
}
