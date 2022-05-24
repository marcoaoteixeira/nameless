namespace Nameless.ObjectMapper {

    public static class MapperExtension {

        #region Public Static Methods

        public static TTo? Map<TTo, TFrom>(this IMapper self, TFrom? instance) {
            if (self == null || instance == null) { return default; }

            var value = self.Map(typeof(TFrom), typeof(TTo), instance);

            return value != null ? (TTo)value : default;
        }

        #endregion
    }
}
