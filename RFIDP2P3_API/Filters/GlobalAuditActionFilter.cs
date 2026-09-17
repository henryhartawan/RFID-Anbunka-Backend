using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using RFIDP2P3_API.Attributes;
using RFIDP2P3_API.Services.Interfaces;

namespace RFIDP2P3_API.Filters;

public class GlobalAuditActionFilter : IAsyncActionFilter
{
    private readonly IAuditService _auditService;
    private static readonly HashSet<string> AuditedHttpMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST", "PUT", "DELETE", "PATCH"
    };

    public GlobalAuditActionFilter(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpMethod = context.HttpContext.Request.Method;

        // 1. Ignore HTTP GET / OPTIONS
        if (!AuditedHttpMethods.Contains(httpMethod))
        {
            await next();
            return;
        }

        // 2. Check for [IgnoreAudit] attribute
        if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            var hasIgnoreAttr = descriptor.MethodInfo.IsDefined(typeof(IgnoreAuditAttribute), true) ||
                                descriptor.ControllerTypeInfo.IsDefined(typeof(IgnoreAuditAttribute), true);
            if (hasIgnoreAttr)
            {
                await next();
                return;
            }
        }

        var controllerName = (context.ActionDescriptor as ControllerActionDescriptor)?.ControllerName ?? "ApiController";
        var actionName = (context.ActionDescriptor as ControllerActionDescriptor)?.ActionName ?? httpMethod;
        var payload = context.ActionArguments;

        string? entityId = null;

        if (context.RouteData.Values.TryGetValue("id", out var routeId) && routeId != null)
            entityId = routeId.ToString();
        else if (context.HttpContext.Request.Query.TryGetValue("id", out var queryId))
            entityId = queryId.ToString();

        if (string.IsNullOrEmpty(entityId) && payload != null)
            entityId = ExtractIdFromJson(payload);

        ActionExecutedContext executedContext;
        try
        {
            executedContext = await next();
        }
        catch (Exception ex)
        {
            _auditService.Log(
                action: $"{httpMethod}_{actionName}",
                entityName: controllerName,
                entityId: entityId,
                payload: new { RequestData = payload, Error = ex.Message },
                status: "FAILED"
            );
            throw;
        }

        var isSuccess = executedContext.Exception == null &&
                        context.HttpContext.Response.StatusCode >= 200 &&
                        context.HttpContext.Response.StatusCode < 300;

        _auditService.Log(
            action: $"{httpMethod}_{actionName}",
            entityName: controllerName,
            entityId: entityId,
            payload: payload,
            status: isSuccess ? "SUCCESS" : "FAILED"
        );
    }

    private string? ExtractIdFromJson(object payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            using var doc = JsonDocument.Parse(json);
            return SearchProperty(doc.RootElement);
        }
        catch
        {
            return null;
        }
    }

    private string? SearchProperty(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in element.EnumerateObject())
            {
                string propName = prop.Name;

                bool isIdField = propName.Equals("id", StringComparison.OrdinalIgnoreCase) ||
                                 propName.EndsWith("id", StringComparison.OrdinalIgnoreCase) ||
                                 propName.EndsWith("_no", StringComparison.OrdinalIgnoreCase);

                if (isIdField && prop.Value.ValueKind != JsonValueKind.Null)
                {
                    var val = prop.Value.ToString();
                    if (!string.IsNullOrWhiteSpace(val)) return val;
                }

                if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                {
                    var result = SearchProperty(prop.Value);
                    if (result != null) return result;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var result = SearchProperty(item);
                if (result != null) return result;
            }
        }
        return null;
    }
}