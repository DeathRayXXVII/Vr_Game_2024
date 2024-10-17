using System.Collections.Generic;

namespace ZPTools.Utility
{
    public static class UtilityFunctions
    {
        public static float ToleranceCheck(float value, float newValue, float tolerance = 0.1f)
        {
            // If the difference between the two values is less than the tolerance, return the original value.
            // Otherwise, return the new value rounded to the same number of decimal places as the tolerance.
            if (System.Math.Abs(value - newValue) < tolerance)
            {
                return value;
            }
            return (float) System.Math.Round(newValue, (int) System.Math.Abs(System.Math.Log10(tolerance)));
        }
        
        public static T FetchFromList<T>(List<T> listToProcess, System.Func<T, bool> condition)
        {
            if (listToProcess == null || listToProcess.Count == 0) return default;
            foreach (var obj in listToProcess)
            {
                if (condition(obj)) return obj;
            }
            return default;
        }
        
        public static string ComputeHashSHA(string input)
        {
            // Convert the input string to a byte array and compute the hash.
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

    }
}