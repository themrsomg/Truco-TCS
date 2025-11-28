using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrucoClient.TrucoServer;

namespace TrucoClient.Utilities
{
    public class ProfanityValidator
    {
        private readonly HashSet<string> bannedWords;
        private bool isInitialized = false;

        private static ProfanityValidator instance;
        public static ProfanityValidator Instance => instance ?? (instance = new ProfanityValidator());

        private ProfanityValidator()
        {
            bannedWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public void Initialize(BannedWordList serverList)
        {
            if (serverList?.BannedWords == null)
            {
                return;
            }

            bannedWords.Clear();

            bannedWords.UnionWith(
                serverList.BannedWords
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Select(word => word.Trim())
            );

            isInitialized = true;
        }

        public bool ContainsProfanity(string text)
        {
            if (!isInitialized || string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var tokens = text.Split(' ');
            return tokens.Any(token => bannedWords.Contains(token));
        }

        public string CensorText(string text)
        {
            if (!isInitialized || string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            string processedText = text;

            var wordsFoundInText = bannedWords
                .Where(word => processedText.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var badWord in wordsFoundInText)
            {
                string pattern = $@"\b{Regex.Escape(badWord)}\b";
                string replacement = new string('*', badWord.Length);

                processedText = Regex.Replace(
                    processedText,
                    pattern,
                    replacement,
                    RegexOptions.IgnoreCase,
                    TimeSpan.FromMilliseconds(500));
            }

            return processedText;
        }

        public bool IsInitialized() 
        { 
            return isInitialized; 
        }
    }
}
