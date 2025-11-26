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
            string code = Properties.Settings.Default.languageCode ?? "en-US";
           
            if (string.IsNullOrEmpty(code))
            {
                var osCulture = CultureInfo.InstalledUICulture;
                code = osCulture.TwoLetterISOLanguageName.Equals("es", StringComparison.OrdinalIgnoreCase)
                       ? "es-MX"
                       : "en-US";

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
