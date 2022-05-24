namespace Nameless.Security {

    public class PasswordGeneratorOptions {

        #region Public Properties

        /// <summary>
        /// Gets or sets the mininum length for the password.
        /// </summary>
        /// <remarks>Default is 6</remarks>
        public int MinLength { get; set; } = 6;
        /// <summary>
        /// Gets or sets the maximun length for the password.
        /// </summary>
        /// <remarks>Default is 12</remarks>
        public int MaxLength { get; set; } = 12;
        /// <summary>
        /// Gets or sets whether will use special chars. Special chars: *$-+?_&=!%{}/
        /// </summary>
        /// <remarks>Default is <c>true</c></remarks>
        public bool UseSpecialChars { get; set; } = true;
        /// <summary>
        /// Gets or sets whether will use numbers. Numbers: 0123456789
        /// </summary>
        /// <remarks>Default is <c>true</c></remarks>
        public bool UseNumeric { get; set; } = true;
        /// <summary>
        /// Gets or sets whether will use lower case chars. Lower case chars: abcdefgijkmnopqrstwxyz
        /// </summary>
        /// <remarks>Default is <c>true</c></remarks>
        public bool UseLowerCase { get; set; } = true;
        /// <summary>
        /// Gets or sets whether will use upper case chars. Upper case chars: ABCDEFGIJKMNOPQRSTWXYZ
        /// </summary>
        /// <remarks>Default is <c>true</c></remarks>
        public bool UseUpperCase { get; set; } = true;

        #endregion
    }
}
