using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace AuraUpdater;

public partial class MainWindow : Window
{
    private const string remoteVersionUrl = "https://build.sealion.fr/updates/version.txt";
    private const string remoteZipUrl = "https://build.sealion.fr/updates/Aura.zip";
    private const string localExe = "AURA - Prototype";

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await CheckAndUpdate();
    }

    private async Task CheckAndUpdate()
    {
        try
        {
            if (!File.Exists("version.txt")) File.WriteAllText("version.txt", "0.0.0");
            string current = File.ReadAllText("version.txt").Trim();

            string remote;
            using (HttpClient hc = new())
                remote = (await hc.GetStringAsync(remoteVersionUrl)).Trim();

            Status.Text = $"Locale {current} / Distante {remote}";
            if (current == remote) { LaunchApp(); return; }

            Status.Text = "Téléchargement…";
            string tmpZip = Path.Combine(Path.GetTempPath(), "Aura_Update.zip");
            await DownloadWithProgress(remoteZipUrl, tmpZip);

            Status.Text = "Extraction…";
            ZipFile.ExtractToDirectory(tmpZip, AppDomain.CurrentDomain.BaseDirectory, true);
            File.Delete(tmpZip);
            File.WriteAllText("version.txt", remote);

            Status.Text = "Mise à jour terminée – lancement…";
            await Task.Delay(400);
            LaunchApp();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur Updater", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
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
            Dispatcher.Invoke(() => UpdateBar(readTotal, total ?? 0));
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

    private void LaunchApp()
    {
        Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, localExe));
        Application.Current.Shutdown();
    }
}
