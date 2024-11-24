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
        UpdateOrInitializeList(_upgradeList, _upgradeDataType, _upgradeKey);
        UpdateOrInitializeList(_costList, _costDataType, _costKey);
        UpdateData();
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
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"Upgrade Level Changed from {previousLevel} to {upgradeLevel}");
#endif
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
    
    public bool maxLevelReached => upgradeLevel >= GetMaxUpgradeLevel();
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
    
    private void UpdateOrInitializeList(DualTypeList list, EnumDataTypes type, string key)
    {
        if (list == null)
        {
            list = new DualTypeList(type);
        }
        else if (list.listType != type)
        {
            list.listType = type;
            list.Clear();
        }
        
        if (_jsonFile == null || string.IsNullOrEmpty(key) || !ValidateJsonKey(key, jsonData))
        {
            SetLoadState(key, false);
            list.Clear();
            return;
        }
    }
    public object baseUpgradeValue => _upgradeDataType == EnumDataTypes.Float ? _baseUpgradeFloat : _baseUpgradeInt;
    public object upgradeValue
    { 
        get
        {
            if (!upgradeIsLoaded)
            {
                SetLoadState(_upgradeKey, false);
#if UNITY_EDITOR
                if (_allowDebug)
                {
                    Debug.LogError(
                        _jsonFile == null
                            ? "Cannot retrieve Upgrade Value due to JSON file is not assigned."
                            : $"Upgrade key '{_upgradeKey}' not found.\nPossible keys: \n   {string.Join(",\n   ", GetJsonKeys(jsonData))}",
                        this);
                }
#endif
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
                    $"The cost list type '{_upgradeList.listType}' does not match the expected data type '{_upgradeDataType}'.");
            }
#if UNITY_EDITOR
            catch (Exception e)
            {
                if (_allowDebug) Debug.LogError($"Error getting upgrade value with...\nEmum type: [{_upgradeDataType}]\nAgainst List type: [{_upgradeList.listType}]\nError: {e}", this);
                return null;
            }
#else
            catch (Exception)
            {
                return null;
            }
#endif
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
#if UNITY_EDITOR
                if (_allowDebug)
                {
                    Debug.LogError(
                        _jsonFile == null
                            ? "Cannot retrieve Upgrade Cost due to JSON file is not assigned."
                            : $"Upgrade key '{_costKey}' not found.\nPossible keys: \n   {string.Join(",\n   ", GetJsonKeys(jsonData))}",
                        this);
                }
#endif
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
#if UNITY_EDITOR
            catch (Exception e)
            {
                if (_allowDebug) Debug.LogError($"Error getting upgrade cost with...\nEmum type: [{_costDataType}]\nAgainst List type: [{_costList.listType}]\nError: {e}", this);
                return null;
            }
#else
            catch (Exception)
            {
                return null;
            }
#endif
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
        UpdateContainer(_upgradeDataType, upgradeValue, _upgradeFloatContainer, _upgradeIntContainer);
        UpdateContainer(_costDataType, upgradeCost, _costFloatContainer, _costIntContainer);
        if (_maxLevelData != null) _maxLevelData.value = maxLevelReached;
    }
    
    /// <summary>
    /// </summary>
    public void ForceJsonReload()
    {
        isLoaded = false;
        _jsonData = null;
        _blobNeedsUpdate = true;
        _jsonBlob = "";
        LoadOnStartup();
        if (_allowDebug) Debug.Log($"JSON Blob after forced reload: {_jsonBlob}", this);
    }
    
    [SerializeField] private TextAsset _jsonFile;
    private JObject _jsonData;
    public JObject jsonData => _jsonData ??= JObject.Parse(_jsonFile.text);
    
    private string GetJsonPath()
    {
#if UNITY_EDITOR
        return AssetDatabase.GetAssetPath(_jsonFile);
#else
        return System.IO.Path.Combine(Application.streamingAssetsPath, _jsonFile.name + ".json");
#endif
    }

    private void ParseJsonValues(string key, DualTypeList targetList)
    {
        if (!ValidateJsonKey(key, jsonData))
        {
            if (_allowDebug) Debug.LogWarning($"Key '{key}' not found.", this);
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
                Debug.LogError($"Error parsing JSON values for key '{key}': {e.Message}", this);
                throw;
            }
            
            SetLoadState(key, true);
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"Loaded JSON Key: ['{key}']\nContents: {valuesArray}", this);
#endif
        }
        else
        {
            SetLoadState(key, false);
            Debug.LogWarning(
                $"JSON {(key == _upgradeKey ? "UpgradeValue" : "UpgradeCost")} key '{key}' not found.\nPossible keys: \n   {string.Join(",\n   ", GetJsonKeys(jsonData))}",
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
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"Json Blob is empty.\nUpgrade Blob: {upgradeBlob}\nCost Blob: {costBlob}", this);
#endif
            return;
        }

        var upgradeJson = JObject.Parse(upgradeBlob);
        var costJson = JObject.Parse(costBlob);

        upgradeJson.Merge(costJson, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
        
        _jsonBlob = upgradeJson.ToString(Formatting.Indented);
        _blobNeedsUpdate = false;
#if UNITY_EDITOR
        if (_allowDebug) Debug.Log($"Updated Json Blob: {_jsonBlob}", this);
#endif
    }
    
    private void AttemptParseFromJsonBlob()
    {
        if (string.IsNullOrEmpty(_jsonBlob)) return;
        
        ParseUpgradeList();
        ParseCostList();
    }
    
    /// <summary>
    /// </summary>
    public HashFileChangeDetector HashFileChangeDetector;
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
        
        HashFileChangeDetector?.UpdateState();
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
        UpdateOrInitializeList(_upgradeList, _upgradeDataType, _upgradeKey);
        UpdateOrInitializeList(_costList, _costDataType, _costKey);
        
        if (!_jsonFile)
        {
            isLoaded = false;
            return;
        }
        
        HashFileChangeDetector ??= new HashFileChangeDetector(GetJsonPath(), _allowDebug);
        var changeState = HashFileChangeDetector.HasChanged();
#if UNITY_EDITOR
        if (_allowDebug) Debug.Log($"Already Loaded: {isLoaded}\nChange State: {changeState}\n" +
                  $"Blob needs update: {_blobNeedsUpdate}\n" +
                  $"Blob is null or empty: {string.IsNullOrEmpty(_jsonBlob)}\n" +
                  $"Json Blob: {_jsonBlob}\n", this);
#endif
        if (!isLoaded && !changeState && !_blobNeedsUpdate)
        {
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"JSON Blob: {_jsonBlob}", this);
#endif
            AttemptParseFromJsonBlob();
        }
        else if (!isLoaded || changeState)
        {
#if UNITY_EDITOR
            if (_allowDebug) Debug.Log($"JSON Path: {GetJsonPath()} JSON: {_jsonFile}\n", this);
#endif
            InitializeDataFromJson();
        }
#if UNITY_EDITOR
        else
        {
            if (_allowDebug) Debug.LogWarning("Data already loaded.", this);
        }
#endif
        
        if (_blobNeedsUpdate) UpdateJsonBlob();
        UpdateData();
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