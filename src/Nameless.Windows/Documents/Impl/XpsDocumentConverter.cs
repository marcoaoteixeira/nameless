using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Nameless.IO;
using Nameless.Windows.Office;

namespace Nameless.Windows.Documents;

public class XpsDocumentConverter : IDocumentConverter {
    private readonly IFileProvider _fileSystemProvider;
    private readonly IWordApplication _wordApplication;
    private readonly ILogger<XpsDocumentConverter> _logger;

    public DocumentType OutputType => DocumentType.XPS;

    public XpsDocumentConverter(IFileProvider fileSystemProvider, IWordApplication wordApplication, ILogger<XpsDocumentConverter> logger) {
        _fileSystemProvider = fileSystemProvider;
        _wordApplication = wordApplication;
        _logger = logger;
    }

    public bool CanConvert(DocumentType type) {
        return type switch {
            DocumentType.DOCX => true,
            DocumentType.RTF => true,
            DocumentType.TXT => true,
            _ => false
        };
    }

    public Task<string> ConvertAsync(string filePath, CancellationToken cancellationToken) {
        EnsureTemporaryDirectoryExistence();

        var relativeXpsFilePath = Path.Combine(
            FolderStructure.TemporaryDirectoryName,
            string.Concat(GetTemporaryFileName(filePath), ".xps")
        );
        var xpsFile = _fileSystemProvider.GetFile(relativeXpsFilePath);

        if (xpsFile.Exists) {
            return Task.FromResult(xpsFile.Path);
        }

        try {
            using var document = _wordApplication.Open(filePath);

            document.SaveAs(xpsFile.Path, DocumentType.XPS);
        }
        catch (Exception ex) {
            CommonLog.Error(_logger, ex.Message, ex, tag: GetType().Tag);

            return Task.FromResult(string.Empty);
        }

        return Task.FromResult(xpsFile.Path);
    }

    private static string GetTemporaryFileName(string filePath) {
        filePath = PathHelper.Normalize(filePath);

        Span<byte> buffer = stackalloc byte[Encoding.UTF8.GetByteCount(filePath)];
        Encoding.UTF8.GetBytes(filePath, buffer);

        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(buffer, hash);

        return Convert.ToHexStringLower(hash);
    }

    private void EnsureTemporaryDirectoryExistence() {
        _fileSystemProvider.GetDirectory(FolderStructure.TemporaryDirectoryName)
                           .Create();
    }
}