namespace Nameless.ObjectModel;

/// <summary>
///     <see cref="Error"/> extension methods.
/// </summary>
public static class ErrorExtensions {
    extension(Error) {
        /// <summary>
        ///     Creates an <see cref="Error"/> informing about the current
        ///     operation cancellation. Message defaults to
        ///     <c>Operation was canceled unexpectedly.</c>
        /// </summary>
        /// <param name="ex">
        ///     The operation canceled exception.
        /// </param>
        /// <returns>
        ///     An <see cref="Error"/> (Conflict) instance.
        /// </returns>
        public static Error OperationCanceled(OperationCanceledException? ex = null) {
            return Error.Failure("Operation was canceled unexpectedly.", ex: ex);
        }
    }

    extension(IEnumerable<Error> self) {
        /// <summary>
        ///     Folds all errors into a single error, by type.
        /// </summary>
        /// <param name="type">
        ///     Type of the error.
        /// </param>
        /// <returns>
        ///     A single error.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     if no errors are available.
        /// </exception>
        public Error AggregateByType(ErrorType type) {
            var errors = self.Where(error => error.Type == type).ToArray();

            switch (errors.Length) {
                case 0: throw new InvalidOperationException(
                    $"No errors of type '{type}' to aggregate."
                );
                case 1: return errors[0];
            }

            var codes = errors.Select(error => error.Code)
                              .Where(code => !string.IsNullOrWhiteSpace(code))
                              .ToArray();

            var exceptions = errors.Select(error => error.Exception)
                                   .OfType<Exception>()
                                   .ToArray();

            const string Separator = "; ";

            return new Error(
                message: string.Join(Separator, errors.Select(error => error.Message)),
                code: codes.Length > 0 ? string.Join(Separator, codes) : null,
                type: type,
                ex: exceptions.Length switch {
                    0 => null,
                    1 => exceptions[0],
                    _ => new AggregateException(exceptions)
                }
            );
        }

        /// <summary>
        ///     Retrieves a flattened version of the error collection.
        /// </summary>
        /// <param name="type">
        ///     If provided, filter errors by its type.
        /// </param>
        /// <param name="separator">
        ///     The separator to use. Default is semicolon.
        /// </param>
        /// <returns>
        ///     A string value representing a semicolon separated value
        ///     of flatten errors.
        /// </returns>
        public string Flatten(ErrorType? type = null, char separator = ';') {
            var flattenByType = self.Where(error => type is null || error.Type == type)
                                    .GroupBy(error => error.Type)
                                    .Select(group => $"[{group.Key}] {GetMessage(group)}");

            return string.Join(separator, flattenByType);

            static string GetMessage(IGrouping<ErrorType, Error> group) {
                var messages = group.Select(
                    item => !string.IsNullOrWhiteSpace(item.Code)
                        ? $"({item.Code}) {item.Message}"
                        : item.Message
                );

                return string.Join("; ", messages);
            }
        }

        /// <summary>
        ///     Retrieves a dictionary version of the collection of errors.
        /// </summary>
        /// <param name="type">
        ///     If provided, filter errors by its type.
        /// </param>
        /// <param name="useMessageOnly">
        ///     Whether it should use only the message part as values or
        ///     flatten.
        /// </param>
        /// <returns>
        ///     An instance of <see cref="Dictionary{TKey,TValue}"/> class.
        /// </returns>
        public Dictionary<string, string[]> ToDictionary(ErrorType? type = null, bool useMessageOnly = true) {
            return self.Where(error => type is null || error.Type == type)
                       .GroupBy(self => self.Code ?? string.Empty)
                       .ToDictionary(
                           keySelector: item => item.Key,
                           elementSelector: item => item.Select(error => useMessageOnly ? error.Message : error.Flatten).ToArray()
                       );
        }
    }
}
