namespace Nameless {

    public static class ArrayExtension {

        #region Public Static Methods

        public static bool TryGetByIndex<T>(this T[] self, int index, out T? output) {
            output = default;

            if (self == null) { return false; }

            if (self.Length > index) {
                output = self[index];
                return true;
            }

            return false;
        }

        #endregion
    }
}
