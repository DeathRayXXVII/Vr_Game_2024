using System.Collections.Generic;

namespace ZPTools.Utility
{
    public static class UtilityFunctions
    {
        public static float ToleranceCheck(float value, float newValue, float tolerance = 0.1f)
        {
            return System.Math.Abs(value - newValue) < tolerance ? value : newValue;
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
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

    }
}