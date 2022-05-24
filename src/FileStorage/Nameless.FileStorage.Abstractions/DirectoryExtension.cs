using Nameless.Helpers;

namespace Nameless.FileStorage {

    public static class DirectoryExtension {

        #region Public Static Methods

        public static IEnumerable<IFile> GetFiles(this IDirectory self, bool includeSubDirectories = false) {
            if (self == null) { return Enumerable.Empty<IFile>(); }

            return AsyncHelper.RunSync(async () => {
                var enumerator = self.GetFilesAsync(includeSubDirectories).GetAsyncEnumerator();
                var result = new List<IFile>();
                while (await enumerator.MoveNextAsync()) {
                    result.Add(enumerator.Current);
                }
                return result;
            });
        }

        public static IEnumerable<IDirectory> GetDirectories(this IDirectory self, bool includeSubDirectories = false) {
            if (self == null) { return Enumerable.Empty<IDirectory>(); }

            return AsyncHelper.RunSync(async () => {
                var enumerator = self.GetDirectoriesAsync(includeSubDirectories).GetAsyncEnumerator();
                var result = new List<IDirectory>();
                while (await enumerator.MoveNextAsync()) {
                    result.Add(enumerator.Current);
                }
                return result;
            });
        }

        public static void Delete(this IDirectory self) {
            if (self == null) { return; }

            AsyncHelper.RunSync(() => self.DeleteAsync());
        }

        #endregion
    }
}