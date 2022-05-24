using System.Text;
using Nameless.Helpers;

namespace Nameless.FileStorage {

    public static class FileExtension {

        #region Public Static Methods

        public static Stream Open(this IFile self) {
            if (self == null) { return Stream.Null; }

            var stream = AsyncHelper.RunSync(() => self.OpenAsync());
            return stream;
        }

        public static void Copy(this IFile self, string destFilePath, bool overwrite = false) {
            if (self == null) { return; }

            AsyncHelper.RunSync(() => self.CopyAsync(destFilePath, overwrite));
        }

        public static void Move(this IFile self, string destFilePath) {
            if (self == null) { return; }

            AsyncHelper.RunSync(() => self.MoveAsync(destFilePath));
        }

        public static void Delete(this IFile self) {
            if (self == null) { return; }

            AsyncHelper.RunSync(() => self.DeleteAsync());
        }

        public static async Task<string?> GetTextAsync(this IFile self, Encoding? encoding = null) {
            if (self == null) { return null; }

            using var stream = await self.OpenAsync();
            using var streamReader = new StreamReader(stream, encoding ?? Encoding.UTF8);

            return streamReader.ReadToEnd();
        }

        public static string? GetText(this IFile self, Encoding? encoding = null) {
            if (self == null) { return null; }

            return AsyncHelper.RunSync(() => GetTextAsync(self, encoding));
        }

        #endregion
    }
}