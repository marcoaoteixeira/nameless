using System.Windows;
using Microsoft.Win32;
using Nameless.Application;
using Nameless.Windows.Localization;

namespace Nameless.Windows.Dialogs.FileSystem;

public class FileSystemDialog : IFileSystemDialog {
    private const string CLASS = nameof(FileSystemDialog);

    private readonly IApplicationContext _applicationContext;
    private ILocalizer T { get; }

    public FileSystemDialog(IApplicationContext applicationContext, ILocalizer localizer) {
        _applicationContext = applicationContext;

        T = localizer;
    }

    public IEnumerable<string> OpenDirectory(Action<DirectorySelectionOptions> configure) {
        const string ActionName = nameof(OpenDirectory);

        var options = new DirectorySelectionOptions();

        configure(options);

        var dialog = new OpenFolderDialog {
            DefaultDirectory = options.Root ?? _applicationContext.ApplicationDataFileProvider.Root,
            Title = options.Title ?? GetFallbackTitle(),
            Multiselect = options.Multiselect,
            ValidateNames = true
        };

        OpenDialog(dialog, options);

        return dialog.FolderNames;

        string GetFallbackTitle() {
            return options.Multiselect
                ? T[$"{CLASS}_{ActionName}_GetFallbackTitlePlural"]
                : T[$"{CLASS}_{ActionName}_GetFallbackTitle"];
        }
    }

    public IEnumerable<string> OpenFile(Action<FileSelectionOptions> configure) {
        const string ActionName = nameof(OpenFile);

        var options = new FileSelectionOptions();

        configure(options);

        var dialog = new OpenFileDialog {
            DefaultDirectory = options.Root ?? _applicationContext.ApplicationDataFileProvider.Root,
            Title = options.Title ?? GetFallbackTitle(),
            Multiselect = options.Multiselect,
            Filter = options.Filter,
            ValidateNames = true
        };

        OpenDialog(dialog, options);

        return dialog.FileNames;

        string GetFallbackTitle() {
            return options.Multiselect
                ? T[$"{CLASS}_{ActionName}_GetFallbackTitlePlural"]
                : T[$"{CLASS}_{ActionName}_GetFallbackTitle"];
        }
    }

    public string OpenSave(Action<SaveSelectionOptions> configure) {
        const string ActionName = nameof(OpenSave);

        var options = new SaveSelectionOptions();

        configure(options);

        var dialog = new SaveFileDialog {
            DefaultDirectory = options.Root ?? _applicationContext.ApplicationDataFileProvider.Root,
            Title = options.Title ?? T[$"{CLASS}_{ActionName}_GetFallbackTitle"],
            CheckFileExists = options.EnsureFileExistence,
            Filter = options.Filter,
            OverwritePrompt = options.OverwriteWarning,
            ValidateNames = true
        };

        OpenDialog(dialog, options);

        return dialog.FileName;
    }

    private static void OpenDialog(CommonDialog dialog, FileSystemDialogOptions options) {
        var owner = GetOwner(options);

        if (owner is not null) { dialog.ShowDialog(owner); }
        else { dialog.ShowDialog(); }

        return;

        static Window? GetOwner(FileSystemDialogOptions options) {
            if (options.Owner is not null) {
                return options.Owner;
            }

            try { return WindowsApplication.Current.MainWindow; }
            catch { /* swallow */ }

            return null;
        }
    }
}