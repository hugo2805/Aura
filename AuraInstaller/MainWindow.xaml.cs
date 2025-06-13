using System.Diagnostics;
using System.IO;
using System.Windows;

namespace AuraInstaller
{
    public partial class MainWindow : Window
    {
        public MainWindow()          
        {
            InitializeComponent();   
        }

        private void Refuser_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void Installer_Click(object sender, RoutedEventArgs e)
        {
            string updateExe = Path.Combine(Directory.GetCurrentDirectory(), "Update.exe");

            if (!File.Exists(updateExe))
            {
                MessageBox.Show("Fichier Update.exe introuvable !");
                return;
            }

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = updateExe,
                Arguments = "--install .",
                UseShellExecute = false
            });

            await process.WaitForExitAsync();

            MessageBox.Show("Installation terminée !");
            Application.Current.Shutdown();
        }
    }
}

