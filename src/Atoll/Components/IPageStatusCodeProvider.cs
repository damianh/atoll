namespace Atoll.Components;

/// <summary>
/// Optional interface implemented by page components that need to signal a non-200
/// HTTP status code to the request handler.
/// </summary>
/// <remarks>
/// <para>
/// The Atoll request handlers (<c>AtollRequestHandler</c> and <c>DevAtollRequestHandler</c>)
/// check for this interface after a page component has rendered. If the component
/// implements <see cref="IPageStatusCodeProvider"/>, the handler reads
/// <see cref="ResponseStatusCode"/> and writes it to the HTTP response instead of
/// the default 200.
/// </para>
/// <para>
/// Example: A docs page that cannot find the requested slug should set
/// <see cref="ResponseStatusCode"/> to 404 so the browser and intermediaries
/// receive the correct status.
/// </para>
/// </remarks>
public interface IPageStatusCodeProvider
{
    /// <summary>
    /// Gets the HTTP status code that should be written to the response after the page renders.
    /// </summary>
    int ResponseStatusCode { get; }
}
