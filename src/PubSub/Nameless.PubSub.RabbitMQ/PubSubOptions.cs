using System.ComponentModel;

namespace Nameless.PubSub.RabbitMQ {

	public class PubSubOptions {

		#region Public Properties

		public Server Server { get; set; } = new();
		public Credentials Credentials { get; set; } = new();
		public Exchange[] Exchanges { get; set; } = Array.Empty<Exchange>();

		#endregion
	}

	public class Server {

		#region Public Properties

		public string Hostname { get; set; } = "http://localhost:5672/";

		#endregion
	}

	public class Credentials {

		#region Public Properties

		public string? Username { get; set; }
		public string? Password { get; set; }

		#endregion
	}

	public class Exchange {

		#region Public Properties

		public string? Name { get; set; }
		public ExchangeType Type { get; set; }
		public bool Durable { get; set; }
		public bool AutoDelete { get; set; }
		public bool AutoAck { get; set; }
		public bool AckMultiple { get; set; }
		public string? RoutingKey { get; set; }

		#endregion
	}

	public enum ExchangeType {
		[Description("topic")]
		Topic = 0,
		[Description("queue")]
		Queue = 1,
		[Description("fanout")]
		Fanout = 2,
		[Description("direct")]
		Direct = 4,
		[Description("headers")]
		Headers = 8
	}
}