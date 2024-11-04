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
        
        public static void PerformActionOnInterface<T>(System.Action<T> action) where T : class
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

            foreach (var loader in matchingObjects)
            {
                try
                {
                    action(loader);
                }
                catch (System.Exception e)
                {
#if UNITY_EDITOR
                    UnityEngine.Debug.LogError(e, null);
#endif
                }
            }
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

    }
}