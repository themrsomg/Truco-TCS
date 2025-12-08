using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrucoClient.TrucoServer;

namespace TrucoClient.Utilities
{
    public class ProfanityValidator
    {
        private Regex bannedWordsRegex;
        private bool isInitialized = false;

        private static ProfanityValidator instance;
        public static ProfanityValidator Instance => instance ?? (instance = new ProfanityValidator());

        public void Initialize(BannedWordList serverList)
        {
            if (serverList?.BannedWords == null || !serverList.BannedWords.Any())
            {
                return;
            }

            var escapedWords = serverList.BannedWords
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .Select(w => Regex.Escape(w.Trim()));

            string pattern = $@"\b({string.Join("|", escapedWords)})\b";

            bannedWordsRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(2));
            isInitialized = true;
        }

        public string CensorText(string text)
        {
            if (!isInitialized || string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            return bannedWordsRegex.Replace(text, match => new string('*', match.Length));
        }
    }
}
