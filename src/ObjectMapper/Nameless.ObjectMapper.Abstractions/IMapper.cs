using System;

namespace Nameless.ObjectMapper {

	public interface IMapper {

		#region Methods

		object? Map(Type? from, Type? to, object? instance);

		#endregion
	}
}
