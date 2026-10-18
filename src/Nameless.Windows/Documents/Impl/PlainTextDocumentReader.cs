using System.IO;
using Microsoft.Extensions.Logging;

namespace Nameless.Windows.Documents;

public class PlainTextDocumentReader : IDocumentReader {
    private readonly ILogger<PlainTextDocumentReader> _logger;

    public PlainTextDocumentReader(ILogger<PlainTextDocumentReader> logger) {
        _logger = logger;
    }

    public bool CanRead(string filePath) {
        return string.Equals(Path.GetExtension(filePath), WellKnownDocuments.Txt, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string> GetContentAsync(string filePath, CancellationToken cancellationToken) {
        Throws.When.NullOrWhiteSpace(filePath);

        try {
            if (File.Exists(filePath)) {
                return await File.ReadAllTextAsync(filePath, cancellationToken)
                                 .ConfigureAwait(continueOnCapturedContext: false);

            }
        }
        catch (Exception ex) { CommonLog.Error(_logger, ex.Message, ex, tag: GetType().Tag); }

        return string.Empty;
    }
}