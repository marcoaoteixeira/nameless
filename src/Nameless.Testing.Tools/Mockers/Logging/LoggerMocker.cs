#pragma warning disable CA1873

using Microsoft.Extensions.Logging;
using Moq;

namespace Nameless.Testing.Tools.Mockers.Logging;

public class LoggerMocker<T> : Mocker<ILogger<T>> {
    public LoggerMocker<T> WithLogTrace(Action<LogLevel, string>? callback = null) {
        return WithLog(LogLevel.Trace, callback);
    }

    public LoggerMocker<T> WithLogDebug(Action<LogLevel, string>? callback = null) {
        return WithLog(LogLevel.Debug, callback);
    }

    public LoggerMocker<T> WithLogInformation(Action<LogLevel, string>? callback = null) {
        return WithLog(LogLevel.Information, callback);
    }

    public LoggerMocker<T> WithLogWarning(Action<LogLevel, string>? callback = null) {
        return WithLog(LogLevel.Warning, callback);
    }

    public LoggerMocker<T> WithLogError(Action<LogLevel, string>? callback = null) {
        return WithLog(LogLevel.Error, callback);
    }

    public LoggerMocker<T> WithLogCritical(Action<LogLevel, string>? callback = null) {
        return WithLog(LogLevel.Critical, callback);
    }

    public LoggerMocker<T> WithAnyLogLevel(Action<LogLevel, string>? callback = null) {
        foreach (var level in Enum.GetValues<LogLevel>()) {
            if (level == LogLevel.None) { continue; }

            WithLog(level, callback);
        }

        return this;
    }
    
    public LoggerMocker<T> VerifyTrace(Func<string, bool>? assertMessage = null, int times = 1, bool exactly = true) {
        return VerifyCallFor(assertMessage, LogLevel.Trace, times, exactly);
    }

    public LoggerMocker<T> VerifyDebug(Func<string, bool>? assertMessage = null, int times = 1, bool exactly = true) {
        return VerifyCallFor(assertMessage, LogLevel.Debug, times, exactly);
    }

    public LoggerMocker<T> VerifyInformation(Func<string, bool>? assertMessage = null, int times = 1, bool exactly = true) {
        return VerifyCallFor(assertMessage, LogLevel.Information, times, exactly);
    }

    public LoggerMocker<T> VerifyWarning(Func<string, bool>? assertMessage = null, int times = 1, bool exactly = true) {
        return VerifyCallFor(assertMessage, LogLevel.Warning, times, exactly);
    }

    public LoggerMocker<T> VerifyError(Func<string, bool>? assertMessage = null, int times = 1, bool exactly = true) {
        return VerifyCallFor(assertMessage, LogLevel.Error, times, exactly);
    }

    public LoggerMocker<T> VerifyCritical(Func<string, bool>? assertMessage = null, int times = 1, bool exactly = true) {
        return VerifyCallFor(assertMessage, LogLevel.Critical, times, exactly);
    }
    
    private LoggerMocker<T> WithLog(LogLevel level, Action<LogLevel, string>? callback = null) {
        MockInstance
            .Setup(mock => mock.IsEnabled(level))
            .Returns(value: true);

        var flow = MockInstance
            .Setup(mock => mock.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ));

        if (callback is not null) {
            flow.Callback(new InvocationAction(invocation => {
                var entry = CaptureEntryFromInvocation(invocation);

                callback.Invoke(entry.Level, entry.Message);
            }));
        }

        return this;
    }

    private LoggerMocker<T> VerifyCallFor(Func<string, bool>? assertMessage, LogLevel level, int times, bool exactly) {
        var stateMessageMatch = (object value, Type type)
            => IsLogValueState(type) &&
               MatchMessage(assertMessage, value);

        MockInstance.Verify(mock => mock.Log(
            It.Is<LogLevel>(currentLogLevel => currentLogLevel == level),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((value, type) => stateMessageMatch(value!, type)),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), exactly ? Times.Exactly(times) : Times.AtLeast(times));

        return this;

        // [LoggerMessage] source generator emits a private nested struct whose
        // *closed* name contains "LogValues". We also accept any IReadOnlyList
        // of KVPs (the common interface), so the matcher is resilient to both
        // generated and hand-rolled callers.
        static bool IsLogValueState(Type type) {
            // Source-generated structs: __LogValues, LogValues, etc.
            return type.Name.Contains("LogValues", StringComparison.OrdinalIgnoreCase)
                   || type.Name.Contains("FormattedLogValues", StringComparison.OrdinalIgnoreCase)
                   || type.Name.Contains("LoggerMessage", StringComparison.OrdinalIgnoreCase)
                   // Fallback: any state that implements the structured-logging contract
                   || typeof(IReadOnlyList<KeyValuePair<string, object?>>).IsAssignableFrom(type);
        }

        static bool MatchMessage(Func<string, bool>? assertMessage, object value) {
            return (assertMessage ?? (_ => true)).Invoke(value.ToString() ?? string.Empty);
        }
    }
    
    private static Entry CaptureEntryFromInvocation(IInvocation invocation) {
        var level = (LogLevel)invocation.Arguments[index: 0]!;
        var state = invocation.Arguments[index: 2];
        var exception = invocation.Arguments[index: 3] as Exception;
        var formatter = invocation.Arguments[index: 4];
        var invoker = formatter!.GetType().GetMethod(name: "Invoke");
        var message = invoker?.Invoke(formatter, [state, exception])?.ToString()
               ?? state!.ToString()
               ?? string.Empty;

        return new Entry(level, message);
    }

    internal record Entry(LogLevel Level, string Message) {
        internal static Entry Empty => new(LogLevel.None, string.Empty);
    }
}