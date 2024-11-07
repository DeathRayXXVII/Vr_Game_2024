using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrefabDataList))]
public class PrefabDataListEditor : Editor
{
    public GameObject prefabToAdd;
    public int priority;
    private bool _showPrefabList;

    private void OnEnable()
    {
        _showPrefabList = SessionState.GetBool("showPrefabList", false);
    }

    public override void OnInspectorGUI()
    {
        SerializedProperty scriptProp = serializedObject.FindProperty("m_Script");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProp);
        EditorGUI.EndDisabledGroup();
        
        PrefabDataList prefabList = (PrefabDataList)target;

        prefabToAdd = (GameObject)EditorGUILayout.ObjectField("Prefab To Add:", prefabToAdd, typeof(GameObject), false);
        priority = EditorGUILayout.IntField("Priority:", priority);

        if (GUILayout.Button("Add Prefab"))
        {
            if (prefabToAdd != null)
            {
                var newPrefabData = CreateInstance<PrefabData>();

                newPrefabData.prefab = prefabToAdd;
                newPrefabData.priority = priority;

                newPrefabData.name =$"[{prefabList.prefabDataList.Count}] {prefabToAdd.name}";

                prefabList.prefabDataList.Add(newPrefabData);

                AssetDatabase.AddObjectToAsset(newPrefabData, prefabList);
                AssetDatabase.SaveAssets();
            }
            else
            {
                Debug.Log("No prefab selected");
            }
        }

        if (GUILayout.Button("Clear List"))
        {
            prefabList.prefabDataList.Clear();
            AssetDatabase.SaveAssets();
        }

        if (prefabList.prefabDataList.Count == 0) return;

        EditorGUILayout.Space();

        _showPrefabList = EditorGUILayout.Foldout(_showPrefabList, "Prefab Data List Items:");
        SessionState.SetBool("showPrefabList", _showPrefabList);

        if (_showPrefabList)
        {
            int counter = 0;
            PrefabData elementToRemove = null;

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginVertical(GUI.skin.box); // Begin a vertical group with a box around it

            foreach (PrefabData prefabData in prefabList.prefabDataList)
            {
                if (prefabData == null)
                {
                    elementToRemove = prefabData;
                    continue;
                }

                EditorGUILayout.BeginVertical(GUI.skin.box); // Box for each element

                EditorGUILayout.BeginHorizontal();
                var spacer = GUILayout.Width(265);
                EditorGUILayout.LabelField("Element [" + counter + "]", spacer);
                EditorGUILayout.LabelField("Priority: " + prefabData.priority, spacer);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(prefabData.prefab.name);
                if (GUILayout.Button("Remove"))
                {
                    elementToRemove = prefabData;
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical(); // End box for each element

                counter++;
            }

            EditorGUILayout.EndVertical(); // End of vertical group

            EditorGUI.indentLevel--;

            if (elementToRemove != null)
            {
                prefabList.prefabDataList.Remove(elementToRemove);
                DestroyImmediate(elementToRemove, true);
                AssetDatabase.SaveAssets();
            }
        }
    }
}