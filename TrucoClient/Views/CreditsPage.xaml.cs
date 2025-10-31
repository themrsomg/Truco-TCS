using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.IO;

namespace TrucoClient
{
    public partial class CreditsPage : Page
    {
        public CreditsPage()
        {
            InitializeComponent();
            MusicInitializer.InitializeMenuMusic();
        }

        private void ClickBack(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new SettingsPage());
        }
        private void ClickEgg(object sender, RoutedEventArgs e)
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = System.IO.Path.Combine(currentDirectory, "Resources", "NobodyBeatsTheHammer.exe");

            try
            {
                if (!File.Exists(fullPath))
                {
                    MessageBox.Show($"Error de Archivo: El ejecutable no fue encontrado en la ruta: {fullPath}", "Error de Ejecución", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Process.Start(fullPath);
                if (this.NavigationService != null)
                {
                    this.NavigationService.Navigate(new SettingsPage());
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"No se pudo ejecutar el programa.Detalle: {ex.Message}", "Error Crítico", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Error de Ejecución", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
