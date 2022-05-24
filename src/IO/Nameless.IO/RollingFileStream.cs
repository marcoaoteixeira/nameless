using System;
using System.IO;

namespace Nameless.IO {

    /// <summary>
    /// Provides a rolling <see cref="Stream"/> for a file.
    /// </summary>
    public sealed class RollingFileStream : FileStream {

        #region Private Constants

        private const int DEFAULT_BUFFER_SIZE = 32 * 1024; // 32Kb

        #endregion

        #region Public Static Read-Only Fields

        public static readonly string MaximunFileLengthShouldBeGreaterThanZero = "Maximun file length should be greater than zero.";
        public static readonly string MaximunFileCountShouldBeGreaterThanZero = "Maximun file count should be greater than zero.";
        public static readonly string BufferSizeExceedsMaximunFileLength = "Buffer size exceeds maximun file length.";

        #endregion

        #region Private Read-Only Fields

        private readonly string? _fileDirectory;
        private readonly string _fileNameBase;
        private readonly string _fileExtension;

        #endregion

        #region Private Fields

        private int _nextFileIndex;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the maximun file length.
        /// </summary>
        public long MaximunFileLength { get; }

        /// <summary>
        /// Gets the maximun file count.
        /// </summary>
        public int MaximunFileCount { get; }

        /// <summary>
        /// Gets or sets the possibility to split data.
        /// </summary>
        public bool CanSplitData { get; set; }

        #endregion

        #region Public Override Properties

        /// <inheritdoc />
        public override bool CanRead {
            get { return false; }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="RollingFileStream"/>.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="maximunFileLength">Maximun size of the file, in bytes.</param>
        /// <param name="maximunFileCount">Maximun number of files in directory.</param>
        /// <param name="mode">See <see cref="FileMode"/>.</param>
        public RollingFileStream(string path, long maximunFileLength, int maximunFileCount, FileMode mode)
            : this(path, maximunFileLength, maximunFileCount, mode, FileShare.Read, DEFAULT_BUFFER_SIZE, false) { }

        /// <summary>
        /// Initializes a new instance of <see cref="RollingFileStream"/>.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="maximunFileLength">Maximun size of the file, in bytes.</param>
        /// <param name="maximunFileCount">Maximun number of files in directory.</param>
        /// <param name="mode">See <see cref="FileMode"/>.</param>
        /// <param name="share"></param>
        public RollingFileStream(string path, long maximunFileLength, int maximunFileCount, FileMode mode, FileShare share)
            : this(path, maximunFileLength, maximunFileCount, mode, share, DEFAULT_BUFFER_SIZE, false) { }

        /// <summary>
        /// Initializes a new instance of <see cref="RollingFileStream"/>.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="maximunFileLength">Maximun size of the file, in bytes.</param>
        /// <param name="maximunFileCount">Maximun number of files in directory.</param>
        /// <param name="mode">See <see cref="FileMode"/>.</param>
        /// <param name="share"></param>
        /// <param name="bufferSize"></param>
        public RollingFileStream(string path, long maximunFileLength, int maximunFileCount, FileMode mode, FileShare share, int bufferSize)
            : this(path, maximunFileLength, maximunFileCount, mode, share, bufferSize, false) { }

        /// <summary>
        /// Initializes a new instance of <see cref="RollingFileStream"/>.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="maximunFileLength">Maximun size of the file, in bytes.</param>
        /// <param name="maximunFileCount">Maximun number of files in directory.</param>
        /// <param name="mode">See <see cref="FileMode"/>.</param>
        /// <param name="share"></param>
        /// <param name="bufferSize"></param>
        /// <param name="useAsync"></param>
        public RollingFileStream(string path, long maximunFileLength, int maximunFileCount, FileMode mode, FileShare share, int bufferSize, bool useAsync)
            : base(path, FilterFileMode(mode), FileAccess.Write, share, bufferSize, useAsync) {
            if (maximunFileLength <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maximunFileLength), maximunFileLength, MaximunFileLengthShouldBeGreaterThanZero);
            }

            if (maximunFileCount <= 0) {
                throw new ArgumentOutOfRangeException(nameof(maximunFileCount), maximunFileCount, MaximunFileCountShouldBeGreaterThanZero);
            }

            MaximunFileLength = maximunFileLength;
            MaximunFileCount = maximunFileCount;

            CanSplitData = true;

            var fullPath = Path.GetFullPath(path);
            _fileDirectory = Path.GetDirectoryName(fullPath);
            _fileNameBase = Path.GetFileNameWithoutExtension(fullPath);
            _fileExtension = Path.GetExtension(fullPath);

            switch (mode) {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.Truncate:
                    // Delete old files
                    for (var fileCount = 0; fileCount < MaximunFileCount; ++fileCount) {
                        var file = GetRollingFileName(fileCount);
                        if (File.Exists(file)) {
                            File.Delete(file);
                        }
                    }
                    break;

                default:
                    // Position file pointer to the last backup file
                    for (var fileCount = 0; fileCount < MaximunFileCount; ++fileCount) {
                        if (File.Exists(GetRollingFileName(fileCount))) {
                            _nextFileIndex = fileCount + 1;
                        }
                    }

                    if (_nextFileIndex == MaximunFileCount) {
                        _nextFileIndex = 0;
                    }

                    Seek(0, SeekOrigin.End);
                    break;
            }
        }

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override void Write(byte[] array, int offset, int count) {
            var actualCount = Math.Min(count, array.GetLength(0));

            if (Position + actualCount <= MaximunFileLength) {
                base.Write(array, offset, count);
                return;
            }

            if (CanSplitData) {
                var partialCount = (int)(Math.Max(MaximunFileLength, Position) - Position);

                base.Write(array, offset, partialCount);

                offset += partialCount;
                count = actualCount - partialCount;
            } else {
                if (count > MaximunFileLength) {
                    throw new ArgumentOutOfRangeException(nameof(count), count, BufferSizeExceedsMaximunFileLength);
                }
            }

            RollingStream();
            Write(array, offset, count);
        }

        #endregion

        #region Private Static Methods

        private static FileMode FilterFileMode(FileMode mode) => mode == FileMode.Append ? FileMode.OpenOrCreate : mode;

        #endregion

        #region Private Methods

        private string GetRollingFileName(int index) {
            var format = string.Concat("D", MaximunFileCount.ToString().Length);
            var fileName = string.Concat(_fileNameBase
                , index.ToString(format)
                , _fileExtension.Length > 0 ? _fileExtension : string.Empty);

            return Path.Combine(_fileDirectory ?? string.Empty, fileName);
        }

        private void RollingStream() {
            Flush();
            File.Copy(Name, GetRollingFileName(_nextFileIndex), true);
            SetLength(0);

            ++_nextFileIndex;
            if (_nextFileIndex >= MaximunFileCount) {
                _nextFileIndex = 0;
            }
        }

        #endregion
    }
}