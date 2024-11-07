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
    [SerializeField] private string _upgradeKey = "values";
    [SerializeField] private string _previousUpgradeKey;
    [SerializeField] private string _costKey = "costs";
    [SerializeField] private string _previousCostKey;
    
    [SerializeField] private string _jsonBlob;
    private bool _blobNeedsUpdate;

    // private void OnEnable()
    // {
    //     UpdateData();
    //     
    //     isLoaded = _valueIsLoaded && _costIsLoaded;
    //     
    //     if (!_jsonFile)
    //     {
    //         _valueIsLoaded = _costIsLoaded = isLoaded = false;
    //         return;
    //     }
    //     _jsonBlob ??= UpdateJsonBlob();
    //     if (!isLoaded) LoadOnStartup();
    // }

    private void UpdateData()
    {
        switch(_upgradeDataType)
        {
            case DataType.Float:
                if (_upgradeFloatContainer != null) _upgradeFloatContainer.value = upgradeValue is float floatCost ? floatCost : 0f;
                if (_upgradeFloatList?.Count == 0) _valueIsLoaded = false;
                // Debug.Log($"Value: {upgradeValue}");
                break;
            case DataType.Int:
                if (_upgradeIntContainer != null) _upgradeIntContainer.value = upgradeValue is int intCost ? intCost : 0;
                if (_upgradeIntList?.Count == 0) _valueIsLoaded = false;
                // Debug.Log($"Value: {upgradeValue}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        switch(_costDataType)
        {
            case DataType.Float:
                if (_costFloatContainer != null) _costFloatContainer.value = upgradeCost is float floatCost ? floatCost : 0f;
                if (_costsFloatList.Count == 0) _costIsLoaded = false;
                // Debug.Log($"Cost: {upgradeCost}");
                break;
            case DataType.Int:
                if (_costIntContainer != null) _costIntContainer.value = upgradeCost is int intCost ? intCost : 0;
                if (_costsIntList.Count == 0) _costIsLoaded = false;
                // Debug.Log($"Cost: {upgradeCost}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void AttemptParseFromJsonBlob()
    {
        if (string.IsNullOrEmpty(_jsonBlob)) return;
        
        var jsonData = JObject.Parse(_jsonBlob);
        ParseUpgradeList(jsonData);
        ParseCostList(jsonData);
        
        isLoaded = _valueIsLoaded && _costIsLoaded;
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
    
    private string upgradeJsonBlob => _jsonFile == null ? "" : 
        CreateJsonBlob(_upgradeDataType, _upgradeKey, _upgradeDataType == DataType.Float ? 
            _upgradeFloatList.Cast<object>().ToList() : _upgradeIntList.Cast<object>().ToList());
    
    private string costJsonBlob => _jsonFile == null ? "" : 
        CreateJsonBlob(_costDataType, _costKey, _costDataType == DataType.Float ? 
            _costsFloatList.Cast<object>().ToList() : _costsIntList.Cast<object>().ToList());

    private string UpdateJsonBlob()
    {
        if (string.IsNullOrEmpty(upgradeJsonBlob) && string.IsNullOrEmpty(costJsonBlob))
            return "";

        var upgradeJson = JObject.Parse(upgradeJsonBlob);
        var costJson = JObject.Parse(costJsonBlob);

        upgradeJson.Merge(costJson, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Union
        });
        
        _jsonBlob = upgradeJson.ToString(Formatting.Indented);
        _blobNeedsUpdate = false;
        return _jsonBlob;
    }
    
    private HashFileChangeDetector _hashFileChangeDetector;
    public bool isLoaded { get; private set; }
    private bool _valueIsLoaded;
    private bool _costIsLoaded;

    public object upgradeValue
    { 
        get
        {
            try 
            {
                // Debug.Log($"Upgrade Level: {upgradeLevel}\nMax Upgrade Level: {GetMaxUpgradeLevel()}\n" +
                //           $"Base Value: {baseValue}\n" +
                //           $"Upgrade Value: {(_upgradeDataType == DataType.Float ? _upgradeFloatList[upgradeLevel] : _upgradeIntList[upgradeLevel])}\n" +
                //           $"Data Type: {_upgradeDataType}");
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
                    (_costDataType == DataType.Float ? (float)(object)_costsFloatList[upgradeLevel + 1] : (int)(object)_costsIntList[upgradeLevel + 1]) :
                    (_costDataType == DataType.Float ? (float)(object)_costsFloatList[upgradeLevel] : (int)(object)_costsIntList[upgradeLevel]);
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
            _valueIsLoaded = _costIsLoaded = isLoaded = false;
            return;
        }
        
        if (upgradeChanged && costChanged) LoadOnStartup(); 
        
        var jsonData = JObject.Parse(_jsonFile.text);
        if (upgradeChanged) ParseUpgradeList(jsonData);
        if (costChanged) ParseCostList(jsonData);
        if (_blobNeedsUpdate) UpdateJsonBlob();
        
        isLoaded = _valueIsLoaded && _costIsLoaded;
    }

    public void LoadOnStartup()
    {
        if (!_jsonFile)
        {
            _valueIsLoaded = _costIsLoaded = isLoaded = false;
            return;
        }
        
        _hashFileChangeDetector ??= new HashFileChangeDetector(GetJsonPath(), _allowDebug);
        var changeState = _hashFileChangeDetector.HasChanged();
        Debug.Log($"JSON Path: {GetJsonPath()} JSON: {_jsonFile}\n", this);
        if (!isLoaded && !string.IsNullOrEmpty(_jsonBlob) && !_blobNeedsUpdate && !changeState) AttemptParseFromJsonBlob();
        if (!isLoaded || changeState)
            InitializeDataFromJson();
        else Debug.LogWarning("Data already loaded.", this);
        
        isLoaded = _valueIsLoaded && _costIsLoaded;
        if (_blobNeedsUpdate) UpdateJsonBlob();
        UpdateData();
    }

    private string GetJsonPath() => UnityEditor.AssetDatabase.GetAssetPath(_jsonFile);

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
                $"upgradeList: {string.Join(", ", _upgradeFloatList)}\ncostList: {string.Join(", ", _costsFloatList)}",
                this);
#endif
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
    
    private void ParseJsonValues<T>(JObject data, string key, ref List<T> targetList)
    {
        string keyId = key == _upgradeKey ? _upgradeKey : key == _costKey ? _costKey : null;
        if (keyId == null)
        {
            Debug.LogWarning($"Key '{key}' not found.", this);
            return;
        }
        
        JToken jsonSelection = data[keyId];
        
        _previousUpgradeKey = key == _upgradeKey ? key : _previousUpgradeKey;
        _previousCostKey = key == _costKey ? key : _previousCostKey;

        if (jsonSelection is JArray valuesArray)
        {
            _blobNeedsUpdate = true;
            targetList.Clear();
            foreach (var value in valuesArray)
            {
                targetList.Add((T)Convert.ChangeType(value, typeof(T)));
            }
            _valueIsLoaded = key == _upgradeKey || _valueIsLoaded;
            _costIsLoaded = key == _costKey || _costIsLoaded;
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"Loaded JSON Key: ['{key}']\nContents: {string.Join(", ", targetList)}", this);
#endif
        }
        else
        {
            if (key == _upgradeKey) _valueIsLoaded = false;
            if (key == _costKey) _costIsLoaded = false;
            Debug.LogWarning(
                $"JSON {(key == _upgradeKey ? "Value" : "Cost")} key '{key}' not found.\nPossible keys: \n   {string.Join(",\n   ", data.Properties().Select(property => property.Name))}",
                this);
        }
    }

    private void ForceUpdate()
    {
        _blobNeedsUpdate = true;
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