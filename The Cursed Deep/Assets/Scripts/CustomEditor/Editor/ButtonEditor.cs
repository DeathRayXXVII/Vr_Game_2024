#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ZPTools.Interface;
 
[CustomEditor(typeof(MonoBehaviour), true)]
public class MonoButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (target is not INeedButton myScript) return;
        var actions = myScript.GetButtonActions();
        foreach (var action in actions)
        {
            if (GUILayout.Button(action.Item2))
            {
                action.Item1.Invoke();
            }
        }
    }
}

[CustomEditor(typeof(ScriptableObject), true)]
public class ScriptableObjButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (target is not INeedButton myScript) return;
        var actions = myScript.GetButtonActions();
        foreach (var action in actions)
        {
            if (GUILayout.Button(action.Item2))
            {
                action.Item1.Invoke();
            }
        }
    }
}
#endif