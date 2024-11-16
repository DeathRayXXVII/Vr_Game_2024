using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static ZPTools.DataType;
using ZPTools.Interface;
using ZPTools.Utility;
using static ZPTools.Utility.UtilityFunctions;
using Debug = UnityEngine.Debug;


/// <summary>
/// </summary>
/// <remarks>
/// </remarks>
[CreateAssetMenu(fileName = "UpgradeData", menuName = "Data/UpgradeData")]
public class UpgradeData : ScriptableObject, ILoadOnStartup, INeedButton
{
    [SerializeField] private bool _allowDebug;

    private void OnEnable()
    {
        UpdateOrInitializeList(_upgradeList, _upgradeDataType);
        UpdateOrInitializeList(_costList, _costDataType);
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
            _upgradeLevel = lastLevel > 0 ? Mathf.Clamp(value, 0, lastLevel) : 0;
            UpdateData();
        }
    }
    private int GetMaxUpgradeLevel()
    {
        var max = _upgradeList.GetLastIndex();
        return max < 0 ? 0 : max;
    }
    
    public void IncreaseUpgradeLevel() => upgradeLevel++;
    public void DecreaseUpgradeLevel() => upgradeLevel--;
    public void SetUpgradeLevel(int level) => upgradeLevel = level;
    
    /// <summary>
    /// </summary>
    
    [SerializeField] private EnumDataTypes _upgradeDataType;
    public EnumDataTypes upgradeDataType => _upgradeDataType;
    
    [SerializeField] private DualTypeList _upgradeList;
    
    [SerializeField] private FloatData _upgradeFloatContainer;
    [SerializeField] private IntData _upgradeIntContainer;
    
    [SerializeField] private float _baseUpgradeFloat;
    [SerializeField] private int _baseUpgradeInt;
    
    private void UpdateOrInitializeList(DualTypeList list, EnumDataTypes type)
    {
        if (list == null)
        {
            list = new DualTypeList(type);
        }
        else if (list.listType != type)
        {
            // Update the list type to match the expected type.
            list.listType = type;
            list.Clear(); // Optionally clear list to avoid inconsistent data.
        }
    }
    public object baseUpgradeValue => _upgradeDataType == EnumDataTypes.Float ? _baseUpgradeFloat : _baseUpgradeInt;
    public object upgradeValue
    { 
        get
        {
            try
            {
                int index = upgradeLevel + 1 <= GetMaxUpgradeLevel() ? upgradeLevel + 1 : upgradeLevel;

                // Ensure correct type is retrieved from DualTypeList
                if (_upgradeDataType == EnumDataTypes.Float && _upgradeList.listType == EnumDataTypes.Float)
                {
                    return _upgradeList.GetValue<float>(index);
                }
                else if (_upgradeDataType == EnumDataTypes.Int && _upgradeList.listType == EnumDataTypes.Int)
                {
                    return _upgradeList.GetValue<int>(index);
                }
                else
                {
                    throw new System.InvalidCastException(
                        $"The cost list type '{_upgradeList.listType}' does not match the expected data type '{_upgradeDataType}'.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting upgrade cost with type: {_upgradeDataType}\nError: {e}", this);
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
    
    public object upgradeCost
    {
        get
        {
            try
            {
                int index = upgradeLevel + 1 <= GetMaxUpgradeLevel() ? upgradeLevel + 1 : upgradeLevel;

                // Ensure correct type is retrieved from DualTypeList
                if (_costDataType == EnumDataTypes.Float && _costList.listType == EnumDataTypes.Float)
                {
                    return _costList.GetValue<float>(index);
                }
                else if (_costDataType == EnumDataTypes.Int && _costList.listType == EnumDataTypes.Int)
                {
                    return _costList.GetValue<int>(index);
                }
                else
                {
                    throw new System.InvalidCastException(
                        $"The cost list type '{_costList.listType}' does not match the expected data type '{_costDataType}'.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting upgrade cost with type: {_costDataType}\nError: {e}", this);
                return null;
            }
        }
    }
    
    /// <summary>
    /// </summary>
    [SerializeField] private string _upgradeKey = "upgrade";
    [SerializeField] private string _previousUpgradeKey;
    
    [SerializeField] private string _costKey = "cost";
    [SerializeField] private string _previousCostKey;
    
    private string GetKey(string key)
    {
        if (key == _upgradeKey) return _upgradeKey;
        if (key == _costKey) return _costKey;
        throw new System.ArgumentException("Invalid key", nameof(key));
    }
    
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
        if (container == null) return;

        switch (container)
        {
            case FloatData floatData when value is float floatValue:
                floatData.value = floatValue;
                break;
            case IntData intData when value is int intValue:
                intData.value = intValue;
                break;
            default:
                throw new System.ArgumentException("Unsupported container type or value type mismatch");
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
                throw new System.ArgumentOutOfRangeException();
        }
    }
    
    private void UpdateData()
    {
        UpdateContainer(_upgradeDataType, upgradeValue, _upgradeFloatContainer, _upgradeIntContainer);
        UpdateContainer(_costDataType, upgradeCost, _costFloatContainer, _costIntContainer);
    }
    
    /// <summary>
    /// </summary>
    public void ForceJsonReload()
    {
        // _upgradeList = new DualTypeList(() => _upgradeDataType);
        // _costList = new DualTypeList(() => _costDataType);
        isLoaded = false;
        _blobNeedsUpdate = true;
        _jsonBlob = "";
        LoadOnStartup();
        if (_allowDebug) Debug.Log($"JSON Blob: {_jsonBlob}", this);
    }
    
    [SerializeField] private TextAsset _jsonFile;
    private JObject _jsonData;
    public JObject jsonData => _jsonData ??= JObject.Parse(_jsonFile.text);
    
    private string GetJsonPath()
    {
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.GetAssetPath(_jsonFile);
#else
        return System.IO.Path.Combine(Application.streamingAssetsPath, _jsonFile.name + ".json");
#endif
    }

    private void ParseJsonValues(string key, DualTypeList targetList)
    {
        var keyId = GetKey(key);
        SetPreviousKey(keyId);
        if (!ValidateJsonKey(keyId, jsonData))
        {
            if (_allowDebug) Debug.LogWarning($"Key '{keyId}' not found.", this);
            SetLoadState(keyId, false);
            return;
        }
        
        JToken jsonSelection = jsonData[keyId];
        
        if (jsonSelection is JArray valuesArray)
        {
            _blobNeedsUpdate = true;
            targetList.Clear();
            try
            {
                targetList.AddRange(valuesArray);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing JSON values for key '{keyId}': {e.Message}", this);
                throw;
            }
            
            SetLoadState(keyId, true);
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"Loaded JSON Key: ['{keyId}']\nContents: {valuesArray}", this);
#endif
        }
        else
        {
            SetLoadState(keyId, false);
            Debug.LogWarning(
                $"JSON {(key == _upgradeKey ? "UpgradeValue" : "UpgradeCost")} key '{keyId}' not found.\nPossible keys: \n   {string.Join(",\n   ", GetJsonKeys(jsonData))}",
                this);
        }
    }
    private void ParseUpgradeList() => ParseJsonValues(_upgradeKey, _upgradeList);
    private void ParseCostList() => ParseJsonValues(_costKey, _costList);
    
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
            _ => throw new System.ArgumentOutOfRangeException()
        };
    }
    
    private string GetJsonBlob(string key, EnumDataTypes type, DualTypeList list)
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

    private string upgradeJsonBlob => GetJsonBlob(_upgradeKey, _upgradeDataType, _upgradeList);
    private string costJsonBlob => GetJsonBlob(_costKey, _costDataType, _costList);

    private void UpdateJsonBlob()
    {
        if (string.IsNullOrEmpty(upgradeJsonBlob) || string.IsNullOrEmpty(costJsonBlob))
        {
            _jsonBlob = "";
            _blobNeedsUpdate = false;
            if (_allowDebug) Debug.Log($"Json Blob is empty.\nUpgrade Blob: {upgradeJsonBlob}\nCost Blob: {costJsonBlob}", this);
            return;
        }

        var upgradeJson = JObject.Parse(upgradeJsonBlob);
        var costJson = JObject.Parse(costJsonBlob);

        upgradeJson.Merge(costJson, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
        
        _jsonBlob = upgradeJson.ToString(Formatting.Indented);
        _blobNeedsUpdate = false;
        if (_allowDebug) Debug.Log($"Updated Json Blob: {_jsonBlob}", this);
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
            Debug.LogError("JSON file not assigned.", this);
            return;
        }

#if UNITY_EDITOR
        if (_allowDebug) Debug.Log($"JSON Size: {jsonData.Count}, Contents: {_jsonFile}", this);
#endif
        
        ParseUpgradeList();
        ParseCostList();
        
        _hashFileChangeDetector?.UpdateState();
#if UNITY_EDITOR
        if (_allowDebug)
            Debug.Log(
                $"Updating Json Blob: {_blobNeedsUpdate}\nupgradeList: {_upgradeList.ToString()}\ncostList: {_costList.ToString()}",
                this);
#endif
        if(_blobNeedsUpdate) UpdateJsonBlob();
    }
    
    public void LoadOnStartup()
    {
        UpdateOrInitializeList(_upgradeList, _upgradeDataType);
        UpdateOrInitializeList(_costList, _costDataType);
        
        if (!_jsonFile)
        {
            isLoaded = false;
            return;
        }
        
        _hashFileChangeDetector ??= new HashFileChangeDetector(GetJsonPath(), _allowDebug);
        var changeState = _hashFileChangeDetector.HasChanged();
       if (_allowDebug) Debug.Log($"Already Loaded: {isLoaded}\nChange State: {changeState}\n" +
                  $"Blob needs update: {_blobNeedsUpdate}\n" +
                  $"Blob is null or empty: {string.IsNullOrEmpty(_jsonBlob)}\n" +
                  $"Json Blob: {_jsonBlob}\n", this);
        
        if (!isLoaded && !changeState && !string.IsNullOrEmpty(_jsonBlob) && !_blobNeedsUpdate)
        {
            if (_allowDebug) Debug.Log($"JSON Blob: {_jsonBlob}", this);
            AttemptParseFromJsonBlob();
        }
        else if (!isLoaded || changeState)
        {
            if (_allowDebug) Debug.Log($"JSON Path: {GetJsonPath()} JSON: {_jsonFile}\n", this);
            InitializeDataFromJson();
        }
        else
        {
            if (_allowDebug) Debug.LogWarning("Data already loaded.", this);
        }
        
        if (_blobNeedsUpdate) UpdateJsonBlob();
        UpdateData();
    }

    /// <summary>
    /// </summary>
    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)>
        {
#if UNITY_EDITOR
            (IncreaseUpgradeLevel, "Increase Upgrade Level"),
            (DecreaseUpgradeLevel, "Decrease Upgrade Level"),
            (ForceJsonReload, "Force Update"),
            (() =>
            {
                switch (isLoaded)
                {
                    case true when !_blobNeedsUpdate:
                        Debug.Log($"upgradeList: {_upgradeList.ToString()}\ncostList: {_costList.ToString()}", this);
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
#endif
        };
    }
}