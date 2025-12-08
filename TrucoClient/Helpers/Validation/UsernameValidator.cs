using System;
using System.Text.RegularExpressions;

namespace TrucoClient.Helpers.Validation
{
    public static class UsernameValidator
    {
        private static readonly Regex usernameRegex = new Regex(
            @"^[a-zA-Z0-9]+(_[a-zA-Z0-9]+)?$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(500));

        public static bool IsValidFormat(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }
         
            return usernameRegex.IsMatch(username);
        }

        public static bool ValidateLength(string username, int min, int max)
        {
            return !string.IsNullOrEmpty(username) && username.Length >= min && username.Length <= max;
        }
    }
}
