using System.Reflection;

namespace Atoll.Routing;

/// <summary>
/// Dispatches an HTTP request to the appropriate handler method on an
/// <see cref="IAtollEndpoint"/> based on the request's HTTP method.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EndpointDispatcher"/> is responsible for resolving which method
/// on an <see cref="IAtollEndpoint"/> should handle a given request. If the
/// endpoint does not implement the requested HTTP method (i.e., the default
/// interface method returns <c>null</c>), the dispatcher returns a
/// <c>405 Method Not Allowed</c> response with an <c>Allow</c> header
/// listing the supported methods.
/// </para>
/// </remarks>
public static class EndpointDispatcher
{
    private static readonly string[] HttpMethodNames =
        ["GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS"];

    private static readonly string[] InterfaceMethodNames =
        ["GetAsync", "PostAsync", "PutAsync", "DeleteAsync", "PatchAsync", "HeadAsync", "OptionsAsync"];

    /// <summary>
    /// Dispatches a request to the appropriate method on the given endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to dispatch to.</param>
    /// <param name="context">The endpoint context containing request information.</param>
    /// <returns>
    /// A task that resolves to the <see cref="AtollResponse"/> from the endpoint handler,
    /// or a <c>405 Method Not Allowed</c> response if the method is not supported.
    /// </returns>
    public static async Task<AtollResponse> DispatchAsync(
        IAtollEndpoint endpoint,
        EndpointContext context)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(context);

        var handlerTask = context.Request.Method switch
        {
            "GET" => endpoint.GetAsync(context),
            "POST" => endpoint.PostAsync(context),
            "PUT" => endpoint.PutAsync(context),
            "DELETE" => endpoint.DeleteAsync(context),
            "PATCH" => endpoint.PatchAsync(context),
            "HEAD" => endpoint.HeadAsync(context),
            "OPTIONS" => endpoint.OptionsAsync(context),
            _ => null
        };

        if (handlerTask is not null)
        {
            return await handlerTask;
        }

        // Method not supported — return 405 with Allow header
        var allowedMethods = GetSupportedMethods(endpoint.GetType());
        return AtollResponse.MethodNotAllowed(allowedMethods);
    }

    /// <summary>
    /// Gets the list of HTTP methods supported by the given endpoint type
    /// by inspecting which interface methods have been explicitly implemented.
    /// </summary>
    /// <param name="endpointType">The endpoint type to inspect.</param>
    /// <returns>A read-only list of supported HTTP method names.</returns>
    public static IReadOnlyList<string> GetSupportedMethods(Type endpointType)
    {
        ArgumentNullException.ThrowIfNull(endpointType);

        var methods = new List<string>();
        var interfaceMap = endpointType.GetInterfaceMap(typeof(IAtollEndpoint));

        for (var i = 0; i < interfaceMap.InterfaceMethods.Length; i++)
        {
            var interfaceMethod = interfaceMap.InterfaceMethods[i];
            var targetMethod = interfaceMap.TargetMethods[i];

            // A method is "supported" if the target method is declared on the
            // concrete type (not on the interface itself via default implementation).
            if (targetMethod.DeclaringType != typeof(IAtollEndpoint))
            {
                var methodIndex = Array.IndexOf(InterfaceMethodNames, interfaceMethod.Name);
                if (methodIndex >= 0)
                {
                    methods.Add(HttpMethodNames[methodIndex]);
                }
            }
        }

        return methods;
    }
}
