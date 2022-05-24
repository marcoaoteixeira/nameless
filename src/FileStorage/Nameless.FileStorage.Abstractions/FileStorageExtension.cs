using Nameless.Helpers;

namespace Nameless.FileStorage {

    public static class FileStorageExtension {

        #region Public Static Methods

        public static bool CreateDirectory(this IFileStorage self, string relativePath) {
            if (self == null) { return false; }

            return AsyncHelper.RunSync(() => self.CreateDirectoryAsync(relativePath));
        }

        public static IDirectory? GetDirectory(this IFileStorage self, string relativePath) {
            if (self == null) { return null; }

            return AsyncHelper.RunSync(() => self.GetDirectoryAsync(relativePath));
        }

        public static void CreateFile(this IFileStorage self, string relativePath, Stream input, bool overwrite = false) {
            if (self == null) { return; }

            AsyncHelper.RunSync(() => self.CreateFileAsync(relativePath, input, overwrite));
        }

        public static IFile? GetFile(this IFileStorage self, string relativePath) {
            if (self == null) { return null; }

            return AsyncHelper.RunSync(() => self.GetFileAsync(relativePath));
        }

        #endregion
    }
}