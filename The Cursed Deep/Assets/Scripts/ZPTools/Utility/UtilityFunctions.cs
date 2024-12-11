using System.Collections.Generic;
using System.Linq;

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
        
        public static bool PerformActionOnInterface<T>(System.Action<T> action) where T : class
        {
            var matchingObjects = new List<T>();
            foreach (var obj in UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.MonoBehaviour>())
            {
                if (obj is T match) matchingObjects.Add(match);
            }

            foreach (var obj in UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.ScriptableObject>())
            {
                if (obj is T match) matchingObjects.Add(match);
            }

            foreach (var interfaceObject in matchingObjects)
            {
                if (interfaceObject == null)
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogError("[ERROR] Interface object is null");
#endif
                    continue;
                }
                try
                {
                    action(interfaceObject);
                }
#if UNITY_EDITOR
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError(e, null);
                }
#else
                catch(System.Exception)
                {
                    // ignored
                }
#endif
            }
            
            return true;
        }
        
        public static string ComputeHashSHA(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return System.BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        
        public static T AdvancedGetComponent<T>(UnityEngine.GameObject obj, bool allowDebug = false) where T : class
        {
            var returnComponent = (obj.GetComponent<T>() ?? obj.GetComponentInChildren<T>()) ?? obj.GetComponentInParent<T>();
            if (returnComponent != null)
            {
#if UNITY_EDITOR
                if (allowDebug) UnityEngine.Debug.Log($"Found {typeof(T)} in {obj.name}", obj);
#endif
                return returnComponent;
            }
#if UNITY_EDITOR
            if (allowDebug) UnityEngine.Debug.LogWarning($"No {typeof(T)} found in {obj.name}", obj);
#endif
            return null;
        }
        
        public static bool ValidateJsonKey(string key, Newtonsoft.Json.Linq.JObject data) => data.Properties().Any(property => property.Name == key);
        public static IEnumerable<string> GetJsonKeys(Newtonsoft.Json.Linq.JObject data) => data.Properties().Select(property => property.Name);
    }
}