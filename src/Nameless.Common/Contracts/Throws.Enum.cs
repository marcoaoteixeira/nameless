// ReSharper disable ClassCannotBeInstantiated
#pragma warning disable CA1822

using System.Globalization;
using System.Runtime.CompilerServices;

namespace Nameless;

public sealed partial class Throws {
    /// <summary>
    ///     Throws exception if <paramref name="paramValue"/> is not defined
    ///     by its enum type.
    /// </summary>
    /// <param name="paramValue">
    ///     The parameter value.
    /// </param>
    /// <param name="paramName">
    ///     The parameter name.
    /// </param>
    /// <param name="message">
    ///     The message.
    /// </param>
    /// <param name="exceptionCreator">
    ///     The exception factory.
    /// </param>
    /// <returns>
    ///     The current type if pass validation.
    /// </returns>
    public TEnum UnknownEnum<TEnum>(TEnum paramValue, [CallerArgumentExpression(nameof(paramValue))] string? paramName = null, string? message = null, Func<Exception>? exceptionCreator = null)
        where TEnum : struct, Enum {
        if (Enum.IsDefined(paramValue)) {
            return paramValue;
        }

        throw exceptionCreator?.Invoke()
              ?? new ArgumentOutOfRangeException(
                  paramName,
                  paramValue,
                  message: string.IsNullOrWhiteSpace(message)
                      ? string.Format(CultureInfo.InvariantCulture, PARAM_ENUM_NOT_DEFINED_MESSAGE, typeof(TEnum).Name)
                      : message);
    }
}
