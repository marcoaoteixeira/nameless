using System.IO;
using Microsoft.Extensions.Logging;
using Nameless.Windows.Office;

namespace Nameless.Windows.Documents;

public class WordDocumentReader : IDocumentReader {
    private readonly IWordApplication _wordApplication;
    private readonly ILogger<WordDocumentReader> _logger;

    public WordDocumentReader(IWordApplication wordApplication, ILogger<WordDocumentReader> logger) {
        _wordApplication = wordApplication;
        _logger = logger;
    }
    public bool CanRead(string filePath) {
        return string.Equals(Path.GetExtension(filePath), WellKnownDocuments.Doc, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Path.GetExtension(filePath), WellKnownDocuments.Docx, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Path.GetExtension(filePath), WellKnownDocuments.Rtf, StringComparison.OrdinalIgnoreCase);
    }

    public Task<string> GetContentAsync(string filePath, CancellationToken cancellationToken) {
        try {
            using var document = _wordApplication.Open(filePath);

            var result = document.GetContent(formatted: false);

            return Task.FromResult(result);
        }
        catch (Exception ex) { CommonLog.Error(_logger, ex.Message, ex, tag: GetType().Tag); }

        return Task.FromResult(string.Empty);
    }
}