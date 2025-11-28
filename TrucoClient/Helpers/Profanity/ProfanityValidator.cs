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

            foreach (var word in serverList.BannedWords)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    bannedWords.Add(word.Trim());
                }
            }
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

            foreach (var badWord in bannedWords)
            {
                if (processedText.IndexOf(badWord, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string pattern = $@"\b{Regex.Escape(badWord)}\b";

                    string replacement = new string('*', badWord.Length);

                    processedText = Regex.Replace(processedText, pattern, replacement, RegexOptions.IgnoreCase);
                }
            }

            return processedText;
        }

        public bool IsInitialized() 
        { 
            return isInitialized; 
        }
    }
}
