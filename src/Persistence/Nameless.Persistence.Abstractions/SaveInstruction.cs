using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Nameless.Persistence {

    public sealed class SaveInstruction<TEntity> where TEntity : class {

        #region Public Properties

        public TEntity Entity { get; private set; }
        public Expression<Func<TEntity, bool>>? Filter { get; private set; }
        public Expression<Func<TEntity, TEntity>>? Patch { get; private set; }
        public SaveMode Mode { get; private set; }

        #endregion

        #region Private Constructors

        private SaveInstruction() {
            Entity = default!;
        }

        #endregion

        #region Public Static Methods

        public static SaveInstruction<TEntity> Insert(TEntity entity) {
            Prevent.Null(entity, nameof(entity));

            return new SaveInstruction<TEntity> {
                Entity = entity,
                Filter = default,
                Patch = default,
                Mode = SaveMode.Insert
            };
        }

        public static SaveInstruction<TEntity> Update(TEntity entity) {
            Prevent.Null(entity, nameof(entity));

            return new SaveInstruction<TEntity> {
                Entity = entity,
                Filter = default,
                Patch = default,
                Mode = SaveMode.Update
            };
        }

        public static SaveInstruction<TEntity> Update(TEntity entity, Expression<Func<TEntity, bool>> filter) {
            Prevent.Null(entity, nameof(entity));
            Prevent.Null(filter, nameof(filter));

            return new SaveInstruction<TEntity> {
                Entity = entity,
                Filter = filter,
                Patch = default,
                Mode = SaveMode.Update
            };
        }

        public static SaveInstruction<TEntity> Update(Expression<Func<TEntity, TEntity>> patch, Expression<Func<TEntity, bool>> filter) {
            Prevent.Null(patch, nameof(patch));
            Prevent.Null(filter, nameof(filter));

            return new SaveInstruction<TEntity> {
                Entity = default!,
                Filter = filter,
                Patch = patch,
                Mode = SaveMode.Update
            };
        }

        public static SaveInstruction<TEntity> UpSert(TEntity entity) {
            Prevent.Null(entity, nameof(entity));

            return new SaveInstruction<TEntity> {
                Entity = entity,
                Filter = default,
                Patch = default,
                Mode = SaveMode.UpSert
            };
        }

        public static SaveInstruction<TEntity> UpSert(TEntity entity, Expression<Func<TEntity, bool>> filter) {
            Prevent.Null(entity, nameof(entity));
            Prevent.Null(filter, nameof(filter));

            return new SaveInstruction<TEntity> {
                Entity = entity,
                Filter = filter,
                Patch = default,
                Mode = SaveMode.UpSert
            };
        }

        #endregion
    }

    public enum SaveMode {
        Insert = 0,

        Update = 1,

        UpSert = 2
    }

    public sealed class SaveInstructionCollection<TEntity> : Collection<SaveInstruction<TEntity>> where TEntity : class {

    }
}
