using System.Text.Json;
using Guides.Models;
using Guides.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace Guides;

public static class MauiProgram
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        { PropertyNameCaseInsensitive = true }; 

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<AppConfig>(serviceProvider => 
        {
            // Load config synchronously
            using var stream = FileSystem.Current.OpenAppPackageFileAsync("capellaConfig.json").Result;
            using var reader = new StreamReader(stream);
            var fileContent = reader.ReadToEndAsync().Result;
            var config = JsonSerializer.Deserialize<AppConfig>(fileContent, JsonSerializerOptions);
            return config ?? new AppConfig("", "", "");
        });

        builder.Services.AddSingleton<IDataService, DittoService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}