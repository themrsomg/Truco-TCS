using System.Threading;
using System.Windows;
using TrucoClient.Helpers.Localization;

namespace TrucoClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var langCode = TrucoClient.Properties.Settings.Default.languageCode; 
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(langCode);
            base.OnStartup(e);
            LanguageManager.ApplyLanguage();

            InitialWindows mainWindow = new InitialWindows();
            this.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}