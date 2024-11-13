using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using ZPTools;
using ZPTools.Interface;
using ZPTools.Utility;
using Debug = UnityEngine.Debug;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Data/UpgradeData", order = 0)]
public class UpgradeData : ScriptableObject, ILoadOnStartup, INeedButton
{
    [SerializeField] private DataType _upgradeDataType;
    [SerializeField] private DataType _costDataType;

    [SerializeField] private bool _allowDebug;
    [SerializeField] private int _upgradeLevel;

    [SerializeField] private float _baseUpgradeFloat;
    [SerializeField] private int _baseUpgradeInt;
    [SerializeField] private FloatData _upgradeFloatContainer;
    [SerializeField] private IntData _upgradeIntContainer;
    [SerializeField] private List<float> _upgradeFloatList;
    [SerializeField] private List<int> _upgradeIntList;

    [SerializeField] private FloatData _costFloatContainer;
    [SerializeField] private IntData _costIntContainer;
    [SerializeField] private List<float> _costsFloatList;
    [SerializeField] private List<int> _costsIntList;

    [SerializeField] private TextAsset _jsonFile;
    [SerializeField] private string _upgradeKey = "upgrade";
    [SerializeField] private string _previousUpgradeKey;
    [SerializeField] private string _costKey = "cost";
    [SerializeField] private string _previousCostKey;
    
    [SerializeField] private string _jsonBlob;
    private bool _blobNeedsUpdate;

