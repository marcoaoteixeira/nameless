using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nameless.Application;
using Nameless.Mediator;
using Nameless.WinApp.Views.Pages;
using Nameless.Windows;
using Nameless.Windows.BackupRecovery;
using Nameless.Windows.Configuration;
using Nameless.Windows.Dialogs.FileSystem;
using Nameless.Windows.Helpers;
using Nameless.Windows.Localization;
using Nameless.Windows.Mvvm;
using Nameless.Windows.TaskRunner;
using Nameless.Windows.UI;
using Nameless.Windows.UseCases;
using Nameless.Windows.UseCases.Backup;
using Wpf.Ui.Abstractions.Controls;
using Wpf.Ui.Appearance;

namespace Nameless.WinApp.ViewModels.Pages;

/// <summary>
///     View model for <see cref="AppConfigurationPage"/>.
/// </summary>
public partial class AppConfigurationPageViewModel : ViewModel, INavigationAware {
    private readonly IAppConfigurationManager _appConfigurationManager;
    private readonly IApplicationContext _applicationContext;
    private readonly IFileSystemDialog _fileSystemDialog;
    private readonly IMediator _mediator;
    private readonly ITaskRunner _taskRunner;

    private bool _initialized;

    private ILocalizer T { get; }

    [ObservableProperty]
    public partial ComboBoxItem CurrentTheme { get; set; } = ComboBoxItemHelper.EmptyComboBoxItem;

    [ObservableProperty]
    public partial ComboBoxItem CurrentLanguage { get; set; } = ComboBoxItemHelper.EmptyComboBoxItem;

    [ObservableProperty]
    public partial bool CurrentConfirmBeforeExit { get; set; }

    public string AppVersion { get; private set; } = string.Empty;

    public ComboBoxItem[] AvailableThemes { get; } = [
        Theme.Light.ToComboBoxItem(),
        Theme.Dark.ToComboBoxItem(),
        Theme.HighContrast.ToComboBoxItem()
    ];

    public ComboBoxItem[] AvailableLanguages { get; } = [
        ComboBoxItemHelper.Create("pt-BR", "Português (Brasil)"),
        ComboBoxItemHelper.Create("en-US", "English (United States)"),
    ];

    public AppConfigurationPageViewModel(
        IAppConfigurationManager appConfigurationManager,
        IApplicationContext applicationContext,
        IFileSystemDialog fileSystemDialog,
        ILocalizer localizer,
        IMediator mediator,
        ITaskRunner taskRunner) {
        _appConfigurationManager = appConfigurationManager;
        _applicationContext = applicationContext;
        _fileSystemDialog = fileSystemDialog;
        _mediator = mediator;
        _taskRunner = taskRunner;

        T = localizer;
    }

    public Task OnNavigatedToAsync() {
        Initialize();

        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync() {
        return Task.CompletedTask;
    }

    private void Initialize() {
        if (_initialized) { return; }

        AppVersion = _applicationContext.Version;

        var theme = _appConfigurationManager.Theme;

        CurrentTheme = theme.GetComboBoxItem(AvailableThemes);
        CurrentConfirmBeforeExit = _appConfigurationManager.ConfirmBeforeExit;

        _initialized = true;
    }

    [RelayCommand]
    private Task PerformSystemUpdateAsync() {
        return _taskRunner.CreateBuilder()
                          .SetName(T["AppConfigurationPageViewModel_PerformSystemUpdate_TaskRunnerWindow_Title"])
                          .SubscribeFor<UseCaseMessage>()
                          .SetDelegate(ExecuteSystemUpdateAsync)
                          .RunAsync();
    }

    [RelayCommand]
    private Task OpenApplicationDataDirectoryAsync() {
        ProcessHelper.OpenDirectory(_applicationContext.ApplicationDataFileProvider.Root);

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task OpenApplicationLogFileAsync() {
        ProcessHelper.OpenTextFile(Constants.Application.LogFileName);

        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task PerformApplicationBackupAsync() {
        return _taskRunner.CreateBuilder()
                          .SetName(T["AppConfigurationPageViewModel_PerformApplicationBackup_TaskRunnerWindow_Title"])
                          .SubscribeFor<UseCaseMessage>()
                          .SubscribeFor<DisasterRecoveryRoutineMessage>()
                          .SetDelegate(ExecuteApplicationBackupAsync)
                          .RunAsync();
    }

    [RelayCommand]
    private Task PerformApplicationRestoreAsync() {
        var files = _fileSystemDialog.OpenFile(opts => {
            opts.Title = "Selecionar arquivo de backup...";
            opts.Filter = $"InfoPhoenix Application Backup File|*{BackupFileExtension}";
        });

        return Task.CompletedTask;
    }

    partial void OnCurrentThemeChanged(ComboBoxItem? oldValue, ComboBoxItem newValue) {
        if (!_initialized || oldValue?.Tag == newValue.Tag) { return; }

        var theme = (Theme)newValue.Tag;

        ApplicationThemeManager.Apply(theme.ToApplicationTheme());

        _appConfigurationManager.Theme = theme;
    }

    partial void OnCurrentLanguageChanged(ComboBoxItem? oldValue, ComboBoxItem newValue) {
        if (!_initialized || oldValue?.Tag == newValue.Tag) { return; }

        var isoCode = (string)newValue.Tag;

        T.SetCulture(isoCode);

        _appConfigurationManager.Language = isoCode;
    }

    partial void OnCurrentConfirmBeforeExitChanged(bool oldValue, bool newValue) {
        if (!_initialized || oldValue == newValue) { return; }

        _appConfigurationManager.ConfirmBeforeExit = newValue;
    }

    private async Task ExecuteApplicationBackupAsync(CancellationToken cancellationToken) {
        _ = await _mediator.ExecuteAsync(
            new PerformApplicationBackupRequest(),
            cancellationToken
        ).SkipContextSync();
    }

    private Task ExecuteSystemUpdateAsync(CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }
}
