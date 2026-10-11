using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nameless.ObjectModel;

namespace Nameless.Web.ObjectModel;

public static class ErrorExtensions {
    extension(IEnumerable<Error> self) {
        /// <summary>
        ///     Converts the array of <see cref="Error"/> into a
        ///     <see cref="IResult"/> object.
        /// </summary>
        /// <returns>
        ///     The corresponding HTTP result.
        /// </returns>
        public IResult ToHttpResult() {
            var array = self.ToArray();

            if (array.Any(error => error.Type is ErrorType.Unauthorized)) {
                return TypedResults.Unauthorized();
            }

            if (array.Any(error => error.Type is ErrorType.Forbidden)) {
                return TypedResults.Forbid();
            }

            if (array.Any(error => error.Type is ErrorType.Missing)) {
                return TypedResults.NotFound(string.Join(
                    separator: "; ",
                    values: array
                            .Where(error => error.Type == ErrorType.Missing)
                            .Select(error => error.Message)
                ));
            }

            if (array.Any(error => error.Type is ErrorType.Validation)) {
                return TypedResults.ValidationProblem(
                    array.ToDictionary(ErrorType.Validation, useMessageOnly: true)
                );
            }

            return TypedResults.Problem(
                array.Flatten()
            );
        }

        public ProblemDetails ToProblemDetails() {
            return new ProblemDetails {
                Detail = string.Join("; ", self.Select(error => error.Message))
            };
        }

        public Dictionary<string, string[]> ToDictionary() {
            return self.GroupBy(error => error.Code).ToDictionary(
                keySelector: group => group.Key ?? string.Empty,
                elementSelector: group => group.Select(item => item.Message).ToArray()
            );
        }
    }
}
