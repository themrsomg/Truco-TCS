using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using TrucoClient.Helpers.Localization;

namespace TrucoClient
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                string currentSetting = TrucoClient.Properties.Settings.Default.languageCode;

                if (string.IsNullOrWhiteSpace(currentSetting))
                {
                    string selectedCulture = ResolveCulture();
                    TrucoClient.Properties.Settings.Default.languageCode = selectedCulture;
                    TrucoClient.Properties.Settings.Default.Save();

                    CultureInfo cultureInfo = new CultureInfo(selectedCulture);
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Thread.CurrentThread.CurrentUICulture = cultureInfo;
                }

                base.OnStartup(e);

                LanguageManager.ApplyLanguage();

                InitialWindows mainWindow = new InitialWindows();
                this.MainWindow = mainWindow;
                mainWindow.Show();
            }
            catch (CultureNotFoundException)
            {
                CultureInfo fallbackCulture = new CultureInfo("es-MX");
                Thread.CurrentThread.CurrentCulture = fallbackCulture;
                Thread.CurrentThread.CurrentUICulture = fallbackCulture;

                base.OnStartup(e);
                LanguageManager.ApplyLanguage();

                InitialWindows mainWindow = new InitialWindows();
                this.MainWindow = mainWindow;
                mainWindow.Show();
            }
        }

        private static string ResolveCulture()
        {
            string systemLang = CultureInfo.CurrentUICulture.Name.ToLowerInvariant();

            if (systemLang == "es-mx")
            {
                return "es-MX";
            }

            if (systemLang == "en-us")
            {
                return "en-US";
            }

            return "es-MX";
        }
    }
}
