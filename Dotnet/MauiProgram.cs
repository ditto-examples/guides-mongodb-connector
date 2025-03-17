using System.Text.Json;
using Guides.Models;
using Guides.Services;
using Guides.ViewModels;
using Guides.Views;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

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
            // Initialize the Syncfusion .NET MAUI Toolkit by adding the below line of code
            .ConfigureSyncfusionToolkit()
            // Initialize the Fonts
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        builder.Services.AddSingleton<AppConfig>(serviceProvider => 
        {
            // Load config synchronously
            using var stream = FileSystem.Current.OpenAppPackageFileAsync("dittoConfig.json").Result;
            using var reader = new StreamReader(stream);
            var fileContent = reader.ReadToEndAsync().Result;
            var config = JsonSerializer.Deserialize<AppConfig>(fileContent, JsonSerializerOptions);
            return config ?? new AppConfig("", "", "");
        });

        //register Services and ViewModels
        builder.Services.AddSingleton<ErrorService>();
        builder.Services.AddSingleton<IDataService, DittoService>();
        builder.Services.AddTransient<MainPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}