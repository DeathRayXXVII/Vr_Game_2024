using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using ZPTools.Interface;
using ZPTools.Utility;
using static ZPTools.DataType;
using static ZPTools.Utility.UtilityFunctions;


/// <summary>
/// </summary>
/// <remarks>
/// </remarks>
[CreateAssetMenu(fileName = "UpgradeData", menuName = "Data/UpgradeData")]
public class UpgradeData : ScriptableObject, ILoadOnStartup, IResetOnNewGame, INeedButton
{
    [SerializeField] private bool _allowDebug;
    
    private void OnEnable()
    {
        if (!isInitialized) hasChanged = false;
    }

    public void ResetToNewGameValues(int tier = 1)
    {
        if (tier < 1) return;
        SetUpgradeLevel(0);
    }

    /// <summary>
    /// </summary>
    [SerializeField] private int _upgradeLevel;
    public int upgradeLevel
    {
        get => _upgradeLevel;
        private set
        {
            var lastLevel = GetMaxUpgradeLevel();
            var previousLevel = _upgradeLevel;
            _upgradeLevel = lastLevel > 0 ? Mathf.Clamp(value, 0, lastLevel) : 0;
            UpdateData();
            if (previousLevel == _upgradeLevel) return;
            
            if (_allowDebug) 
                Debug.Log($"[INFO] {name} | Upgrade Level Changed from {previousLevel} to {upgradeLevel}");
            UpgradeLevelChanged();
        }
    }
    
    public delegate void UpgradeLevelChangeEvent(int level);
    public event UpgradeLevelChangeEvent UpgradeEvent;
    private void UpgradeLevelChanged() => UpgradeEvent?.Invoke(upgradeLevel);
    
    private int GetMaxUpgradeLevel()
    {
        var max = _upgradeList.GetLastIndex();
        return max < 0 ? 0 : max;
    }
    
    public void IncreaseUpgradeLevel() => upgradeLevel++;
    public void DecreaseUpgradeLevel() => upgradeLevel--;
    public void SetUpgradeLevel(int level) => upgradeLevel = level;
    
    public bool hasMaxLevelBeenReached => upgradeLevel >= GetMaxUpgradeLevel();
    [SerializeField] private BoolData _maxLevelData;
    
    /// <summary>
    /// </summary>
    
    [SerializeField] private EnumDataTypes _upgradeDataType;
    public EnumDataTypes upgradeDataType => _upgradeDataType;
    
    [SerializeField] private DualTypeList _upgradeList;
    
    [SerializeField] private FloatData _upgradeFloatContainer;
    [SerializeField] private IntData _upgradeIntContainer;
    
    [SerializeField] private float _baseUpgradeFloat;
    [SerializeField] private int _baseUpgradeInt;
    
    private void InitializeList(DualTypeList list, EnumDataTypes type)
    {
        if (list == null)
        {
            if (_allowDebug)
                Debug.LogWarning($"[WARNING] {name} | List is null. Creating new list of type: '{type}'", this);
            list = new DualTypeList(type);
        }
        else if (list.listType != type)
        {
            list.listType = type;
            list.Clear();
        }
        
        if (_allowDebug)
            Debug.Log($"[INFO] {name} | List of type: '{list.listType}' configured and contains\n{list}", this);
    }
    public object baseUpgradeValue => _upgradeDataType == EnumDataTypes.Float ? _baseUpgradeFloat : _baseUpgradeInt;
    public object upgradeValue
    { 
        get
        {
            if (!upgradeIsLoaded)
            {
                SetLoadState(_upgradeKey, false);
                if (_allowDebug)
                {
                    Debug.LogError(
                        _jsonFile == null
                            ? $"[ERROR] {name} | Cannot retrieve Upgrade Value due to JSON file is not assigned."
                            : $"[ERROR] {name} | Upgrade key '{_upgradeKey}' not found.\nPossible keys: \n   {string.Join(",\n   ", GetJsonKeys(jsonData))}",
                        this);
                }
                return null;
            }
            try
            {
                // Ensure correct type is retrieved from DualTypeList
                if (_upgradeDataType == EnumDataTypes.Float && _upgradeList.listType == EnumDataTypes.Float)
                {
                    return _upgradeList.GetValue<float>(upgradeLevel) + _baseUpgradeFloat;
                }

                if (_upgradeDataType == EnumDataTypes.Int && _upgradeList.listType == EnumDataTypes.Int)
                {
                    return _upgradeList.GetValue<int>(upgradeLevel) + _baseUpgradeInt;
                }
                throw new InvalidCastException(
                    $"[ERROR] {name} | The cost list type '{_upgradeList.listType}' does not match the expected data type '{_upgradeDataType}'.");
            }
            catch (Exception e)
            {
                if (_allowDebug) Debug.LogError($"[ERROR] {name} | Error getting upgrade value with...\nEnum type: [{_upgradeDataType}]\nAgainst List type: [{_upgradeList.listType}]\nError: {e}", this);
                return null;
            }
        }
    }
    
