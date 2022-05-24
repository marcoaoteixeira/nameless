using System.Data;

namespace Nameless.Data {

	/// <summary>
	/// Represents a command parameter.
	/// </summary>
	public sealed class Parameter {

		#region Public Properties

		/// <summary>
		/// Gets the parameter name.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the parameter type.
		/// </summary>
		public DbType Type { get; }

		/// <summary>
		/// Gets or sets the parameter value.
		/// </summary>
		public object? Value { get; set; }

		/// <summary>
		/// Gets the parameter direction.
		/// </summary>
		public ParameterDirection Direction { get; }

		#endregion

		#region Private Constructors

		private Parameter(string name, object? value, DbType type, ParameterDirection direction) {
			Name = name;
			Value = value;
			Type = type;
			Direction = direction;
		}

		#endregion

		#region Public Static Methods

		/// <summary>
		/// Creates an input parameter.
		/// </summary>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		/// <param name="type">The parameter type (default: <see cref="DbType.String"/>.</param>
		/// <returns>An instance of <see cref="Parameter"/>.</returns>
		public static Parameter CreateInputParameter(string name, object value, DbType type = DbType.String) {
			return new Parameter(name, value, type, ParameterDirection.Input);
		}

		/// <summary>
		/// Creates an output parameter.
		/// </summary>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		/// <param name="type">The parameter type (default: <see cref="DbType.String"/>.</param>
		/// <returns>An instance of <see cref="Parameter"/>.</returns>
		public static Parameter CreateOutputParameter(string name, object value, DbType type = DbType.String) {
			return new Parameter(name, value, type, ParameterDirection.Output);
		}

		/// <summary>
		/// Creates a return value parameter.
		/// </summary>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		/// <param name="type">The parameter type (default: <see cref="DbType.String"/>.</param>
		/// <returns>An instance of <see cref="Parameter"/>.</returns>
		public static Parameter CreateReturnValueParameter(string name, object value, DbType type = DbType.String) {
			return new Parameter(name, value, type, ParameterDirection.ReturnValue);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Determines whether this instance and another specified <see cref="Parameter"/> object havethe same value.
		/// </summary>
		/// <param name="obj">The <see cref="Parameter"/> to compare to this instance.</param>
		/// <returns>
		/// <c>true</c> if the value of the value parameter is the same as the value of this instance;
		/// otherwise, <c>false</c>. If value is null, the method returns <c>false</c>.
		/// </returns>
		public bool Equals(Parameter? obj) {
			return obj != null && obj.Name == Name;
		}

		#endregion

		#region Public Override Methods

		/// <inheritdoc />
		public override bool Equals(object? obj) => Equals(obj as Parameter);

		/// <inheritdoc />
		public override int GetHashCode() {
			var hash = 13;
			unchecked {
				hash += (Name ?? string.Empty).GetHashCode() * 7;
			}
			return hash;
		}

		/// <inheritdoc />
		public override string ToString() {
			return $"({Direction}) {Name}: {Value}";
		}

		#endregion
	}
}