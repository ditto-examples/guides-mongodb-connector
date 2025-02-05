using System.Text.Json.Serialization;

namespace Guides.Models;

public record Planet(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("hasRings")] bool HasRings,
    [property: JsonPropertyName("isArchived")] bool IsArchived,
    [property: JsonPropertyName("mainAtmosphere")] List<string> MainAtmosphere,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("orderFromSun")] int OrderFromSun,
    [property: JsonPropertyName("planetId")] string PlanetId,
    [property: JsonPropertyName("surfaceTemperatureC")] Temperature SurfaceTemperatureC)
{
    public static Planet FromDictionary(Dictionary<string, object?> value)
    {
        return new Planet(
            Id: value["_id"]?.ToString() ?? string.Empty,
            HasRings: Convert.ToBoolean(value["hasRings"]),
            IsArchived: Convert.ToBoolean(value["isArchived"]),
            MainAtmosphere: ((value["mainAtmosphere"] as List<object>)?.Select(x => x.ToString() ?? string.Empty).ToList() ?? new List<string>()),
            Name: value["name"]?.ToString() ?? string.Empty,
            OrderFromSun: Convert.ToInt32(value["orderFromSun"]),
            PlanetId: value["planetId"]?.ToString() ?? string.Empty,
            SurfaceTemperatureC: Temperature.FromDictionary((Dictionary<string, object?>)value["surfaceTemperatureC"]!)
        );
    }
}

public record Temperature(
    [property: JsonPropertyName("max")] double? Max,
    [property: JsonPropertyName("mean")] double Mean,
    [property: JsonPropertyName("min")] double? Min)
{
    public static Temperature FromDictionary(Dictionary<string, object?> temp)
    {
        return new Temperature(
            Max: temp["max"] != null ? Convert.ToDouble(temp["max"]) : null,
            Mean: Convert.ToDouble(temp["mean"]),
            Min: temp["min"] != null ? Convert.ToDouble(temp["min"]) : null
        );
    }
}