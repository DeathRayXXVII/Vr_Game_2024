using UnityEngine;
using ZPTools.Interface;
using ZPTools.Utility;

namespace ShipGame.Inventory
{
    public abstract class GameSelectionData : ScriptableObject, IStartupLoader
    {
#if UNITY_EDITOR
        [SerializeField] protected bool _allowDebug;
#endif
        
        [SerializeField] [InspectorReadOnly] protected int currentIndex;
        private HashFileChangeDetector _hashFileChangeDetector;
        
        public abstract int selectionIndex { get; set; }
        public bool isLoaded { get; private set; }

        // Common paths that derived classes should define
        protected abstract string dataFilePath { get; }
        protected abstract string resourcePath { get; }

        // Common data structure method to be implemented by derived classes
        protected abstract void InitializeData(int count);

        // Common method for logging data, allowing derived classes to provide specific logging
        protected abstract void LogCurrentData();

        public void LoadOnStartup()
        {
            // Initialize the HashFileChangeDetector if it hasn't been already
            _hashFileChangeDetector ??= new HashFileChangeDetector(dataFilePath, _allowDebug);

            var hasChanged = _hashFileChangeDetector.HasChanged();
            
            // Use the change detector to see if the JSON has changed
            if (isLoaded && hasChanged == false)
            {
#if UNITY_EDITOR
                if (_allowDebug) Debug.LogWarning($"{name} is already loaded, and the file has not changed.", this);
                LogCurrentData();
#endif
                return;
            }
            
            var jsonFile = Resources.Load<TextAsset>(resourcePath);

            if (!jsonFile)
            {
#if UNITY_EDITOR
                if (_allowDebug) Debug.LogError($"JSON file not found at {resourcePath}.", this);
#endif
                return;
            }

            var objectCount = ParseJsonFile(jsonFile.text);
            
            InitializeData(objectCount);
            _hashFileChangeDetector.UpdateState();
            
            Resources.UnloadAsset(jsonFile);

            LogCurrentData();
            
            isLoaded = true;
        }

        // Method to parse the JSON file, returning the number of elements (to be implemented by derived classes)
        protected abstract int ParseJsonFile(string jsonContent);
    }
}
