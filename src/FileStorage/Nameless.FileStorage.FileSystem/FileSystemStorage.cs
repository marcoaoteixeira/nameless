using System;
using System.Threading;
using System.Threading.Tasks;
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

namespace Nameless.FileStorage.FileSystem {

    public sealed class FileSystemStorage : IFileStorage {

        #region Private Read-Only Fields

        private readonly FileSystemStorageOptions _opts;

        #endregion

        #region Public Constructors

        public FileSystemStorage(FileSystemStorageOptions? opts = null) {
            _opts = opts ?? new();

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
            var obj = sender as SysFileSystemInfo;

            if (obj == null) { return; }

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
        public Task<bool> CreateDirectoryAsync(string relativePath, CancellationToken token = default) {
            relativePath = PathHelper.Normalize(relativePath);

            var directoryPath = PathHelper.GetPhysicalPath(Root, relativePath);
            if (SysDirectory.Exists(directoryPath)) {
                return Task.FromResult(false);
            }

            SysDirectory.CreateDirectory(directoryPath);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        /// <exception cref="FileStorageException">
        /// Thrown if the specified path does not points to a directory.
        /// </exception>
        public Task<IDirectory> GetDirectoryAsync(string relativePath) {
            relativePath = PathHelper.Normalize(relativePath);

            var directoryPath = PathHelper.GetPhysicalPath(Root, relativePath);
            if (!SysDirectory.Exists(directoryPath)) {
                throw new FileStorageException("The specified path does not points to a directory.");
            }

            IDirectory directory = new Directory(Root, relativePath, ChangeWatcherFactory);
            return Task.FromResult(directory);
        }

        /// <inheritdoc />
        public Task CreateFileAsync(string relativePath, SysStream input, bool overwrite = false, CancellationToken token = default) {
            Ensure.NotNull(input, nameof(input));

            relativePath = PathHelper.Normalize(relativePath);

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
            relativePath = PathHelper.Normalize(relativePath);

            var file = new File(Root, relativePath, ChangeWatcherFactory);

            return Task.FromResult<IFile>(file);
        }

        #endregion
    }
}