    /// <summary>
    /// </summary>
    [SerializeField] private EnumDataTypes _costDataType;
    public EnumDataTypes costDataType => _costDataType;
    
    [SerializeField] private DualTypeList _costList;
    
    [SerializeField] private FloatData _costFloatContainer;
    [SerializeField] private IntData _costIntContainer;
    
    [SerializeField] private bool _zeroBasedCostList;
    
    public object upgradeCost
    {
        get
        {
            if (!costIsLoaded)
            {
                SetLoadState(_costKey, false);
                if (_allowDebug)
                {
                    Debug.LogError(
                        _jsonFile == null
                            ? $"[ERROR] {name} | Cannot retrieve Upgrade Cost due to JSON file not being assigned."
                            : $"[ERROR] {name} | Upgrade key '{_costKey}' not found.\nPossible keys: \n   {string.Join(",\n   ", GetJsonKeys(jsonData))}",
                        this);
                }
                return null;
            }
            try
            {
                int index = !_zeroBasedCostList ? upgradeLevel + 1 <= GetMaxUpgradeLevel() ? upgradeLevel + 1 : upgradeLevel : upgradeLevel;

                // Ensure correct type is retrieved from DualTypeList
                if (_costDataType == EnumDataTypes.Float && _costList.listType == EnumDataTypes.Float)
                {
                    return _costList.GetValue<float>(index);
                }

                if (_costDataType == EnumDataTypes.Int && _costList.listType == EnumDataTypes.Int)
                {
                    return _costList.GetValue<int>(index);
                }

                throw new InvalidCastException(
                    $"The cost list type '{_costList.listType}' does not match the expected data type '{_costDataType}'.");
            }
            catch (Exception e)
            {
                if (_allowDebug)
                    Debug.LogError($"[ERROR] {name} | Error getting upgrade cost with...\nEnum type: [{_costDataType}]\nAgainst List type: [{_costList.listType}]\nError: {e}", this);
                return null;
            }
        }
    }
    
    /// <summary>
    /// </summary>
    [SerializeField] private string _upgradeKey = "";
    [SerializeField] private string _previousUpgradeKey;
    
    [SerializeField] private string _costKey = "";
    [SerializeField] private string _previousCostKey;
    
    private void SetPreviousKey(string key)
    {
        if (key == _upgradeKey) _previousUpgradeKey = _upgradeKey;
        if (key == _costKey) _previousCostKey = _costKey;
    }
    
    /// <c></c>
    /// <summary>
    /// </summary>
    [SerializeField, HideInInspector] private bool upgradeIsLoaded;
    [SerializeField, HideInInspector] private bool costIsLoaded;

    public bool isLoaded
    {
        get => upgradeIsLoaded && costIsLoaded;
        private set => upgradeIsLoaded = costIsLoaded = value;
    }
    
    private void SetLoadState(string key, bool state)
    {
        if (key == _upgradeKey) upgradeIsLoaded = state;
        if (key == _costKey) costIsLoaded = state;
    }
    
    /// <summary>
    /// </summary>
    private static void UpdateContainer(object container, object value)
    {
        if (container == null || value == null) return;

        switch (container)
        {
            case FloatData floatData when value is float floatValue:
                floatData.value = floatValue;
                break;
            case IntData intData when value is int intValue:
                intData.value = intValue;
                break;
            default:
                throw new ArgumentException("Unsupported container type or value type mismatch");
        }
    }

