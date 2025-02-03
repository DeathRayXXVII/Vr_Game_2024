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
                Debug.LogWarning("Attempted to add a prefab, but no prefab was provided.", target);
            }
        }

        if (GUILayout.Button("Clear List"))
        {
            if (prefabList.prefabDataList.Count == 0)
            {
                EditorUtility.DisplayDialog("Clear List", "List is already empty.", "OK");
                return;
            }
            
            if (EditorUtility.DisplayDialog("Clear List", "Are you sure you want to clear the list?", "Yes", "No"))
            {
                foreach (var prefabData in prefabList.prefabDataList)
                {
                    DestroyImmediate(prefabData, true);
                }
                prefabList.prefabDataList.Clear();
                AssetDatabase.SaveAssets();
            }
        }

        if (prefabList.prefabDataList.Count == 0) return;
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        _showPrefabList = EditorGUILayout.Foldout(_showPrefabList, "Prefab Data List Items:");
        SessionState.SetBool("showPrefabList", _showPrefabList);

        if (_showPrefabList)
        {
            int counter = 0;
            const int col1MinWidth = 80;
            const int col1MaxWidth = 165;
            const int col3MaxWidth = 75;
            PrefabData elementToRemove = null;

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginVertical(GUI.skin.box); // Begin of List vertical group

            foreach (PrefabData prefabData in prefabList.prefabDataList)
            {
                if (prefabData == null)
                {
                    elementToRemove = prefabData;
                    continue;
                }

                EditorGUILayout.BeginVertical(GUI.skin.box); // Box for each list element
                
                // Row 1
                EditorGUILayout.BeginHorizontal();
                
                // Column 1 Row 1
                EditorGUILayout.LabelField("Element [" + counter + "]", GUILayout.MinWidth(col1MinWidth),
                    GUILayout.MaxWidth(col1MaxWidth), GUILayout.ExpandWidth(false));
                
                // Column 2 Row 1
                GUILayout.FlexibleSpace();
                
                // Column 3 Row 1
                EditorGUILayout.LabelField("Priority: " + prefabData.priority, GUILayout.MaxWidth(col3MaxWidth), GUILayout.ExpandWidth(false));
                
                // End Row 1
                EditorGUILayout.EndHorizontal();

                // Row 2
                EditorGUILayout.BeginHorizontal();
                
                try
                {
                    // Column 1 Row 2
                    EditorGUILayout.ObjectField(prefabData.prefab, typeof(GameObject), false,
                        GUILayout.MinWidth(col1MinWidth), GUILayout.MaxWidth(col1MaxWidth), GUILayout.ExpandWidth(false));
                }
                catch
                {
                    elementToRemove = prefabData;
                }
                
                // Column 2 Row 2
                GUILayout.FlexibleSpace();
                
                // Column 3 Row 2
                if (GUILayout.Button("Remove", GUILayout.MaxWidth(col3MaxWidth+10),
                        GUILayout.ExpandWidth(false)))
                {
                    elementToRemove = prefabData;
                }

                // End Row 2
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical(); // End box for each list element

                counter++;
            }

            EditorGUILayout.EndVertical(); // End of List vertical group

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