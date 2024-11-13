using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using ZPTools.Interface;

[System.Serializable]
public class TriggeredActionsListWrapper
{
    public List<DebugInputAction.TriggeredActions> list = new List<DebugInputAction.TriggeredActions>();
}

public class DebugInputAction : MonoBehaviour, INeedButton
{
    [System.Serializable]
    public struct TriggeredActions
    {
#if UNITY_EDITOR
        [HideInInspector] public string actionName;
        public bool excludeFromDebug;

        public TriggeredActions(string name, bool exclude)
        {
            actionName = name;
            excludeFromDebug = exclude;
        }
#endif
    }
    
#if UNITY_EDITOR
    [SerializeField] private bool allowDebug = true;
#endif
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private List<TriggeredActions> _triggeredActions = new List<TriggeredActions>();

#if UNITY_EDITOR
    private bool _isInitialized;
    
    public void Update()
    {
        if (!_isInitialized && inputActionAsset != null) _isInitialized = true;
        if (!_isInitialized) return;

        foreach (var action in inputActionAsset)
        {
            if (!action.triggered) continue;

            var existingAction = _triggeredActions.FirstOrDefault(element => element.actionName == action.ToString());
            UnityEditor.EditorUtility.SetDirty(this);
            if (!string.IsNullOrEmpty(existingAction.actionName))
            {
                // If the action exists and should not be excluded, log it
                if (!existingAction.excludeFromDebug && allowDebug)
                {
                    Debug.Log($"Input triggered: {action.name}", this);
                }
            }
            else
            {
                // If the action does not exist in the list, add it and log it
                _triggeredActions.Add(new TriggeredActions(action.ToString(), true));
                if (allowDebug) Debug.Log($"Input triggered: {action}", this);
            }
        }
    }
    
    private const string _editorPrefsKey = "TriggeredActions";
    private void SaveData()
    {
        var wrapper = new TriggeredActionsListWrapper { list = _triggeredActions };
        UnityEditor.EditorPrefs.SetString(_editorPrefsKey, JsonUtility.ToJson(wrapper));
    }

    private void LoadData()
    {
        if (!UnityEditor.EditorPrefs.HasKey(_editorPrefsKey)) return;
        var wrapper = JsonUtility.FromJson<TriggeredActionsListWrapper>(UnityEditor.EditorPrefs.GetString(_editorPrefsKey));
        _triggeredActions = wrapper?.list ?? new List<TriggeredActions>();
    }
    
    private bool _dataLoaded;
    private void OnValidate()
    {
        if (!_dataLoaded)
        {
            LoadData();
            _dataLoaded = true; // Ensure data is loaded only once when the component is validated initially.
        }
        else
        {
            // Allow changes in the Inspector to be saved only after a manual change is made.
            SaveData();
        }
    }
    private void OnEnable() => LoadData();
    private void OnDisable() => SaveData();
    private void OnApplicationQuit() => SaveData();

    private void ClearList(ref List<TriggeredActions> targetList)
    {
        targetList?.Clear();
        SaveData();
    }

    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)>
        {
            (() => ClearList(ref _triggeredActions), "Clear Triggered Actions")
        };
    }
#endif
}
