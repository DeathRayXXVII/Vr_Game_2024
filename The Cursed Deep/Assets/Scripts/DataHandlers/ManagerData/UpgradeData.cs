using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine.PlayerLoop;
using ZPTools.Interface;
using ZPTools.Utility;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Data/UpgradeData", order = 0)]
public class UpgradeData : ScriptableObject, ILoadOnStartup, INeedButton
{
    public enum DataType
    {
        Float,
        Int
    }

    [SerializeField] private DataType _upgradeType;
    [SerializeField] private DataType _costType;

    [SerializeField] private bool _allowDebug;
    [SerializeField] private int _upgradeLevel;

    [SerializeField] private float _baseValueFloat;
    [SerializeField] private int _baseValueInt;
    [SerializeField] private FloatData _valueFloat;
    [SerializeField] private IntData _valueInt;
    [SerializeField] private List<float> _upgradeFloatValues;
    [SerializeField] private List<int> _upgradeIntValues;

    [SerializeField] private FloatData _costFloat;
    [SerializeField] private IntData _costInt;
    [SerializeField] private List<float> _upgradeFloatCosts;
    [SerializeField] private List<int> _upgradeIntCosts;

    [SerializeField] private TextAsset _jsonFile;
    [SerializeField] private string _valueKey = "values";
    [SerializeField] private string _previousValueKey;
    [SerializeField] private string _costKey = "costs";
    [SerializeField] private string _previousCostKey;
    
    [SerializeField] private string _jsonBlob;
    private bool _blobNeedsUpdate;

    private void OnEnable()
    {
        UpdateData();
        
        isLoaded = _valueIsLoaded && _costIsLoaded;
        
        if (!_jsonFile)
        {
            _valueIsLoaded = _costIsLoaded = isLoaded = false;
            return;
        }
        _jsonBlob ??= UpdateJsonBlob();
        if (!isLoaded) LoadOnStartup();
    }

    private void UpdateData()
    {
        switch(_upgradeType)
        {
            case DataType.Float:
                if (_valueFloat != null) _valueFloat.value = upgradeValue == null ? 0f : (float)upgradeValue;
                if (_upgradeFloatValues.Count == 0) _valueIsLoaded = false;
                // Debug.Log($"Value: {upgradeValue}");
                break;
            case DataType.Int:
                if (_valueInt != null) _valueInt.value = upgradeValue == null ? 0 : (int)upgradeValue;
                if (_upgradeIntValues.Count == 0) _valueIsLoaded = false;
                // Debug.Log($"Value: {upgradeValue}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        switch(_costType)
        {
            case DataType.Float:
                if (_costFloat != null) _costFloat.value = upgradeCost == null ? 0f : (float)upgradeCost;
                if (_upgradeFloatCosts.Count == 0) _costIsLoaded = false;
                // Debug.Log($"Cost: {upgradeCost}");
                break;
            case DataType.Int:
                if (_costInt != null) _costInt.value = upgradeCost == null ? 0 : (int)upgradeCost;
                if (_upgradeIntCosts.Count == 0) _costIsLoaded = false;
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
        CreateJsonBlob(_upgradeType, _valueKey, _upgradeType == DataType.Float ? 
            _upgradeFloatValues.Cast<object>().ToList() : _upgradeIntValues.Cast<object>().ToList());
    
    private string costJsonBlob => _jsonFile == null ? "" : 
        CreateJsonBlob(_costType, _costKey, _costType == DataType.Float ? 
            _upgradeFloatCosts.Cast<object>().ToList() : _upgradeIntCosts.Cast<object>().ToList());

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
                //           $"Upgrade Value: {(_upgradeType == DataType.Float ? _upgradeFloatValues[upgradeLevel] : _upgradeIntValues[upgradeLevel])}\n" +
                //           $"Data Type: {_upgradeType}");
               return (_upgradeType == DataType.Float)
                    ? (float)baseValue + _upgradeFloatValues[upgradeLevel]
                    : (int)baseValue + _upgradeIntValues[upgradeLevel];
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
                return upgradeLevel+1 <= GetMaxUpgradeLevel() ? 
                    _costType == DataType.Float ? _upgradeFloatCosts[upgradeLevel+1] : _upgradeIntCosts[upgradeLevel+1] :
                    _costType == DataType.Float ? _upgradeFloatCosts[upgradeLevel] : _upgradeIntCosts[upgradeLevel];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }  
    
    public object baseValue => _upgradeType == DataType.Float ? _baseValueFloat : _baseValueInt;
    private int GetMaxUpgradeLevel() => _upgradeType == DataType.Float ? _upgradeFloatValues.Count : _upgradeIntValues.Count;

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
        if (!isLoaded && !string.IsNullOrEmpty(_jsonBlob)) AttemptParseFromJsonBlob();
        else if (!isLoaded || _hashFileChangeDetector.HasChanged())
            InitializeDataFromJson();
        
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
        
        _hashFileChangeDetector.UpdateState();
#if UNITY_EDITOR
        if (_allowDebug)
            Debug.Log(
                $"upgradeList: {string.Join(", ", _upgradeFloatValues)}\ncostList: {string.Join(", ", _upgradeFloatCosts)}",
                this);
#endif
    }
    
    private void ParseUpgradeList(JObject jsonData)
    {
        if (_upgradeType == DataType.Float)
            ParseJsonValues(jsonData, _valueKey, ref _upgradeFloatValues);
        else
            ParseJsonValues(jsonData, _valueKey, ref _upgradeIntValues);
    }

    private void ParseCostList(JObject jsonData)
    {
        if (_costType == DataType.Float)
            ParseJsonValues(jsonData, _costKey, ref _upgradeFloatCosts);
        else
            ParseJsonValues(jsonData, _costKey, ref _upgradeIntCosts);
    }
    
    private void ParseJsonValues<T>(JObject data, string key, ref List<T> targetList)
    {
        string keyId = key == _valueKey ? _valueKey : key == _costKey ? _costKey : null;
        if (keyId == null)
        {
            Debug.LogWarning($"Key '{key}' not found.", this);
            return;
        }
        
        JToken jsonSelection = data[keyId];
        
        _previousValueKey = key == _valueKey ? key : _previousValueKey;
        _previousCostKey = key == _costKey ? key : _previousCostKey;

        if (jsonSelection is JArray valuesArray)
        {
            targetList.Clear();
            foreach (var value in valuesArray)
            {
                targetList.Add((T)Convert.ChangeType(value, typeof(T)));
            }
            _valueIsLoaded = key == _valueKey || _valueIsLoaded;
            _costIsLoaded = key == _costKey || _costIsLoaded;
            _blobNeedsUpdate = true;
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"Loaded JSON Key: ['{key}']\nContents: {string.Join(", ", targetList)}", this);
#endif
        }
        else
        {
            if (key == _valueKey) _valueIsLoaded = false;
            if (key == _costKey) _costIsLoaded = false;
            Debug.LogWarning(
                $"JSON {(key == _valueKey ? "Value" : "Cost")} key '{key}' not found.\nPossible keys: \n   {string.Join(",\n   ", data.Properties().Select(property => property.Name))}",
                this);
        }
    }

    public List<(Action, string)> GetButtonActions()
    {
        return new List<(Action, string)>
        {
            (() => upgradeLevel++, "Increase Upgrade Level"),
            (() => upgradeLevel--, "Decrease Upgrade Level")
        };
    }
}