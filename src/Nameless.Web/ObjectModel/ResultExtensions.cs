using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Nameless.ObjectModel;

namespace Nameless.Web.ObjectModel;

public static class ResultExtensions {
    extension<T>(Result<T> self) {
        /// <summary>
        ///     Translates the current <see cref="Result{T}"/> to an instance
        ///     of <see cref="IResult"/>.
        /// </summary>
        /// <remarks>
        ///     On <see cref="Result{T}.Success"/> returns a
        ///     <see cref="Ok{TValue}"/> instance; otherwise the appropriate
        ///     error result.
        /// </remarks>
        /// <returns>
        ///     An instance of <see cref="IResult"/>.
        /// </returns>
        public IResult ToHttpResult() {
            return self.Match(
                onSuccess: TypedResults.Ok,
                onFailure: errors => errors.ToHttpResult()
            );
        }
    }
}
