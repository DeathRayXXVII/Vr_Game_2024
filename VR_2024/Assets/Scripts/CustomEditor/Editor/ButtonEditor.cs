using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtonEditor : Editor
{
#if UNITY_EDITOR
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
#endif
}