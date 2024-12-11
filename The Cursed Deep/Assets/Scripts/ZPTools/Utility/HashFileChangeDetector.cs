using System.IO;
using UnityEngine;
using static ZPTools.Utility.UtilityFunctions;

namespace ZPTools.Utility
{
    public class HashFileChangeDetector : FileChangeDetector
    {
        private readonly bool _allowDebug;
        
        private string _lastFileHash;
        private readonly string _hashFilePath;

        public HashFileChangeDetector(string filePath, bool allowDebug = false)
        {
            _allowDebug = allowDebug;
            this.filePath = filePath;
            _hashFilePath = GetHashFilePath(filePath);
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"[DEBUG] Hash Change Detector created:\n   FILEPATH: {filePath}\n    CACHE: {_hashFilePath}");
#endif

            // Try to load the last hash from the saved hash file
            LoadLastHash();
        }

        public override bool HasChanged()
        {
            if (!File.Exists(filePath))
            {
#if UNITY_EDITOR
                Debug.LogError($"[ERROR] File not found: {filePath}");
#endif
                return false;
            }

            var fileData = File.ReadAllText(filePath);
            var currentFileHash = ComputeHashSHA(fileData);

            // Check if the hash has changed
            if (_lastFileHash != null)
                return !string.Equals(currentFileHash, _lastFileHash, System.StringComparison.Ordinal);

#if UNITY_EDITOR
            if (_allowDebug) Debug.LogWarning("[WARNING] Last file hash is null; assuming file has changed.");
#endif
            return true;
        }

        public sealed override void UpdateState()
        {
            if (!File.Exists(filePath))
            {
#if UNITY_EDITOR
                Debug.LogError($"[ERROR] File not found: {filePath}");
#endif
                return;
            }
            
            var fileData = File.ReadAllText(filePath);
            _lastFileHash = ComputeHashSHA(fileData);

            // Save the current hash to the hash file
            SaveHash();
        }

        private static string GetHashFilePath(string filePath)
        {
            // Generate a unique hash file path based on the original file path
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var hashFileName = $"{fileName}_hash.txt";
            return Path.Combine(Application.persistentDataPath, hashFileName).Replace("\\", "/");
        }

        private void SaveHash()
        {
            if (string.IsNullOrEmpty(_lastFileHash) || string.IsNullOrEmpty(_hashFilePath)) return;
            try
            {
                // Write only the hash to the file
                File.WriteAllText(_hashFilePath, _lastFileHash);
#if UNITY_EDITOR
                if (_allowDebug) Debug.Log($"[DEBUG] Hash saved to: {_hashFilePath}");
#endif
            }
            catch (IOException e)
            {
                Debug.LogError($"[ERROR] Failed to save hash: {e.Message}");
            }
        }

        private void LoadLastHash()
        {
            if (File.Exists(_hashFilePath))
            {
                try
                {
                    // Read the hash value from the hash file
                    _lastFileHash = File.ReadAllText(_hashFilePath);
#if UNITY_EDITOR
                    if (_allowDebug) Debug.Log($"[DEBUG] Hash loaded from: {_hashFilePath}");
#endif
                }
                catch (IOException e)
                {
                    Debug.LogError($"[ERROR] Failed to load hash: {e.Message}");
                }
            }
            else
            {
#if UNITY_EDITOR
                if (_allowDebug) Debug.LogWarning($"[WARNING] No hash file found at {_hashFilePath}. Assuming no previous state.");
#endif
            }
        }
    }
}
