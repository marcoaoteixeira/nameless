using Nameless.Helpers;

namespace Nameless.FileStorage {

    public static class FileExtension {

        #region Public Static Methods

        public static Stream Open(this IFile self) {
            if (self == null) { return Stream.Null; }

            return AsyncHelper.RunSync(() => self.OpenAsync());
        }

        #endregion
    }
}
