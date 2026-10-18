using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Nameless.Windows.Configuration;
using Nameless.Windows.Documents;
using Nameless.Windows.Localization;
using Nameless.Windows.UI.Impl;

namespace Nameless.Windows;

[ExcludeFromCodeCoverage]
internal static partial class Log {
    #region Common

    [LoggerMessage(level: LogLevel.Error, message: "[{Tag}] An error has occurred while executing action '{ActionName}'.")]
    internal static partial void Failure(ILogger logger, string tag, string actionName, Exception exception);

    [LoggerMessage(level: LogLevel.Warning, message: "[{Tag}] A problem has occurred while executing action '{ActionName}'. Reason: {Reason}")]
    internal static partial void Warning(ILogger logger, string tag, string actionName, string reason);

    #endregion

    #region AppConfigurationManager

    [LoggerMessage(level: LogLevel.Error, message: "[APP_CONFIGURATION_MANAGER] An error occurred while trying to get the configuration value for '{Key}' as '{Type}'.")]
    internal static partial void AppConfigurationManagerTryGetFailure(ILogger<IAppConfigurationManager> logger, string key, string type, Exception exception);

    #endregion

    #region DocumentReader

    [LoggerMessage(level: LogLevel.Error, message: "[DOCUMENT_READER] An error occurred while trying to read the document content. File: {FilePath}")]
    internal static partial void DocumentReaderGetContentFailure(ILogger<IDocumentReader> logger, string filePath, Exception exception);

    #endregion

    #region ResourceLocalizer

    [LoggerMessage(level: LogLevel.Debug, message: "[LOCALIZER] Missing key '{Key}'.")]
    internal static partial void ResourceLocalizerMissingKey(ILogger<ResourceLocalizer> logger, string key);

    #endregion

    #region XpsDocumentConverter

    [LoggerMessage(level: LogLevel.Error, message: "[XPS_DOCUMENT_CONVERTER] An error occurred while trying to convert XPS document. File: {FilePath}")]
    internal static partial void XpsDocumentConverterConvertFailure(ILogger<XpsDocumentConverter> logger, string filePath, Exception exception);

    #endregion

    #region WindowFactory

    [LoggerMessage(level: LogLevel.Error, message: "[WINDOW_FACTORY] An error occurred while trying to create a new instance of the window '{Window}'. The error may be caused by the window not being registered in the dependency container.")]
    internal static partial void WindowFactoryCreateFailure(ILogger<WindowFactory> logger, Type window, Exception exception);

    #endregion

    #region Hosting

    [LoggerMessage(level: LogLevel.Information, message: "Registering feature: {Feature}")]
    internal static partial void RegisteringFeature(ILogger logger, string feature);

    [LoggerMessage(level: LogLevel.Critical, message: "{Message}")]
    internal static partial void HostBuildFailure(ILogger logger, string message, Exception exception);

    #endregion
}
