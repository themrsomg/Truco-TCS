using System;
using System.Globalization;
using System.Threading;
using TrucoClient.Properties.Langs;

namespace TrucoClient.Helpers.Localization
{
    public static class LanguageManager
    {
        public static event Action LanguageChanged;

        public static void ChangeLanguage(string languageCode)
        {
            var culture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            Properties.Settings.Default.languageCode = languageCode;
            Properties.Settings.Default.Save();

            ApplyLanguage();
            LanguageChanged?.Invoke();
        }

        public static void ApplyLanguage()
        {
            string code = Properties.Settings.Default.languageCode;

            if (string.IsNullOrWhiteSpace(code))
            {
                code = "es-MX";
                Properties.Settings.Default.languageCode = code;
                Properties.Settings.Default.Save();
            }

            var culture = new CultureInfo(code);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            Lang.Culture = culture;
        }
    }
}
