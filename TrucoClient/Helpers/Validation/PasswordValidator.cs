using System.Linq;

namespace TrucoClient.Helpers.Validation
{
    public static class PasswordValidator
    {
        public static bool IsComplex(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSymbol = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasLower && hasDigit && hasSymbol;
        }

        public static bool ValidateLength(string password, int min, int max)
        {
            return !string.IsNullOrEmpty(password) && password.Length >= min && password.Length <= max;
        }

        public static bool AreMatching(string password1, string password2)
        {
            return string.Equals(password1, password2);
        }
    }
}
