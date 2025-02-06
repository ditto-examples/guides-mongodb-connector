using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Guides.Models;

public partial class Planet 
    : ObservableObject
{
    [ObservableProperty] 
    [property: JsonPropertyName("_id")]
    string id;

    [ObservableProperty] 
    [property: JsonPropertyName("hasRings")]
    bool hasRings;

    [ObservableProperty] 
    [property: JsonPropertyName("isArchived")]
    bool isArchived;

    [ObservableProperty] 
    [property: JsonPropertyName("mainAtmosphere")]
    List<string> mainAtmosphere;
    
    [ObservableProperty] 
    [property: JsonPropertyName("name")] 
    string name;

    [ObservableProperty] 
    [property: JsonPropertyName("orderFromSun")]
    int orderFromSun;
    
    [ObservableProperty] 
    [property: JsonPropertyName("planetId")] 
    string planetId;

    [ObservableProperty] 
    [property: JsonPropertyName("surfaceTemperatureC")]
    private Temperature surfaceTemperatureC;

    public Planet(string id,
        bool hasRings,
        bool isArchived,
        List<string> mainAtmosphere,
        string name,
        int orderFromSun,
        string planetId,
        Temperature surfaceTemperatureC)
    {
        Id = id;
        HasRings = hasRings;
        IsArchived = isArchived;
        MainAtmosphere = mainAtmosphere;
        Name = name;
        OrderFromSun = orderFromSun;
        PlanetId = planetId;
        SurfaceTemperatureC = surfaceTemperatureC;
    }
    
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { "_id", Id },
            { "hasRings", HasRings },
            { "isArchived", IsArchived },
            { "mainAtmosphere", MainAtmosphere },
            { "name", Name },
            { "orderFromSun", OrderFromSun },
            { "planetId", PlanetId },
            { "surfaceTemperatureC", SurfaceTemperatureC.ToDictionary() }
        };
    }

    public static Planet FromDictionary(Dictionary<string, object?> value)
    {
        return new Planet(
            id: value["_id"]?.ToString() ?? string.Empty,
            hasRings: Convert.ToBoolean(value["hasRings"]),
            isArchived: Convert.ToBoolean(value["isArchived"]),
            mainAtmosphere: ((value["mainAtmosphere"] as List<object>)?.Select(x => x.ToString() ?? string.Empty).ToList() ?? new List<string>()),
            name: value["name"]?.ToString() ?? string.Empty,
            orderFromSun: Convert.ToInt32(value["orderFromSun"]),
            planetId: value["planetId"]?.ToString() ?? string.Empty,
            surfaceTemperatureC: Temperature.FromDictionary((Dictionary<string, object?>)value["surfaceTemperatureC"]!)
        );
    }
}

public partial class Temperature(
    double? max,
    double mean,
    double? min) : ObservableObject
{
    [ObservableProperty] 
    [property: JsonPropertyName("max")] 
    private double? max = max;
    
    [ObservableProperty] 
    [property: JsonPropertyName("mean")] 
    double mean = mean;
    
    [ObservableProperty] 
    [property: JsonPropertyName("min")] 
    double? min = min;

    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>
        {
            { "mean", Mean }
        };

        if (Max.HasValue)
            dict.Add("max", Max.Value);
        if (Min.HasValue)
            dict.Add("min", Min.Value);

        return dict;
    }

    public static Temperature FromDictionary(Dictionary<string, object?> temp)
    {
        return new Temperature(
            max: temp["max"] != null ? Convert.ToDouble(temp["max"]) : null,
            mean: Convert.ToDouble(temp["mean"]),
            min: temp["min"] != null ? Convert.ToDouble(temp["min"]) : null
        );
    }
}