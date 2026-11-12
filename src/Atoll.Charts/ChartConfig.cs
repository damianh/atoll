using System.Text.Json;

namespace Atoll.Charts;

internal static class ChartConfig
{
    internal static bool TryParse(string json, out string? chartType)
    {
        chartType = null;

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("type", out var type) &&
                type.ValueKind == JsonValueKind.String)
            {
                chartType = type.GetString();
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
