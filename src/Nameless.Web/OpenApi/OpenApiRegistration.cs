using Microsoft.AspNetCore.OpenApi;

namespace Nameless.Web.OpenApi;

/// <summary>
///     Provides configuration settings for registering multiple
///     OpenAPI documents, each with its own options.
/// </summary>
public class OpenApiRegistration {
    private readonly Dictionary<string, Action<OpenApiOptions>> _documentOptions = [];

    /// <summary>
    ///     Gets a read-only dictionary that maps document names to actions
    ///     for configuring OpenAPI options for each document.
    /// </summary>
    public IReadOnlyDictionary<string, Action<OpenApiOptions>> DocumentOptions => _documentOptions;

    /// <summary>
    ///     Registers an OpenAPI document with the specified name and
    ///     configuration options.
    /// </summary>
    /// <param name="document">
    ///     The name of the OpenAPI document to register. Cannot be null.
    /// </param>
    /// <param name="options">
    ///     An action that configures the OpenAPI options for the registered
    ///     document. Cannot be null.
    /// </param>
    /// <returns>
    ///     The current instance of <see cref="OpenApiRegistration"/>,
    ///     so other actions can be chained.
    /// </returns>
    public OpenApiRegistration RegisterOpenApiDocument(string document, Action<OpenApiOptions> options) {
        Throws.When.NullOrWhiteSpace(document);

        _documentOptions[document] = options;

        return this;
    }
}