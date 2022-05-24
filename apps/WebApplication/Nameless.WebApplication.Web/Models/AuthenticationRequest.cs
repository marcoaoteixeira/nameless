using System.ComponentModel.DataAnnotations;

namespace Nameless.WebApplication.Web.Models {

    public sealed class AuthenticationRequest {

        #region Public Properties

        [Required]
        public string? Identification { get; set; }

        [Required]
        public string? Password { get; set; }

        #endregion
    }
}
