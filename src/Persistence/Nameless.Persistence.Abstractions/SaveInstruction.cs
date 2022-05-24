using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Nameless.Persistence {

    public sealed class SaveInstruction<TEntity> where TEntity : class {

        #region Public Properties

        public TEntity Entity { get; }
        public Expression<Func<TEntity, bool>>? Filter { get; }
        public WriteType Type { get; }

        #endregion

        #region Public Constructors

        public SaveInstruction(TEntity entity, Expression<Func<TEntity, bool>>? filter = null, WriteType type = WriteType.Upsert) {
            Ensure.NotNull(entity, nameof(entity));

            Entity = entity;
            Filter = filter;
            Type = type;
        }

        #endregion
    }

    public enum WriteType {

        Insert = 0,

        Update = 1,

        Upsert = 2
    }

    public sealed class SaveInstructionCollection<TEntity> : Collection<SaveInstruction<TEntity>> where TEntity : class {

        #region Public Constructors

        public SaveInstructionCollection(SaveInstruction<TEntity>[]? instructions = null)
            : base(instructions ?? Array.Empty<SaveInstruction<TEntity>>()) { }

        #endregion

        #region Public Static Methods

        public static SaveInstructionCollection<TEntity> Create(params SaveInstruction<TEntity>[] instructions) {
            return new SaveInstructionCollection<TEntity>(instructions);
        }

        #endregion

        #region Public Methods

        public void Add(TEntity entity, Expression<Func<TEntity, bool>>? filter = null, WriteType type = WriteType.Upsert) {
            Add(new SaveInstruction<TEntity>(entity, filter, type));
        }

        #endregion
    }
}
