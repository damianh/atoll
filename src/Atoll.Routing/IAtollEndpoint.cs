namespace Atoll.Routing;

/// <summary>
/// Defines the contract for an Atoll API endpoint — a class that handles HTTP
/// requests at a specific route and returns <see cref="AtollResponse"/> objects.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IAtollEndpoint"/> is the API counterpart of <see cref="IAtollPage"/>.
/// While pages render HTML content, endpoints return structured HTTP responses
/// (JSON, plain text, redirects, etc.) suitable for API routes.
/// </para>
/// <para>
/// Endpoints are discovered by <see cref="FileSystem.RouteDiscovery"/> from the
/// <c>src/pages/</c> directory alongside page components. For example, a file
/// at <c>src/pages/api/posts.cs</c> containing a class implementing
/// <see cref="IAtollEndpoint"/> will be routed to <c>/api/posts</c>.
/// </para>
/// <para>
/// Each HTTP method is handled by a separate method on the endpoint. Only
/// methods that are implemented will respond to that HTTP verb; unimplemented
/// methods will result in a <c>405 Method Not Allowed</c> response.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public sealed class PostsEndpoint : IAtollEndpoint
/// {
///     public Task&lt;AtollResponse&gt; GetAsync(EndpointContext context)
///     {
///         var posts = new[] { new { Id = 1, Title = "Hello" } };
///         return Task.FromResult(AtollResponse.Json(posts));
///     }
///
///     public Task&lt;AtollResponse&gt; PostAsync(EndpointContext context)
///     {
///         // Read body, create post, return 201
///         return Task.FromResult(AtollResponse.Json(
///             new { Id = 2, Title = "New Post" }, 201));
///     }
/// }
/// </code>
/// </example>
public interface IAtollEndpoint
{
    /// <summary>
    /// Handles an HTTP GET request.
    /// </summary>
    /// <param name="context">The endpoint context.</param>
    /// <returns>
    /// A task that resolves to an <see cref="AtollResponse"/>, or <c>null</c>
    /// if this method is not supported (results in 405).
    /// </returns>
    Task<AtollResponse>? GetAsync(EndpointContext context) => null;

    /// <summary>
    /// Handles an HTTP POST request.
    /// </summary>
    /// <param name="context">The endpoint context.</param>
    /// <returns>
    /// A task that resolves to an <see cref="AtollResponse"/>, or <c>null</c>
    /// if this method is not supported (results in 405).
    /// </returns>
    Task<AtollResponse>? PostAsync(EndpointContext context) => null;

    /// <summary>
    /// Handles an HTTP PUT request.
    /// </summary>
    /// <param name="context">The endpoint context.</param>
    /// <returns>
    /// A task that resolves to an <see cref="AtollResponse"/>, or <c>null</c>
    /// if this method is not supported (results in 405).
    /// </returns>
    Task<AtollResponse>? PutAsync(EndpointContext context) => null;

    /// <summary>
    /// Handles an HTTP DELETE request.
    /// </summary>
    /// <param name="context">The endpoint context.</param>
    /// <returns>
    /// A task that resolves to an <see cref="AtollResponse"/>, or <c>null</c>
    /// if this method is not supported (results in 405).
    /// </returns>
    Task<AtollResponse>? DeleteAsync(EndpointContext context) => null;

    /// <summary>
    /// Handles an HTTP PATCH request.
    /// </summary>
    /// <param name="context">The endpoint context.</param>
    /// <returns>
    /// A task that resolves to an <see cref="AtollResponse"/>, or <c>null</c>
    /// if this method is not supported (results in 405).
    /// </returns>
    Task<AtollResponse>? PatchAsync(EndpointContext context) => null;

    /// <summary>
    /// Handles an HTTP HEAD request.
    /// </summary>
    /// <param name="context">The endpoint context.</param>
    /// <returns>
    /// A task that resolves to an <see cref="AtollResponse"/>, or <c>null</c>
    /// if this method is not supported (results in 405).
    /// </returns>
    Task<AtollResponse>? HeadAsync(EndpointContext context) => null;

    /// <summary>
    /// Handles an HTTP OPTIONS request.
    /// </summary>
    /// <param name="context">The endpoint context.</param>
    /// <returns>
    /// A task that resolves to an <see cref="AtollResponse"/>, or <c>null</c>
    /// if this method is not supported (results in 405).
    /// </returns>
    Task<AtollResponse>? OptionsAsync(EndpointContext context) => null;
}
