using System.Security.Claims;

namespace AccountingERP.API.Middleware;

public class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = context.User.FindFirstValue("tenant_id")
            ?? context.Request.Headers["X-Tenant-Id"].FirstOrDefault();

        if (!string.IsNullOrEmpty(tenantId) && int.TryParse(tenantId, out var id))
            context.Items["TenantId"] = id;

        await next(context);
    }
}
