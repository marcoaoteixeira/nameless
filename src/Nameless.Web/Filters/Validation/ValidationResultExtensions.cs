using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nameless.ObjectModel;

namespace Nameless.Web.Filters.Validation;

public static class ErrorExtensions {
    extension(IEnumerable<Error> self) {
        public ModelStateDictionary ToModelStateDictionary() {
            var result = new ModelStateDictionary();
            
            foreach (var error in self) {
                result.AddModelError(
                    error.Code ?? string.Empty,
                    error.Message
                );
            }

            return result;
        }
    }
}