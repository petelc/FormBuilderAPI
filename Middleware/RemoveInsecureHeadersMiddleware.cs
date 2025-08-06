using Microsoft.Extensions.Primitives;

namespace FormBuilderAPI.Middleware;

public class RemoveInsecureHeadersMiddleware
{
    private readonly RequestDelegate _next;
    
    public RemoveInsecureHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext context)
    {
        context.Response.OnStarting((state) =>
        {
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            context.Response.Headers.Remove("X-AspNet-Version");
            context.Response.Headers.Remove("X-AspNetMVC-Version");
            context.Response.Headers.Add("X-Content-Type-Options", new StringValues("nosniff"));
            context.Response.Headers.Add("X-Frame-Options", new StringValues("DENY"));
            return Task.CompletedTask;
        }, null!);
        await _next(context);
    }
}