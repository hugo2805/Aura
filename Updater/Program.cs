using Avalonia;
using Velopack;

namespace AuraUpdater;

internal sealed class Program
{
    // CRITIQUE : VelopackApp.Build().Run() doit être la toute première instruction de Main,
    // avant qu'Avalonia ne soit initialisé. Velopack intercepte ici les hooks
    // d'installation/désinstallation/mise à jour injectés par son runtime.
    [STAThread]
    public static void Main(string[] args)
    {
        VelopackApp.Build().Run();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .LogToTrace();
}