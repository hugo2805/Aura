using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Threading;
using Velopack;
using Velopack.Sources;

namespace AuraUpdater;

public partial class MainWindow : Window
{
    private const string remoteVersionUrl = "https://build.sealion.fr/updates/version.txt";

    private static readonly string localExe = RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
        ? "Aura.x86_64"
        : "AURA - Prototype";

    // Répertoire de données utilisateur (hors AppImage, donc writable)
    private static readonly string DataDir = GetDataDirectory();

    private static string GetDataDirectory()
    {
        string dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local", "share", "Aura");
        Directory.CreateDirectory(dir);
        return dir;
    }

    // ZIP du jeu Unity — build différent par plateforme
    private static readonly string remoteZipUrl = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "https://build.sealion.fr/updates/win/Aura.zip"
        : "https://build.sealion.fr/updates/linux/Aura.zip";

    // URL Velopack par plateforme
    private static readonly string VelopackUpdateUrl = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "https://build.sealion.fr/releases/win"
        : "https://build.sealion.fr/releases/linux";

    public MainWindow()
    {
        InitializeComponent();
        Opened += async (_, _) =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                EnsureDesktopIntegration();
            await CheckForSelfUpdate();
            await CheckAndUpdate();
        };
    }

    // -------------------------------------------------------------------------
    // Intégration bureau Linux — crée le .desktop au premier lancement
    // -------------------------------------------------------------------------
    private static void EnsureDesktopIntegration()
    {
        try
        {
            string home     = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string menuFile = Path.Combine(home, ".local", "share", "applications", "aura.desktop");
            if (File.Exists(menuFile)) return; // déjà installé

            // Chemin réel de l'exécutable : variable APPIMAGE si on tourne en AppImage,
            // sinon le binaire installé par Velopack
            string execPath = Environment.GetEnvironmentVariable("APPIMAGE")
                              ?? Environment.ProcessPath
                              ?? string.Empty;
            if (string.IsNullOrEmpty(execPath)) return;

            // Copie l'icône dans DataDir (accessible en écriture) si besoin
            string iconPath = Path.Combine(DataDir, "logo.png");
            if (!File.Exists(iconPath))
            {
                // logo.png est embarqué dans le AppImage (AppDir/logo.png)
                string appDir    = Environment.GetEnvironmentVariable("APPDIR") ?? string.Empty;
                string bundled   = Path.Combine(appDir, "logo.png");
                if (File.Exists(bundled)) File.Copy(bundled, iconPath, overwrite: false);
            }

            string content = $"""
                [Desktop Entry]
                Name=Aura
                Comment=Assistant d'Urgence et de Régulation de l'Alerte
                Exec={execPath}
                Icon={iconPath}
                Type=Application
                Categories=Game;Utility;
                Terminal=false
                StartupNotify=true
                """;

            Directory.CreateDirectory(Path.Combine(home, ".local", "share", "applications"));
            File.WriteAllText(menuFile, content);
#pragma warning disable CA1416
            File.SetUnixFileMode(menuFile,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.OtherRead);
#pragma warning restore CA1416

            // Raccourci Bureau
            string desktopDir = GetDesktopDir(home);
            if (!string.IsNullOrEmpty(desktopDir))
            {
                string shortcut = Path.Combine(desktopDir, "aura.desktop");
                File.WriteAllText(shortcut, content);
#pragma warning disable CA1416
                File.SetUnixFileMode(shortcut,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.OtherRead);
#pragma warning restore CA1416
            }
        }
        catch { /* non-fatal */ }
    }

    private static string GetDesktopDir(string home)
    {
        string userDirsFile = Path.Combine(home, ".config", "user-dirs.dirs");
        if (File.Exists(userDirsFile))
        {
            foreach (string line in File.ReadAllLines(userDirsFile))
            {
                if (!line.StartsWith("XDG_DESKTOP_DIR=")) continue;
                string val = line.Split('=', 2)[1].Trim().Trim('"').Replace("$HOME", home);
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

    // -------------------------------------------------------------------------
    // Auto-mise-à-jour du launcher (Velopack)
    // Si une nouvelle version est disponible : télécharge et redémarre.
    // En cas d'erreur (pas de réseau, serveur non encore configuré…) : continue.
    // -------------------------------------------------------------------------
    private async Task CheckForSelfUpdate()
    {
        try
        {
            Status.Text = "Vérification de la mise à jour du launcher…";
            var mgr = new UpdateManager(new SimpleWebSource(VelopackUpdateUrl));

            var updateInfo = await mgr.CheckForUpdatesAsync();
            if (updateInfo == null) return; // déjà à jour

            Status.Text = $"Mise à jour du launcher ({updateInfo.TargetFullRelease.Version})…";
            await mgr.DownloadUpdatesAsync(updateInfo, progress =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Bar.IsIndeterminate = false;
                    Bar.Value = progress;
                    Pct.Text = $"{progress} %";
                });
            });

            mgr.ApplyUpdatesAndRestart(updateInfo); // redémarre avec la nouvelle version, ne retourne pas
        }
        catch
        {
            // Non-fatal : on continue avec la mise à jour du jeu Unity
        }
    }

    // -------------------------------------------------------------------------
    // Mise à jour du jeu Unity (ZIP)
    // -------------------------------------------------------------------------
    private async Task CheckAndUpdate()
    {
        try
        {
            string versionFile = Path.Combine(DataDir, "version.txt");
            if (!File.Exists(versionFile)) File.WriteAllText(versionFile, "0.0.0");
            string current = File.ReadAllText(versionFile).Trim();

            string remote;
            using (HttpClient hc = new())
                remote = (await hc.GetStringAsync(remoteVersionUrl)).Trim();

            string gameExe = Path.Combine(DataDir, localExe);
            Status.Text = $"Locale {current} / Distante {remote}";
            if (current == remote && File.Exists(gameExe)) { LaunchApp(); return; }

            Status.Text = "Téléchargement…";
            string tmpZip = Path.Combine(Path.GetTempPath(), "Aura_Update.zip");
            await DownloadWithProgress(remoteZipUrl, tmpZip);

            Status.Text = "Extraction…";
            ZipFile.ExtractToDirectory(tmpZip, DataDir, true);
            File.Delete(tmpZip);
            File.WriteAllText(versionFile, remote);
            SetGameExecutable();

            Status.Text = "Mise à jour terminée – lancement…";
            await Task.Delay(400);
            LaunchApp();
        }
        catch (Exception ex)
        {
            Status.Foreground = new SolidColorBrush(Color.Parse("#FF4444"));
            Status.Text = $"Erreur : {ex.Message}";
            await Task.Delay(3000);
            ShutdownApp();
        }
    }

    private async Task DownloadWithProgress(string url, string outPath)
    {
        using HttpClient hc = new();
        using var resp = await hc.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();
        long? total = resp.Content.Headers.ContentLength;
        using var http = await resp.Content.ReadAsStreamAsync();
        using var file = File.Create(outPath);
        const int buf = 64 * 1024;
        var buffer = new byte[buf];
        long readTotal = 0;
        int read;
        while ((read = await http.ReadAsync(buffer.AsMemory(0, buf))) > 0)
        {
            await file.WriteAsync(buffer.AsMemory(0, read));
            readTotal += read;
            await Dispatcher.UIThread.InvokeAsync(() => UpdateBar(readTotal, total ?? 0));
        }
    }

    private void UpdateBar(long sent, long total)
    {
        if (total == 0) { Bar.IsIndeterminate = true; return; }
        Bar.IsIndeterminate = false;
        double pct = sent / (double)total;
        Bar.Value = pct * 100;
        Pct.Text = $"{pct:P0}";
    }

    private void SetGameExecutable()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;
        try
        {
#pragma warning disable CA1416
            string exe = Path.Combine(DataDir, localExe);
            if (File.Exists(exe))
                File.SetUnixFileMode(exe,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                    UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                    UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
#pragma warning restore CA1416
        }
        catch { /* non-fatal */ }
    }

    private void LaunchApp()
    {
        var psi = new ProcessStartInfo(Path.Combine(DataDir, localExe))
        {
            UseShellExecute = false,
            WorkingDirectory = DataDir,
        };
        Process.Start(psi);
        ShutdownApp();
    }

    private static void ShutdownApp()
    {
        if (Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
        else
            Environment.Exit(0);
    }
}