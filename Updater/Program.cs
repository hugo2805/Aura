using System.Runtime.InteropServices;
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
        VelopackApp.Build()
            .OnAfterInstallFastCallback(_ => OnInstall())
            .OnBeforeUninstallFastCallback(_ => OnUninstall())
            .Run();

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // -------------------------------------------------------------------------
    // Hook : premier lancement après installation de l'AppImage
    // Crée le .desktop (menu applicatif + Bureau)
    // -------------------------------------------------------------------------
    private static void OnInstall()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

        string home       = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string installDir = Path.Combine(home, ".local", "share", "AuraInstaller");
        string exePath    = Path.Combine(installDir, "current", "AuraInstaller");
        string iconPath   = Path.Combine(installDir, "current", "logo.png");

        string desktopContent = $"""
            [Desktop Entry]
            Name=Aura
            Comment=Assistant d'Urgence et de Régulation de l'Alerte
            Exec={exePath}
            Icon={iconPath}
            Type=Application
            Categories=Game;Utility;
            Terminal=false
            StartupNotify=true
            """;

        // Menu applicatif (~/.local/share/applications/)
        string appsDir    = Path.Combine(home, ".local", "share", "applications");
        Directory.CreateDirectory(appsDir);
        string menuEntry  = Path.Combine(appsDir, "aura.desktop");
        File.WriteAllText(menuEntry, desktopContent);
        SetExecutable(menuEntry);

        // Raccourci Bureau (cherche le dossier via xdg-user-dirs puis noms courants)
        string desktopDir = GetDesktopDir(home);
        if (!string.IsNullOrEmpty(desktopDir))
        {
            string shortcut = Path.Combine(desktopDir, "aura.desktop");
            File.WriteAllText(shortcut, desktopContent);
            SetExecutable(shortcut);
        }
    }

    // -------------------------------------------------------------------------
    // Hook : désinstallation — supprime les raccourcis
    // -------------------------------------------------------------------------
    private static void OnUninstall()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        TryDelete(Path.Combine(home, ".local", "share", "applications", "aura.desktop"));

        string desktopDir = GetDesktopDir(home);
        if (!string.IsNullOrEmpty(desktopDir))
            TryDelete(Path.Combine(desktopDir, "aura.desktop"));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    // Lit ~/.config/user-dirs.dirs pour trouver le dossier Bureau XDG,
    // avec repli sur les noms courants (Desktop, Bureau…).
    private static string GetDesktopDir(string home)
    {
        string userDirsFile = Path.Combine(home, ".config", "user-dirs.dirs");
        if (File.Exists(userDirsFile))
        {
            foreach (string line in File.ReadAllLines(userDirsFile))
            {
                if (!line.StartsWith("XDG_DESKTOP_DIR=")) continue;
                string val = line.Split('=', 2)[1].Trim().Trim('"')
                                 .Replace("$HOME", home);
                if (Directory.Exists(val)) return val;
            }
        }

        foreach (string name in new[] { "Desktop", "Bureau", "Escritorio" })
        {
            string dir = Path.Combine(home, name);
            if (Directory.Exists(dir)) return dir;
        }

        return string.Empty;
    }

    private static void SetExecutable(string path)
    {
        try
        {
#pragma warning disable CA1416
            File.SetUnixFileMode(path,
                UnixFileMode.UserRead  | UnixFileMode.UserWrite  | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.OtherRead);
#pragma warning restore CA1416
        }
        catch { /* non-fatal */ }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .LogToTrace();
}
