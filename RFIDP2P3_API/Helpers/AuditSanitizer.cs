using System.Text.Json;
using System.Text.Json.Nodes;

namespace RFIDP2P3_API.Helpers;

public class AuditSanitizer
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "confirmpassword", "oldpassword", "newpassword",
        "token", "accesstoken", "refreshtoken", "secret", "pin", "cvv",
        "cardnumber", "authorization", "apikey"
    };

    public static string? SanitizePayload(object? payload)
    {
        if (payload == null) return null;

        try
        {
            var jsonString = payload is string str ? str : JsonSerializer.Serialize(payload);
            var node = JsonNode.Parse(jsonString);

            if (node == null) return null;

            MaskJsonNode(node);
            return node.ToJsonString();
        }
        catch
        {
            return "{\"warning\": \"Payload cannot be serialized to JSON format\"}";
        }
    }

    private static void MaskJsonNode(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            var keys = obj.Select(k => k.Key).ToList();
            foreach (var key in keys)
            {
                if (SensitiveKeys.Contains(key))
                    obj[key] = "***MASKED***";
                else if (obj[key] != null)
                    MaskJsonNode(obj[key]!);
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item != null) MaskJsonNode(item);
            }
        }
    }
}