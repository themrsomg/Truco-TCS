using System;
using System.Globalization;
using System.Threading;

namespace TrucoClient
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

            LanguageChanged?.Invoke();
        }

        public static void ApplyLanguage()
        {
            string code = Properties.Settings.Default.languageCode ?? "en-US";
            var culture = new CultureInfo(code);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