    private void UpdateData()
    {
        switch(_upgradeDataType)
        {
            case DataType.Float:
                if (_upgradeFloatContainer != null) _upgradeFloatContainer.value = upgradeValue is float floatCost ? floatCost : 0f;
                if (_upgradeFloatList?.Count == 0) upgradeIsLoaded = false;
                break;
            case DataType.Int:
                if (_upgradeIntContainer != null) _upgradeIntContainer.value = upgradeValue is int intCost ? intCost : 0;
                if (_upgradeIntList?.Count == 0) upgradeIsLoaded = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        switch(_costDataType)
        {
            case DataType.Float:
                if (_costFloatContainer != null) _costFloatContainer.value = upgradeCost is float floatCost ? floatCost : 0f;
                if (_costsFloatList.Count == 0) costIsLoaded = false;
                break;
            case DataType.Int:
                if (_costIntContainer != null) _costIntContainer.value = upgradeCost is int intCost ? intCost : 0;
                if (_costsIntList.Count == 0) costIsLoaded = false;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void PerformActionOnType<T>(DataType type, string key, Action<T> action)
    {
        var floatMap = new Dictionary<string, List<float>>
        {
            { _upgradeKey, _upgradeFloatList },
            { _costKey, _costsFloatList }
        };

        var intMap = new Dictionary<string, List<int>>
        {
            { _upgradeKey, _upgradeIntList },
            { _costKey, _costsIntList }
        };

        if (type == DataType.Float && floatMap.ContainsKey(key))
        {
            action?.Invoke((T)(object)floatMap[key]);
        }
        else if (type == DataType.Int && intMap.ContainsKey(key))
        {
            action?.Invoke((T)(object)intMap[key]);
        }
    }

    private void ClearList(DataType type, string key)
    {
        PerformActionOnType(type, key, (List<float> list) => list?.Clear());
        PerformActionOnType(type, key, (List<int> list) => list?.Clear());
    }

    private List<object> CastList(DataType type, string key)
    {
        List<object> result = new List<object>();
        PerformActionOnType(type, key, (List<float> list) => result = list.Cast<object>().ToList());
        PerformActionOnType(type, key, (List<int> list) => result = list.Cast<object>().ToList());
        return result;
    }
    
    private void AttemptParseFromJsonBlob()
    {
        if (string.IsNullOrEmpty(_jsonBlob)) return;
        
        var jsonData = JObject.Parse(_jsonBlob);
        ParseUpgradeList(jsonData);
        ParseCostList(jsonData);
        
        isLoaded = upgradeIsLoaded && costIsLoaded;
    }

    private static string CreateJsonBlob(DataType type, string jsonKey, List<object> values)
    {
        if (values.Count == 0 || string.IsNullOrEmpty(jsonKey)) return "";

        return type switch
        {
            DataType.Float => JsonConvert.SerializeObject(new Dictionary<string, object> { { jsonKey, values } }, Formatting.Indented),
            DataType.Int => JsonConvert.SerializeObject(new Dictionary<string, object> { { jsonKey, values } }, Formatting.Indented),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    private string upgradeJsonBlob 
    { 
        get
        {
            if (_jsonFile == null || !ValidateKey(_upgradeKey, JObject.Parse(_jsonFile.text)))
            {
                upgradeIsLoaded = false;
                ClearList(_upgradeDataType, _upgradeKey);
                return "";
            }
            upgradeIsLoaded = true;
            return CreateJsonBlob(_upgradeDataType, _upgradeKey, CastList(_upgradeDataType, _upgradeKey));
        }
    }
    
    private string costJsonBlob 
    { 
        get
        {
            if (_jsonFile == null || !ValidateKey(_costKey, JObject.Parse(_jsonFile.text)))
            {
                upgradeIsLoaded = false;
                ClearList(_costDataType, _costKey);
                return "";
            }
            upgradeIsLoaded = true;
            return CreateJsonBlob(_costDataType, _costKey, _costDataType == DataType.Float ? 
                _costsFloatList.Cast<object>().ToList() : _costsIntList.Cast<object>().ToList());
        }
    }

    private string UpdateJsonBlob()
    {
        if (string.IsNullOrEmpty(upgradeJsonBlob) || string.IsNullOrEmpty(costJsonBlob))
        {
            Debug.Log($"Json Blob is empty.\nUpgrade Blob: {upgradeJsonBlob}\nCost Blob: {costJsonBlob}", this);
            return "";
        }

        var upgradeJson = JObject.Parse(upgradeJsonBlob);
        var costJson = JObject.Parse(costJsonBlob);

        upgradeJson.Merge(costJson, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });
        
        _jsonBlob = upgradeJson.ToString(Formatting.Indented);
        Debug.Log($"Updated Json Blob: {_jsonBlob}", this);
        _blobNeedsUpdate = false;
        return _jsonBlob;
    }
    
    private HashFileChangeDetector _hashFileChangeDetector;
    public bool isLoaded { get; private set; }
    [SerializeField, HideInInspector] private bool upgradeIsLoaded;
    [SerializeField, HideInInspector] private bool costIsLoaded;

    public object upgradeValue
    { 
        get
        {
            try 
            {
               return (_upgradeDataType == DataType.Float)
                    ? (float)(object)baseValue + _upgradeFloatList[upgradeLevel]
                    : (int)(object)baseValue + _upgradeIntList[upgradeLevel];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public object upgradeCost
    {
        get
        {
            try
            {
                return upgradeLevel + 1 <= GetMaxUpgradeLevel() ? 
                    _costDataType == DataType.Float ? (float)(object)_costsFloatList[upgradeLevel + 1] : (int)(object)_costsIntList[upgradeLevel + 1] :
                    _costDataType == DataType.Float ? (float)(object)_costsFloatList[upgradeLevel] : (int)(object)_costsIntList[upgradeLevel];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public object baseValue => _upgradeDataType == DataType.Float ? _baseUpgradeFloat : _baseUpgradeInt;
    private int GetMaxUpgradeLevel() => _upgradeDataType == DataType.Float ? _upgradeFloatList.Count : _upgradeIntList.Count;

    public int upgradeLevel
    {
        get => _upgradeLevel;
        private set
        {
            _upgradeLevel = Mathf.Clamp(value, 0, GetMaxUpgradeLevel() - 1);
            UpdateData();
        }
    }
    
    public void LoadOnGUIChange(bool upgradeChanged = false, bool costChanged = false)
    {
        if (!_jsonFile || !upgradeChanged && !costChanged)
        {
            upgradeIsLoaded = costIsLoaded = isLoaded = false;
            return;
        }
        
        if (upgradeChanged && costChanged) LoadOnStartup(); 
        
        var jsonData = JObject.Parse(_jsonFile.text);
        if (upgradeChanged) ParseUpgradeList(jsonData);
        if (costChanged) ParseCostList(jsonData);
        if (_blobNeedsUpdate) UpdateJsonBlob();
        
        isLoaded = upgradeIsLoaded && costIsLoaded;
    }

    public void LoadOnStartup()
    {
        if (!_jsonFile)
        {
            upgradeIsLoaded = costIsLoaded = isLoaded = false;
            return;
        }
        
        _hashFileChangeDetector ??= new HashFileChangeDetector(GetJsonPath(), _allowDebug);
        var changeState = _hashFileChangeDetector.HasChanged();
        Debug.Log($"Already Loaded: {isLoaded}\nChange State: {changeState}\nBlob needs update: {_blobNeedsUpdate}\nBlob is null or empty: {string.IsNullOrEmpty(_jsonBlob)}\nJson Blob: {_jsonBlob}\n", this);
        
        if (!isLoaded && !string.IsNullOrEmpty(_jsonBlob) && !_blobNeedsUpdate && !changeState)
        {
            Debug.Log($"JSON Blob: {_jsonBlob}", this);
            AttemptParseFromJsonBlob();
        }
        else if (!isLoaded || changeState)
        {
            Debug.Log($"JSON Path: {GetJsonPath()} JSON: {_jsonFile}\n", this);
            InitializeDataFromJson();
        }
        else
        {
            Debug.LogWarning("Data already loaded.", this);
        }
        
        isLoaded = upgradeIsLoaded && costIsLoaded;
        if (_blobNeedsUpdate) UpdateJsonBlob();
        UpdateData();
    }
    
    private string GetJsonPath()
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.GetAssetPath(_jsonFile);
#else
        return System.IO.Path.Combine(Application.streamingAssetsPath, _jsonFile.name + ".json");
#endif
    }


    private void InitializeDataFromJson()
    {
        if (_jsonFile == null)
        {
            Debug.LogError("JSON file not assigned.", this);
            return;
        }

        var jsonData = JObject.Parse(_jsonFile.text);

#if UNITY_EDITOR
        if (_allowDebug) Debug.Log($"JSON Size: {jsonData.Count}, Contents: {_jsonFile}", this);
#endif
        
        ParseUpgradeList(jsonData);
        ParseCostList(jsonData);
        
        _hashFileChangeDetector?.UpdateState();
#if UNITY_EDITOR
        if (_allowDebug)
            Debug.Log(
                $"Updating Json Blob: {_blobNeedsUpdate}\nupgradeList: {string.Join(", ", _upgradeFloatList)}\ncostList: {string.Join(", ", _costsFloatList)}",
                this);
#endif
        if(_blobNeedsUpdate) UpdateJsonBlob();
    }
    
    private void ParseUpgradeList(JObject jsonData)
    {
        if (_upgradeDataType == DataType.Float)
            ParseJsonValues(jsonData, _upgradeKey, ref _upgradeFloatList);
        else
            ParseJsonValues(jsonData, _upgradeKey, ref _upgradeIntList);
    }

    private void ParseCostList(JObject jsonData)
    {
        if (_costDataType == DataType.Float)
            ParseJsonValues(jsonData, _costKey, ref _costsFloatList);
        else
            ParseJsonValues(jsonData, _costKey, ref _costsIntList);
    }
    
    private bool ValidateKey(string key, JObject data) => data.Properties().Any(property => property.Name == key);
    
    private ref string GetKey(string key)
    {
        if (key == _upgradeKey) return ref _upgradeKey;
        if (key == _costKey) return ref _costKey;
        throw new ArgumentException("Invalid key", nameof(key));
    }
    
    private void SetPreviousKey(string key)
    {
        if (key == _upgradeKey) _previousUpgradeKey = _upgradeKey;
        if (key == _costKey) _previousCostKey = _costKey;
    }
    
    private void SetLoadState(string key, bool state)
    {
        if (key == _upgradeKey) upgradeIsLoaded = state;
        if (key == _costKey) costIsLoaded = state;
    }
    
    private void ParseJsonValues<T>(JObject data, string key, ref List<T> targetList)
    {
        ref var keyId = ref GetKey(key);
        if (!ValidateKey(keyId, data))
        {
            Debug.LogWarning($"Key '{key}' not found.", this);
            SetLoadState(keyId, false);
            return;
        }
        
        JToken jsonSelection = data[keyId];
        
        SetPreviousKey(key);
        
        if (jsonSelection is JArray valuesArray)
        {
            _blobNeedsUpdate = true;
            targetList.Clear();
            foreach (var value in valuesArray)
            {
                targetList.Add((T)Convert.ChangeType(value, typeof(T)));
            }
            
            SetLoadState(keyId, true);
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"Loaded JSON Key: ['{key}']\nContents: {string.Join(", ", targetList)}", this);
#endif
        }
        else
        {
            SetLoadState(keyId, false);
            Debug.LogWarning(
                $"JSON {(key == _upgradeKey ? "Value" : "Cost")} key '{key}' not found.\nPossible keys: \n   {string.Join(",\n   ", data.Properties().Select(property => property.Name))}",
                this);
        }
    }

    private void ForceUpdate()
    {
        isLoaded = false;
        _blobNeedsUpdate = true;
        _jsonBlob = "";
        LoadOnStartup();
        Debug.Log($"JSON Blob: {_jsonBlob}", this);
    }

    public List<(Action, string)> GetButtonActions()
    {
        return new List<(Action, string)>
        {
            (() => upgradeLevel++, "Increase Upgrade Level"),
            (() => upgradeLevel--, "Decrease Upgrade Level"),
            (ForceUpdate, "Force Update"),
            (() => Debug.Log($"upgradeList: {string.Join(", ", _upgradeFloatList)}\ncostList: {string.Join(", ", _costsFloatList)}", this), "Output Upgrade and Cost Lists"),
        };
    }
}