using System.Collections;
using MS_IHostEnvironment = Microsoft.Extensions.Hosting.IHostEnvironment;
using Sys_Environment = System.Environment;

namespace Nameless.Environment.System {

    public sealed class HostEnvironment : IHostEnvironment {

        #region Private Read-Only Fields

        private readonly MS_IHostEnvironment _hostEnvironment;

        #endregion

        #region Public Constructors

        public HostEnvironment(MS_IHostEnvironment hostEnvironment) {
            _hostEnvironment = hostEnvironment ?? throw new ArgumentNullException(nameof(hostEnvironment));
        }

        #endregion

        #region Private Static Methods

        private static EnvironmentVariableTarget Parse(VariableTarget target) {
            return target switch {
                VariableTarget.Process => EnvironmentVariableTarget.Process,
                VariableTarget.User => EnvironmentVariableTarget.User,
                VariableTarget.Machine => EnvironmentVariableTarget.Machine,
                _ => EnvironmentVariableTarget.User,
            };
        }

        #endregion

        #region IHostingEnvironment Members

        public string EnvironmentName => _hostEnvironment.EnvironmentName;

        public string ApplicationName => _hostEnvironment.ApplicationName;

        public string ApplicationBasePath => _hostEnvironment.ContentRootPath;

        public object? GetData(string key) => AppDomain.CurrentDomain.GetData(key);

        public void SetData(string key, object data) => AppDomain.CurrentDomain.SetData(key, data);

        public string? GetVariable(string key, VariableTarget target = VariableTarget.User) {
            return Sys_Environment.GetEnvironmentVariable(key, Parse(target));
        }

        public IDictionary<string, string?> GetVariables(VariableTarget target) {
            var variables = Sys_Environment.GetEnvironmentVariables(Parse(target));
            var result = new Dictionary<string, string?>();
            if (variables != null) {
                foreach (DictionaryEntry kvp in variables) {
                    result.Add((string)kvp.Key, kvp.Value as string);
                }
            }
            return result;
        }

        public void SetVariable(string key, string variable, VariableTarget target = VariableTarget.User) {
            Sys_Environment.SetEnvironmentVariable(key, variable, Parse(target));
        }

        #endregion
    }
}