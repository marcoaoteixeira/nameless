using System.IO;
using SysDirectory = System.IO.Directory;
using SysDirectoryInfo = System.IO.DirectoryInfo;
using SysPath = System.IO.Path;
using SysSearchOption = System.IO.SearchOption;

namespace Nameless.FileStorage.FileSystem {

    public sealed class Directory : IDirectory {

        #region Private Properties

        private string Root { get; }
        private SysDirectoryInfo CurrentDirectory { get; }
        private string? FullPath => CurrentDirectory.FullName[Root.Length..];
        private Func<string, string, Action<ChangeEventArgs>, IDisposable> ChangeWatcherFactory { get; }

        #endregion

        #region Public Constructors

        public Directory(string root, string path, Func<string, string, Action<ChangeEventArgs>, IDisposable> changeWatcherFactory) {
            Ensure.NotNullEmptyOrWhiteSpace(root, nameof(root));
            Ensure.NotNull(path, nameof(path));
            Ensure.NotNull(changeWatcherFactory, nameof(changeWatcherFactory));

            Root = root;
            CurrentDirectory = new SysDirectoryInfo(PathHelper.GetPhysicalPath(root, path));
            ChangeWatcherFactory = changeWatcherFactory;
        }

        #endregion

        #region IDirectory Members

        /// <inheritdoc />
        public string Name => CurrentDirectory.Name;

        /// <inheritdoc />
        public string Path => CurrentDirectory.FullName[Root.Length..].TrimStart(SysPath.DirectorySeparatorChar);

        /// <inheritdoc />
        public bool Exists => CurrentDirectory.Exists;

        /// <inheritdoc />
        public DateTimeOffset LastWriteTimeUtc => CurrentDirectory.LastWriteTimeUtc;

        /// <inheritdoc />
        public async IAsyncEnumerable<IFile> GetFilesAsync(bool includeSubDirectories = false) {
            if (!Exists) { yield break; }

            var files = CurrentDirectory.GetFiles(
                searchPattern: "*",
                searchOption: includeSubDirectories ? SysSearchOption.AllDirectories : SysSearchOption.TopDirectoryOnly
            );

            foreach (var file in files) {
                yield return await Task.FromResult(new File(
                    root: Root,
                    path: file.FullName[Root.Length..],
                    changeWatcherFactory: ChangeWatcherFactory
                ));
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IDirectory> GetDirectoriesAsync(bool includeSubDirectories = false) {
            if (!Exists) { yield break; }

            var directories = CurrentDirectory.GetDirectories(
                searchPattern: "*",
                searchOption: includeSubDirectories ? SysSearchOption.AllDirectories : SysSearchOption.TopDirectoryOnly
            );

            foreach (var directory in directories) {
                yield return await Task.FromResult(new Directory(
                    root: Root,
                    path: directory.FullName[Root.Length..],
                    changeWatcherFactory: ChangeWatcherFactory
                ));
            }
        }

        /// <inheritdoc />
        public Task CopyAsync(string destPath, bool overwrite = false, CancellationToken token = default) {
            // TODO: Implement CopyAsync method
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task MoveAsync(string destPath, CancellationToken token = default) {
            SysDirectory.Move(sourceDirName: FullPath!, destDirName: destPath);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<bool> DeleteAsync(CancellationToken token = default) {
            if (!Exists) { return Task.FromResult(false); }

            SysDirectory.Delete(path: FullPath!, recursive: true);

            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public IDisposable Watch(Action<ChangeEventArgs> callback, string? filter = null) {
            filter = !string.IsNullOrWhiteSpace(filter) ? filter : "*.*";
            filter = PathHelper.Normalize(SysPath.Combine(Path, filter));

            return ChangeWatcherFactory(Path, filter, callback);
        }

        #endregion
    }
}