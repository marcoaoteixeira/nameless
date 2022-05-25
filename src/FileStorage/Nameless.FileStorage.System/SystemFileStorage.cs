using SysDirectory = System.IO.Directory;
using SysFile = System.IO.File;
using SysFileSystemEventArgs = System.IO.FileSystemEventArgs;
using SysFileSystemInfo = System.IO.FileSystemInfo;
using SysFileSystemWatcher = System.IO.FileSystemWatcher;
using SysNotifyFilters = System.IO.NotifyFilters;
using SysPath = System.IO.Path;
using SysRenamedEventArgs = System.IO.RenamedEventArgs;
using SysStream = System.IO.Stream;
using SysWatcherChangeTypes = System.IO.WatcherChangeTypes;

namespace Nameless.FileStorage.System {

    public sealed class SystemFileStorage : IFileStorage {

        #region Private Read-Only Fields

        private readonly FileStorageOptions _opts;

        #endregion

        #region Public Constructors

        public SystemFileStorage(FileStorageOptions? opts = null) {
            _opts = opts ?? new FileStorageOptions {
                Root = typeof(SystemFileStorage).Assembly.GetDirectoryPath()
            };

            Root = PathHelper.Normalize(_opts.Root);
        }

        #endregion

        #region Private Static Methods

        private static ChangeReason ParseChangeTypes(SysWatcherChangeTypes types) {
            switch (types) {
                case SysWatcherChangeTypes.All:
                    break;
                case SysWatcherChangeTypes.Changed:
                    return ChangeReason.Changed;
                case SysWatcherChangeTypes.Created:
                    return ChangeReason.Created;
                case SysWatcherChangeTypes.Deleted:
                    return ChangeReason.Deleted;
                case SysWatcherChangeTypes.Renamed:
                    return ChangeReason.Renamed;
            }
            return ChangeReason.None;
        }

        #endregion

        #region Private Methods

        private IDisposable ChangeWatcherFactory(string path, string filter, Action<ChangeEventArgs> callback) {
            var watcher = new SysFileSystemWatcher {
                Filter = filter ?? "*.*",
                Path = path,
                NotifyFilter = SysNotifyFilters.LastAccess |
                               SysNotifyFilters.LastWrite |
                               SysNotifyFilters.FileName |
                               SysNotifyFilters.DirectoryName
            };

            watcher.Changed += (sender, evt) => FileSystemWatcherCallback(sender, evt, callback);
            watcher.Deleted += (sender, evt) => FileSystemWatcherCallback(sender, evt, callback);
            watcher.Renamed += (sender, evt) => FileSystemWatcherCallback(sender, evt, callback);

            watcher.EnableRaisingEvents = true;

            return watcher;
        }

        private void FileSystemWatcherCallback(object sender, SysFileSystemEventArgs args, Action<ChangeEventArgs> callback) {
            if (sender is not SysFileSystemInfo obj) { return; }

            var originalPath = obj.FullName;
            var currentPath = args is SysRenamedEventArgs renamedArgs ? renamedArgs.OldFullPath : args.FullPath;

            var newArgs = new ChangeEventArgs {
                OriginalPath = originalPath[Root.Length..].TrimStart(SysPath.DirectorySeparatorChar),
                CurrentPath = currentPath[Root.Length..].TrimStart(SysPath.DirectorySeparatorChar),
                Reason = ParseChangeTypes(args.ChangeType)
            };

            callback(newArgs);
        }

        #endregion

        #region IFileStorage Members

        /// <inheritdoc />
        public string Root { get; }

        /// <inheritdoc />
        public Task CreateFileAsync(string relativePath, SysStream input, bool overwrite = false, CancellationToken token = default) {
            Ensure.NotNullEmptyOrWhiteSpace(relativePath, nameof(relativePath));
            Ensure.NotNull(input, nameof(input));

            var filePath = PathHelper.GetPhysicalPath(Root, relativePath);

            if (SysFile.Exists(filePath) && !overwrite) {
                throw new FileStorageException("Cannot create file because the destination path already exists.");
            }

            // Create directory path if it doesn't exist.
            var directoryPath = SysPath.GetDirectoryName(filePath);
            SysDirectory.CreateDirectory(directoryPath!);

            using var output = SysFile.Create(filePath);
            return input.CopyToAsync(output, token);
        }

        /// <inheritdoc />
        public Task<IFile> GetFileAsync(string relativePath) {
            Ensure.NotNullEmptyOrWhiteSpace(relativePath, nameof(relativePath));

            var currentRelativePath = PathHelper.Normalize(relativePath);

            var file = new File(Root, currentRelativePath, ChangeWatcherFactory);

            return Task.FromResult<IFile>(file);
        }

        public IAsyncEnumerable<IFile> GetFilesAsync(string? filter = null) {
            var files = filter != null
                ? SysDirectory.GetFiles(Root, filter)
                : SysDirectory.GetFiles(Root);

            var result = files.Select(_ => new File(Root, _, ChangeWatcherFactory));

            return result.AsAsyncEnumerable();
        }

        #endregion
    }
}