    private static void UpdateContainer(EnumDataTypes type, object value, FloatData floatData, IntData intData)
    {
        switch (type)
        {
            case EnumDataTypes.Float:
                UpdateContainer(floatData, value);
                break;
            case EnumDataTypes.Int:
                UpdateContainer(intData, value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void UpdateData()
    {
        if (_allowDebug)
            Debug.Log($"[INFO] {name} | Updating data...");
        UpdateContainer(_upgradeDataType, upgradeValue, _upgradeFloatContainer, _upgradeIntContainer);
        UpdateContainer(_costDataType, upgradeCost, _costFloatContainer, _costIntContainer);
        if (_maxLevelData != null) _maxLevelData.value = hasMaxLevelBeenReached;
        if (_allowDebug)
            Debug.Log($"[INFO] {name} |\nUpgrade Level: {upgradeLevel}\nUpgrade Value: {upgradeValue}\nUpgrade Cost: {upgradeCost}\n" +
                  $"Max level container: {_maxLevelData != null}\nMax Level Reached: {hasMaxLevelBeenReached}", this);
    }
    
    /// <summary>
    /// </summary>
    public void ForceJsonReload()
    {
        if (_allowDebug) 
            Debug.Log($"[INFO] {name} | Forcing JSON reload...", this);
        isLoaded = false; 
        _jsonData = null;
        _blobNeedsUpdate = true;
        // _jsonBlob = "";
        LoadOnStartup();
        if (_allowDebug) 
            Debug.Log($"[INFO] {name} | JSON Blob after forced reload: {_jsonBlob}", this);
    }
    
    [SerializeField] private TextAsset _jsonFile;
    private JObject _jsonData;

    public JObject jsonData
    {
        get
        {
            if (_jsonData != null) return _jsonData;
            
            if (_allowDebug) 
                Debug.LogWarning($"[WARNING] {name} | JSON Data is null. Attempting to parse JSON file...", this);

            string debugString; 
            
            if (_jsonFile == null)
            {
                if (_allowDebug) 
                    Debug.LogError($"[ERROR] {name} | JSON file not assigned. Attempting to load from cached data...", this);
                
                if (!string.IsNullOrEmpty(_jsonBlob))
                {
                    if (_allowDebug) 
                        Debug.Log($"[INFO] {name} | Cached JSON Blob is not empty. Parsing JSON data...", this);
                    
                    _jsonData = JObject.Parse(_jsonBlob);
                    debugString = "cached data";
                }
                else
                {
                    if (_allowDebug)
                        Debug.LogError($"[ERROR] {name} | JSON Blob cache is empty. Cannot parse JSON data.", this);

                    return null;
                }
            }
            else
            {
                _jsonData = JObject.Parse(_jsonFile.text);
                debugString = "JSON file";
            }
            
            if (_allowDebug) 
                Debug.Log($"[INFO] {name} | JSON Data parsed successfully from {debugString}:\n {_jsonData}", this);
            
            return _jsonData;
        }
    }
    
    private string GetJsonPath()
    {
        if (_jsonFile == null)
        {
            if (_allowDebug) 
                Debug.LogError($"[ERROR] {name} | JSON file not found when attempting to get path.", this);
            return null;
        }
#if UNITY_EDITOR
        return AssetDatabase.GetAssetPath(_jsonFile);
#else
        // return System.IO.Path.Combine(Application.streamingAssetsPath, _jsonFile.name + ".json");
        return null;
#endif
    }

    private void ParseJsonValues(string key, DualTypeList targetList, string listName = "")
    {
        if (_allowDebug) 
            Debug.Log($"[INFO] {name} | Parsing JSON Key: ['{key}']", this);
        
        if (!ValidateJsonKey(key, jsonData))
        {
            if (_allowDebug) 
                Debug.LogWarning($"[WARNING] {name} | Key '{key}' not found on {listName}.", this);
            
            targetList.Clear();
            SetLoadState(key, false);
            SetPreviousKey(key);
            return;
        }
        SetPreviousKey(key);
        
        JToken jsonSelection = jsonData[key];
        
        if (jsonSelection is JArray valuesArray)
        {
            _blobNeedsUpdate = true;
            targetList.Clear();
            try
            {
                targetList.AddRange(valuesArray);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ERROR] {name} | Error parsing JSON values for key '{key}' for list '{listName}': {e.Message}", this);
                throw;
            }
            
            SetLoadState(key, true);
            
            if (_allowDebug) 
                Debug.Log($"[INFO] {name} | Loaded JSON Key: ['{key}']\nContents: {valuesArray}", this);
        }
        else
        {
            SetLoadState(key, false);
            Debug.LogWarning(
                $"[WARNING] {name} | JSON {(key == _upgradeKey ? "UpgradeValue" : "UpgradeCost")} key '{key}' not found.\nPossible keys: \n   {string.Join(",\n   ", GetJsonKeys(jsonData))}",
                this);
        }
    }
    private void ParseUpgradeList() => ParseJsonValues(_upgradeKey, _upgradeList, "Upgrade List");
    private void ParseCostList() => ParseJsonValues(_costKey, _costList, "Cost List");
    
    /// <summary>
    /// </summary>
    [SerializeField] private string _jsonBlob;
    private bool _blobNeedsUpdate;

    private static string CreateJsonBlob(EnumDataTypes type, string jsonKey, List<object> values)
    {
        if (values.Count == 0 || string.IsNullOrEmpty(jsonKey)) return "";

        return type switch
        {
            EnumDataTypes.Float => JsonConvert.SerializeObject(new Dictionary<string, object> { { jsonKey, values } }, Formatting.Indented),
            EnumDataTypes.Int => JsonConvert.SerializeObject(new Dictionary<string, object> { { jsonKey, values } }, Formatting.Indented),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private string GetListBlob(string key, EnumDataTypes type, DualTypeList list)
    {
        if (_jsonFile == null || !ValidateJsonKey(key, jsonData))
        {
            SetLoadState(key, false);
            list.Clear();
            return "";
        }

        SetLoadState(key, true);
        return CreateJsonBlob(type, key, list);
    }

    private string upgradeListBlob => GetListBlob(_upgradeKey, _upgradeDataType, _upgradeList);
    private string costListBlob => GetListBlob(_costKey, _costDataType, _costList);

    private void UpdateJsonBlob()
    {
        var upgradeBlob = upgradeListBlob;
        var costBlob = costListBlob;
        
        if (string.IsNullOrEmpty(upgradeBlob) || string.IsNullOrEmpty(costBlob))
        {
            _jsonBlob = "";
            _blobNeedsUpdate = false;
            
            if (_allowDebug) 
                Debug.Log($"[INFO] {name} | Json Blob is empty.\nUpgrade Blob: {upgradeBlob}\nCost Blob: {costBlob}", this);
            return;
        }

        var upgradeJson = JObject.Parse(upgradeBlob);
        var costJson = JObject.Parse(costBlob);

        upgradeJson.Merge(costJson, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
        
        _jsonBlob = upgradeJson.ToString(Formatting.Indented);
        _blobNeedsUpdate = false;
        
        if (_allowDebug) 
            Debug.Log($"[INFO] {name} | Updated Json Blob: {_jsonBlob}", this);
    }
    
    private void AttemptParseFromJsonBlob()
    {
        if (string.IsNullOrEmpty(_jsonBlob)) return;
        
        ParseUpgradeList();
        ParseCostList();
    }
    
    /// <summary>
    /// </summary>
    private HashFileChangeDetector _hashFileChangeDetector;
    private void InitializeDataFromJson()
    {
        if (_jsonFile == null)
        {
            Debug.LogError($"[ERROR] {name} | JSON file not assigned.", this);
            return;
        }

        if (_allowDebug) 
            Debug.Log($"[INFO] {name} | JSON Size: {jsonData.Count}, Contents: {_jsonFile}", this);
        
        ParseUpgradeList();
        ParseCostList();
        
        _hashFileChangeDetector?.UpdateState();
        
        if (_allowDebug)
            Debug.Log(
                $"[INFO] {name} | Updating Json Blob: {_blobNeedsUpdate}\nupgradeList: {_upgradeList.ToString()}\ncostList: {_costList.ToString()}",
                this);
        
        if(_blobNeedsUpdate) UpdateJsonBlob();
    }
    
    public bool isInitialized { get; private set; }
    public bool hasChanged { get; private set; }
    public void LoadOnStartup()
    {
        isInitialized = false;
        if (_allowDebug) 
            Debug.Log($"[INFO] {name} | Initializing...", this);
        
        InitializeList(_upgradeList, _upgradeDataType);
        InitializeList(_costList, _costDataType);
        
        if (jsonData == null)
        {
            if (_allowDebug) 
                Debug.LogError($"[ERROR] {name} | Initialization failed due to JSON data missing.", this);
            
            isLoaded = false;
            isInitialized = true;
            
            return;
        }
        
        // var path = GetJsonPath();
        var path = "";
        if (!string.IsNullOrEmpty(path))
        {
            _hashFileChangeDetector ??= new HashFileChangeDetector(path, _allowDebug);
            hasChanged = _hashFileChangeDetector.HasChanged();
        }
        else
        {
            hasChanged = false;
        }
        
        if (_allowDebug) 
            Debug.Log($"[INFO] {name} |\nAlready Loaded: {isLoaded}\nChange State: {hasChanged}\n" +
                  $"Blob needs update: {_blobNeedsUpdate}\n" +
                  $"Blob is null or empty: {string.IsNullOrEmpty(_jsonBlob)}\n" +
                  $"Json Blob: {_jsonBlob}\n", this);
        
        if (!isLoaded && !hasChanged && !_blobNeedsUpdate)
        {
            if (_allowDebug) 
                Debug.Log($"[INFO] {name} | Attempting to parse from cached JSON Blob:\n{_jsonBlob}", this);
            
            AttemptParseFromJsonBlob();
        }
        else if (!isLoaded || hasChanged)
        {
            if (_allowDebug) 
                Debug.Log($"[INFO] {name} | Attempting to parse from JSON file at Path: {path}", this);
            
            InitializeDataFromJson();
        }
        else
        {
            if (_allowDebug) 
                Debug.LogWarning($"[WARNING] {name} | Upgrade data is already loaded.", this);
        }
        
        if (_blobNeedsUpdate) UpdateJsonBlob();
        
        UpdateData();
        
        if (_allowDebug) 
            Debug.Log($"[INFO] {name} | Completed initialization.", this);
        
        isInitialized = true;
    }

    /// <summary>
    /// </summary>
    public List<(Action, string)> GetButtonActions()
    {
        return new List<(Action, string)>
        {
#if UNITY_EDITOR
            (ForceJsonReload, "Force Update"),
            (IncreaseUpgradeLevel, "Increase Upgrade Level"),
            (DecreaseUpgradeLevel, "Decrease Upgrade Level"),
            (() =>
            {
                switch (isLoaded)
                {
                    case true when !_blobNeedsUpdate:
                        Debug.Log($"\nupgradeList: {_upgradeList.ToString()}\tcostList: {_costList.ToString()}", this);
                        break;
                    case false when _jsonFile == null:
                        Debug.LogWarning("Data not loaded due to JSON file not assigned.", this);
                        break;
                    case false:
                        switch (upgradeIsLoaded)
                        {
                            case false when !costIsLoaded:
                                Debug.LogWarning("Data not loaded due to missing keys.", this);
                                break;
                            case false:
                                Debug.LogWarning("Data not loaded due to missing upgrade key.", this);
                                break;
                            default:
                                Debug.LogWarning("Data not loaded due to missing cost key.", this);
                                break;
                        }
                        break;
                    default:
                        Debug.LogWarning("Data needs update.", this);
                        break;
                }
            }, "Output Upgrade and Cost Lists"),
            (() => Debug.Log($"\nUpgrade Level: {upgradeLevel}\tUpgrade Value: {upgradeValue}\tUpgrade Cost: {upgradeCost}", this),
                "Output Current Values"),
#endif
        };
    }
}