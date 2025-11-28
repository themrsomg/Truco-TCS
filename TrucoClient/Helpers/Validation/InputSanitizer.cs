using System.Linq;

namespace TrucoClient.Helpers.Validation
{
    public static class InputSanitizer
    {
        private static readonly string[] dangerousPatterns =
        {
            ";", "--", "'",
            "\"", "<", ">", "/*", "*/", "\\"
        };

        public static bool ContainsDangerousCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return dangerousPatterns.Any(pattern => input.Contains(pattern));
        }

        public static string SanitizeForCodeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            foreach (string pattern in dangerousPatterns)
            {
                input = input.Replace(pattern, string.Empty);
            }

            return input;
        }

    }
}
