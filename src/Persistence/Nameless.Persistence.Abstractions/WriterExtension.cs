using System.Linq.Expressions;
using Nameless.Helpers;

namespace Nameless.Persistence {

    public static class WriterExtension {

        #region Public Static Methods

        public static TEntity Save<TEntity>(this IWriter self, TEntity entity, Expression<Func<TEntity, bool>>? filter = null) where TEntity : class {
            Prevent.Null(self, nameof(self));

            return AsyncHelper.RunSync(() => self.SaveAsync(entity, filter));
        }

        public static bool Delete<TEntity>(this IWriter self, Expression<Func<TEntity, bool>> filter) where TEntity : class {
            Prevent.Null(self, nameof(self));

            return AsyncHelper.RunSync(() => self.DeleteAsync(filter));
        }

        #endregion
    }
}
