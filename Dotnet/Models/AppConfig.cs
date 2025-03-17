using System.Text.Json.Serialization;

namespace Guides.Models;

public record AppConfig(
    [property: JsonPropertyName("appId")] string AppId,
    [property: JsonPropertyName("authToken")] string AuthToken,
    [property: JsonPropertyName("endpointUrl")] string EndpointUrl);