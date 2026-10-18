using Nameless.Registration;

namespace Nameless.Windows.Documents;

public class DocumentServicesRegistration : AssemblyScanAware<DocumentServicesRegistration> {
    private static readonly Type DocumentConverterType = typeof(IDocumentConverter);
    private static readonly Type DocumentReaderType = typeof(IDocumentReader);

    private readonly HashSet<Type> _documentConverters = [];
    private readonly HashSet<Type> _documentReaders = [];

    public IReadOnlyCollection<Type> DocumentConverters => UseAssemblyScan
        ? ExecuteAssemblyScan(DocumentConverterType)
        : _documentConverters;

    public IReadOnlyCollection<Type> DocumentReaders => UseAssemblyScan
        ? ExecuteAssemblyScan(DocumentReaderType)
        : _documentReaders;

    public DocumentServicesRegistration RegisterDocumentConverter<TDocumentConverter>() where TDocumentConverter : IDocumentConverter {
        return RegisterDocumentConverter(typeof(TDocumentConverter));
    }

    public DocumentServicesRegistration RegisterDocumentConverter(Type type) {
        Throws.When.IsNonConcreteType(type);
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNotAssignableFrom(type, lhs: DocumentConverterType);

        _documentConverters.Add(type);

        return this;
    }

    public DocumentServicesRegistration RegisterDocumentReader<TDocumentReader>() where TDocumentReader : IDocumentReader {
        return RegisterDocumentReader(typeof(TDocumentReader));
    }
    
    public DocumentServicesRegistration RegisterDocumentReader(Type type) {
        Throws.When.IsNonConcreteType(type);
        Throws.When.IsOpenGenericType(type);
        Throws.When.IsNotAssignableFrom(type, lhs: DocumentReaderType);

        _documentReaders.Add(type);

        return this;
    }
}