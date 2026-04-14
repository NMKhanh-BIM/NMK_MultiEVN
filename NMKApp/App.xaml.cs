using System.IO;
using System.Windows;
using Newtonsoft.Json.Linq;
using NMKApp.Services;
using NMKApp.ViewModels;

namespace NMKApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Load configuration
        var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        string supabaseUrl = "https://ffbszfswcppvdhnmsqkd.supabase.co";
        string supabaseKey = "sb_publishable_D2YDfYEZCww-pQFavtV1Xg_TG29l-l3";

        if (File.Exists(configPath))
        {
            var config = JObject.Parse(File.ReadAllText(configPath));
            supabaseUrl = config["Supabase"]?["Url"]?.ToString() ?? supabaseUrl;
            supabaseKey = config["Supabase"]?["Key"]?.ToString() ?? supabaseKey;
        }

        // Initialize services
        var supabaseService = new SupabaseService(supabaseUrl, supabaseKey);
        var outlookService = new OutlookService();
        outlookService.Initialize();

        // Create and wire up ViewModel
        var mainViewModel = new MainViewModel(supabaseService, outlookService);

        // Show main window
        var mainWindow = new MainWindow(mainViewModel);
        mainWindow.Show();
    }
}

