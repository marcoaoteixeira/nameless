using Nameless.Web.Hosting;

namespace Nameless.Microservices.Worker;

public class EntryPoint {
    public static void Main(string[] args) {
        WebHostFactory.Create(
            args,
            [
                typeof(EntryPoint).Assembly,
                typeof(AssemblyMarker).Assembly
            ]
        ).Run();
    }
}
