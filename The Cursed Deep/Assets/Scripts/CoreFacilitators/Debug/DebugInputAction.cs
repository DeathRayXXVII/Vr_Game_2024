using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using ZPTools.Interface;

[System.Serializable]
public class StringListWrapper
{
    public List<string> list = new List<string>();
}

public class DebugInputAction : MonoBehaviour, INeedButton
{
#if UNITY_EDITOR
    [SerializeField] private bool allowDebug = true;
    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private List<string> exclusionList;
    [SerializeField, InspectorReadOnly] private List<string> triggeredActions = new List<string>();

    private bool _isInitialized;

    public void Update()
    {
        if (!allowDebug) return;
        if (!_isInitialized && inputActionAsset != null) _isInitialized = true;
        if (!_isInitialized) return;
        foreach (var action in inputActionAsset)
        {
            if (!action.triggered || exclusionList.Contains(action.name)) continue;
            if (action.triggered && !triggeredActions.Contains(action.name))
            {
                triggeredActions.Add(action.name);
                UnityEditor.EditorUtility.SetDirty(this);
            }
            if (!triggeredActions.Contains(action.name))
            {
                Debug.Log($"Input triggered: {action.name}");
            }
        }
    }

    private void LoadData()
    {
        if (UnityEditor.EditorPrefs.HasKey("TriggeredActions"))
        {
            string json = UnityEditor.EditorPrefs.GetString("TriggeredActions");
            triggeredActions = JsonUtility.FromJson<StringListWrapper>(json)?.list ?? new List<string>();
        }
        if (UnityEditor.EditorPrefs.HasKey("ExclusionList"))
        {
            string json = UnityEditor.EditorPrefs.GetString("ExclusionList");
            exclusionList = JsonUtility.FromJson<StringListWrapper>(json)?.list ?? new List<string>();
        }
    }

    private void SaveData()
    {
        StringListWrapper triggeredWrapper = new StringListWrapper { list = triggeredActions };
        StringListWrapper exclusionWrapper = new StringListWrapper { list = exclusionList };

        UnityEditor.EditorPrefs.SetString("TriggeredActions", JsonUtility.ToJson(triggeredWrapper));
        UnityEditor.EditorPrefs.SetString("ExclusionList", JsonUtility.ToJson(exclusionWrapper));
    }

    private void OnValidate() => LoadData();
    private void OnEnable() => LoadData();
    private void OnDisable() => SaveData();
    private void OnApplicationQuit() => SaveData();

    private void ExcludeTriggeredActions()
    {
        if (triggeredActions.Count == 0) return;
        foreach (var action in triggeredActions.Where(action => !exclusionList.Contains(action)))
        {
            exclusionList.Add(action);
        }
    }

    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)>
        {
            (ExcludeTriggeredActions, "Exclude Triggered Actions"),
            (() => exclusionList.Clear(), "Clear Exclusions"),
            (() => triggeredActions.Clear(), "Clear Triggered Actions"),
        };
    }
#endif
}
