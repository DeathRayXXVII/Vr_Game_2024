using UnityEngine;
using ZPTools.Interface;
using ZPTools.Utility;

namespace ShipGame.ScriptObj
{
    public abstract class ScriptableObjectLoadOnStartupDataFromJson : ScriptableObject, ILoadOnStartup
    {
        [SerializeField] protected bool _allowDebug;
        
        private HashFileChangeDetector _hashFileChangeDetector;
        public bool isLoaded { get; private set; }
        
        // Hash file path
        protected abstract string dataFilePath { get; }
        // Json file path
        protected abstract string resourcePath { get; }
        
        // Generic method to parse JSON into any type
        protected T ParseJsonData<T>(string jsonContent)
        {
            return JsonUtility.FromJson<T>(jsonContent);
        }

        // Method to parse the JSON file, returning the number of elements (to be implemented by derived classes)
        protected abstract void ParseJsonFile(TextAsset jsonObject);

        protected abstract void InitializeData();

        protected abstract void LogCurrentData();

        public void LoadOnStartup()
        {
            // Initialize the HashFileChangeDetector if it hasn't been already
            // _hashFileChangeDetector ??= new HashFileChangeDetector(dataFilePath, _allowDebug);

            // var hasChanged = _hashFileChangeDetector.HasChanged();
            
            // Use the change detector to see if the JSON has changed
            // if (isLoaded && hasChanged == false)
            if (isLoaded)
            {
                if (_allowDebug) 
                    Debug.LogWarning($"[WARNING] {name} is already loaded, and the file has not changed.", this);
                LogCurrentData();
                
                return;
            }
            
            var jsonFile = Resources.Load<TextAsset>(resourcePath);

            if (!jsonFile)
            {
                if (_allowDebug) 
                    Debug.LogError($"[ERROR] JSON file not found at ../Assets/Resources/{resourcePath}.", this);
                
                return;
            }
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Loading {name} from ../Assets/Resources/{resourcePath}.", this);

            ParseJsonFile(jsonFile);
            
            InitializeData();
            
            if (_allowDebug) 
                Debug.Log($"[DEBUG] Initialized {name} data.", this);
            
            // _hashFileChangeDetector.UpdateState();
            
            Resources.UnloadAsset(jsonFile);

            LogCurrentData();
            
            if (_allowDebug) 
                Debug.Log($"Completed load of {name}.", this);
                
            isLoaded = true;
        }
        
        public event System.Action LoadError;
        public void ArrayError(string arrayName, string error, Object context = null)
        {
            Debug.LogError($"[ERROR] {arrayName} is {error}. Attempting to resolve.", context);
            
            LoadError?.Invoke();
        }
    }
}
