using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace TrucoClient.Helpers.Validation
{
    public static class EmailValidator
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) 
            {
                return false;
            }
         
            if (email.Contains("..") || email.Contains(" "))
            { 
                return false;
            }

            try
            {
                var addr = new MailAddress(email);
             
                return Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
                    RegexOptions.None, TimeSpan.FromMilliseconds(500)) && addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsCommonDomain(string email)
        {
            string[] common = {
                "gmail.com","outlook.com","hotmail.com","yahoo.com","icloud.com",
                "live.com","aol.com","protonmail.com","uv.mx","estudiantes.uv.mx"
            };

            string domain = email?.Split('@').Length == 2 ? email.Split('@')[1].ToLower() : string.Empty;
            
            return common.Contains(domain);
        }
    }
}
