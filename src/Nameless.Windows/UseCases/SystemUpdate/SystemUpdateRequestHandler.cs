using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nameless.Application;
using Nameless.GitHub;
using Nameless.IO;
using Nameless.ObjectModel;
using Nameless.Windows.Messaging;

namespace Nameless.Windows.UseCases.SystemUpdate;

/// <summary>
///     System update request handler.
/// </summary>
public class SystemUpdateRequestHandler : RequestHandlerBase<SystemUpdateRequestHandler, SystemUpdateRequest, SystemUpdateResponse> {
    private readonly IApplicationContext _applicationContext;
    private readonly IGitHubHttpClient _githubHttpClient;
    private readonly GitHubOptions _githubOpts;
    private readonly HttpClient _httpClient;
    private readonly TimeProvider _timeProvider;

    public SystemUpdateRequestHandler(
        IApplicationContext applicationContext,
        IGitHubHttpClient githubHttpClient,
        HttpClient httpClient,
        IMessenger messenger,
        IOptions<GitHubOptions> githubOpts,
        TimeProvider timeProvider,
        ILogger<SystemUpdateRequestHandler> logger)
        : base(messenger, logger) {
        _applicationContext = applicationContext;
        _githubHttpClient = githubHttpClient;
        _githubOpts = githubOpts.Value;
        _httpClient = httpClient;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public override async Task<SystemUpdateResponse> HandleAsync(SystemUpdateRequest request, CancellationToken cancellationToken) {
        var latestRelease = await GetLatestReleaseAsync(cancellationToken).SkipContextSync();
        if (latestRelease.Failure) {
            return latestRelease.Errors;
        }

        if (!latestRelease.Value.IsAvailable) {
            return (SystemUpdateMetadata)default;
        }

        var latestReleaseAsset = await GetLatestReleaseAssetAsync(latestRelease.Value, cancellationToken).SkipContextSync();
        if (latestReleaseAsset.Failure) {
            return latestReleaseAsset.Errors;
        }

        if (string.IsNullOrWhiteSpace(latestReleaseAsset.Value.AssetName)) {
            return (SystemUpdateMetadata)default;
        }

        var downloadUpdate = await DownloadUpdateAsync(latestReleaseAsset.Value, cancellationToken).SkipContextSync();

        return downloadUpdate.Match<SystemUpdateResponse>(
            onSuccess: value => new SystemUpdateMetadata(
                IsAvailable: true,
                Version: latestRelease.Value.Version,
                ZipFilePath: value
            ),
            onFailure: failure => failure
        );
    }

    private async Task<Result<ReleaseInfo>> GetLatestReleaseAsync(CancellationToken cancellationToken) {
        await NotifyInformationAsync("Checking from system update information...").SkipContextSync();

        var result = await _githubHttpClient.GetLastestReleaseAsync(_githubOpts.Owner, _githubOpts.Repository, cancellationToken)
                                            .SkipContextSync();

        if (result.Failure) {
            await NotifyFailureAsync(result.Errors.Flatten()).SkipContextSync();

            return result.Errors;
        }

        if (!SemVersion.TryParse(_applicationContext.Version, out var currentVersion)) {
            currentVersion = SemVersion.Empty;
        }

        if (!SemVersion.TryParse(result.Value.TagName, out var latestVersion)) {
            latestVersion = SemVersion.Empty;
        }

        if (currentVersion >= latestVersion) {
            await NotifySuccessAsync("The current version is the latest.").SkipContextSync();

            return (ReleaseInfo)default;
        }

        await NotifySuccessAsync($"A new version '{latestVersion}' is available.").SkipContextSync();

        return new ReleaseInfo(
            IsAvailable: true,
            Version: latestVersion.ToString(),
            ReleaseID: result.Value.Id
        );
    }

    private async Task<Result<ReleaseAssetInfo>> GetLatestReleaseAssetAsync(ReleaseInfo info, CancellationToken cancellationToken) {
        var result = await _githubHttpClient.GetReleaseAssetsAsync(_githubOpts.Owner, _githubOpts.Repository, info.ReleaseID, cancellationToken)
                                            .SkipContextSync();

        if (result.Failure) {
            await NotifyFailureAsync(result.Errors.Flatten()).SkipContextSync();

            return result.Errors;
        }

        var assetName = $"{_applicationContext.ApplicationName}.v{info.Version}.zip";
        var asset = result.Value.SingleOrDefault(item => item.Name == assetName);

        if (asset is null) {
            await NotifyInformationAsync($"Release asset '{assetName}' not found").SkipContextSync();

            return (ReleaseAssetInfo)default;
        }

        await NotifySuccessAsync($"Release asset '{assetName}' found").SkipContextSync();

        return new ReleaseAssetInfo(
            assetName,
            asset.BrowserDownloadUrl
        );
    }

    private async Task<Result<string>> DownloadUpdateAsync(ReleaseAssetInfo info, CancellationToken cancellationToken) {
        try {
            var response = await _httpClient.GetAsync(info.DownloadUrl, cancellationToken)
                                            .SkipContextSync();

            response.EnsureSuccessStatusCode();

            var updateDirectory = _applicationContext.ApplicationDataFileProvider.GetUpdateDirectory();
            var fileName = $"{_timeProvider.GetUtcNow():yyyyMMddHHmmss}_{info.AssetName}";
            var filePath = Path.Combine(updateDirectory.Path, fileName);
            var file = _applicationContext.ApplicationDataFileProvider.GetFile(filePath);

            await using var fileStream = file.Open();
            await using var httpStream = await response.Content
                                                       .ReadAsStreamAsync(cancellationToken)
                                                       .SkipContextSync();

            await httpStream.CopyToAsync(fileStream, cancellationToken)
                            .SkipContextSync();

            httpStream.Close();
            fileStream.Close();

            return filePath;
        }
        catch (Exception ex) {
            Logger.Failure(ex);

            return Error.Failure(ex.Message);
        }
    }

    internal readonly record struct ReleaseInfo(
        bool IsAvailable,
        string Version,
        int ReleaseID
    );

    internal readonly record struct ReleaseAssetInfo(
        string AssetName,
        string DownloadUrl
    );
}