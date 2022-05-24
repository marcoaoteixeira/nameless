using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MS_ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Nameless.AspNetCore.Logging {

    public sealed class Logger : MS_ILogger {
        public IDisposable BeginScope<TState>(TState state) {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel) {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
            throw new NotImplementedException();
        }
    }
}
