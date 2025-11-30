using System;

namespace TrucoClient.Helpers.Validation
{
    public static class FieldValidator
    {
        public static bool IsRequired(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsLengthInRange(string value, int min, int max)
        {
            if (value == null)
            {
                return false;
            }
          
            return value.Length >= min && value.Length <= max;
        }

        public static bool IsExactLength(string value, int length)
        {
            if (value == null)
            {
                return false;
            }
           
            return value.Length == length;
        }

        public static bool TryParseInt(string value)
        {
            int tmp;
           
            return int.TryParse(value, out tmp);
        }
    }
}
