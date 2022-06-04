using System.Linq.Expressions;
using Nameless.Helpers;

namespace Nameless.Persistence {

    public static class WriterExtension {

        #region Public Static Methods

        public static Task<int> SaveAsync<TEntity>(this IWriter self, SaveInstruction<TEntity> instruction, CancellationToken cancellationToken = default) where TEntity : class {
            Prevent.Null(self, nameof(self));
            Prevent.Null(instruction, nameof(instruction));

            var instructions = new SaveInstructionCollection<TEntity> {
                instruction
            };

            return self.SaveAsync(instructions, cancellationToken);
        }

        public static Task<int> DeleteAsync<TEntity>(this IWriter self, Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default) where TEntity : class {
            Prevent.Null(self, nameof(self));
            Prevent.Null(filter, nameof(filter));

            var instructions = new DeleteInstructionCollection<TEntity> {
                DeleteInstruction<TEntity>.Create(filter)
            };

            return self.DeleteAsync(instructions, cancellationToken);
        }

        public static Task<int> DeleteAsync<TEntity>(this IWriter self, DeleteInstruction<TEntity> instruction, CancellationToken cancellationToken = default) where TEntity : class {
            Prevent.Null(self, nameof(self));
            Prevent.Null(instruction, nameof(instruction));

            var instructions = new DeleteInstructionCollection<TEntity> {
                instruction
            };

            return self.DeleteAsync(instructions, cancellationToken);
        }

        public static int Delete<TEntity>(this IWriter self, Expression<Func<TEntity, bool>> filter) where TEntity : class {
            Prevent.Null(self, nameof(self));
            Prevent.Null(filter, nameof(filter));

            var instructions = new DeleteInstructionCollection<TEntity> {
                DeleteInstruction<TEntity>.Create(filter)
            };

            return AsyncHelper.RunSync(() => self.DeleteAsync(instructions, cancellationToken: default));
        }

        public static int Delete<TEntity>(this IWriter self, DeleteInstruction<TEntity> instruction) where TEntity : class {
            Prevent.Null(self, nameof(self));
            Prevent.Null(instruction, nameof(instruction));

            var instructions = new DeleteInstructionCollection<TEntity> {
                instruction
            };

            return AsyncHelper.RunSync(() => self.DeleteAsync(instructions, cancellationToken: default));
        }

        #endregion
    }
}
