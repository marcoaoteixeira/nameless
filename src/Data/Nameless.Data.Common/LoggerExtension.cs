using System.Data;
using System.Text;
using Nameless.Logging;

namespace Nameless.Data {

    internal static class LoggerExtension {

        #region Internal Static Methods

        internal static void DebugOrInfo(this ILogger self, IDbCommand command) {
            if (!self.IsEnabled(LogLevel.Information) || !self.IsEnabled(LogLevel.Debug)) { return; }

            var sb = new StringBuilder();

            sb.AppendLine($"***** COMMAND EXECUTED *****");
            sb.AppendLine(command.CommandText);
            sb.AppendLine();

            sb.AppendLine($"***** COMMAND PARAMETERS *****");
            command.Parameters.OfType<IDbDataParameter>().Each(parameter => {
                sb.AppendLine($"[{parameter.Direction}]");
                sb.AppendLine($"{parameter.ParameterName} {parameter.DbType}: {parameter.Value}");
                sb.AppendLine();
            });

            var log = sb.ToString();

            self.Debug(log);
            self.Information(log);
        }

        #endregion
    }
}
