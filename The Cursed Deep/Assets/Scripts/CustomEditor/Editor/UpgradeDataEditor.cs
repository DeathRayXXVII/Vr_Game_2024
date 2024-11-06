#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ZPTools.Interface;

[CustomEditor(typeof(UpgradeData))]
public class UpgradeDataEditor : Editor
{
    private string _lastFocusedControl = "";
    private Vector2 blobScrollPosition = Vector2.zero; 
    
    private void GUILine()
    {
        GUILayout.Space(-8f);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(-8f);
    }
    
    private bool HasChanged<T>(T checkValue, T previousValue) => !checkValue.Equals(previousValue);
    
    private T PerformActionOnValueChange<T>(T checkValue, T previousValue, System.Action action, bool allowDebug = false)
    {
        if (!HasChanged(checkValue, previousValue)) return previousValue;
        if (allowDebug) Debug.Log($"Value changed from {previousValue} to {checkValue}", target);
        action();
        return checkValue;
    }
    
    public override void OnInspectorGUI()
    { 
        Color darkGrey = new Color(0.3f, 0.3f, 0.3f);
        Color darkerGrey = new Color(0.15f, 0.15f, 0.15f);
        Color darkGreen = new Color(0.3f, 0.6f, 0.3f);
        
        SerializedObject obj = serializedObject;
        UpgradeData upgradeData = (UpgradeData)target;
        
        SerializedProperty scriptProperty = obj.FindProperty("m_Script");
        
        SerializedProperty upgradeTypeProperty = obj.FindProperty("_upgradeType");
        SerializedProperty costTypeProperty = obj.FindProperty("_costType");
        UpgradeData.DataType upgradeTypeEnum = (UpgradeData.DataType)upgradeTypeProperty.enumValueIndex;
        UpgradeData.DataType costTypeEnum = (UpgradeData.DataType)costTypeProperty.enumValueIndex;
        
        SerializedProperty allowDebugProperty = obj.FindProperty("_allowDebug");
        
        int upgradeLevelProperty = upgradeData.upgradeLevel;
        object currentUpgradeProperty = upgradeData.upgradeValue;
        object currentCostProperty = upgradeData.upgradeCost;
        SerializedProperty baseFloatValueProperty = obj.FindProperty("_baseValueFloat");
        SerializedProperty baseIntValueProperty = obj.FindProperty("_baseValueInt");
        SerializedProperty upgradeValueFloatProperty = obj.FindProperty("_valueFloat");
        SerializedProperty upgradeValueIntProperty = obj.FindProperty("_valueInt");
        SerializedProperty costValueFloatProperty = obj.FindProperty("_costFloat");
        SerializedProperty costValueIntProperty = obj.FindProperty("_costInt");
        
        bool jsonLoadState = upgradeData.isLoaded;
        SerializedProperty jsonFileProperty = obj.FindProperty("_jsonFile");
        SerializedProperty valueKeyProperty = obj.FindProperty("_valueKey");
        SerializedProperty previousValueKey = obj.FindProperty("_previousValueKey");
        SerializedProperty costKeyProperty = obj.FindProperty("_costKey");
        SerializedProperty previousCostKey = obj.FindProperty("_previousCostKey");
        
        SerializedProperty jsonBlob = obj.FindProperty("_jsonBlob");
        bool blobNeedsUpdate = false;
        
        var debugString = "";
        // Monitor focus
        string currentFocusedControl = GUI.GetNameOfFocusedControl();
        
        // Draw the default script field
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(scriptProperty);
        EditorGUI.EndDisabledGroup();
        
        GUILayout.Space(15f);
        
        EditorGUILayout.PropertyField(allowDebugProperty, new GUIContent("Allow Debug"));
        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUILine();
        
        GUI.backgroundColor = darkGrey;
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Read Only", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
        
        // Draw read only fields
        GUI.enabled = false;
        EditorGUI.indentLevel++;
        EditorGUILayout.IntField("Upgrade Level", upgradeLevelProperty);
        
        // EditorGUI.BeginChangeCheck();
        switch (upgradeTypeEnum)
        {
            case UpgradeData.DataType.Float:
                currentUpgradeProperty = (float)EditorGUILayout.FloatField("Current Upgrade", currentUpgradeProperty is float floatUpgrade ? floatUpgrade : 0f);
                break;
            case UpgradeData.DataType.Int:
                currentUpgradeProperty = (int)EditorGUILayout.IntField("Current Upgrade", currentUpgradeProperty is int intUpgrade ? intUpgrade : 0);
                break;
        }
        
        // if (EditorGUI.EndChangeCheck())
        // {
        // }
        try
        {
            switch (costTypeEnum)
            {
                case UpgradeData.DataType.Float:
                    currentCostProperty = EditorGUILayout.FloatField("Current Cost", currentCostProperty is float floatCost ? floatCost : 0f);
                    break;
                case UpgradeData.DataType.Int:
                    currentCostProperty = EditorGUILayout.IntField("Current Cost", currentCostProperty is int intCost ? intCost : 0);
                    break;
            }
        }
        catch (System.InvalidCastException)
        {
            var currentType = costTypeEnum;
            costTypeProperty.enumValueIndex = costTypeEnum == UpgradeData.DataType.Float ? (int)UpgradeData.DataType.Int : (int)UpgradeData.DataType.Float;
            costTypeEnum = (UpgradeData.DataType)costTypeProperty.enumValueIndex;
            Debug.LogWarning(
                $"Invalid expected type: {currentType}, attempting to switch type to {costTypeEnum}.",
                this);
            obj.ApplyModifiedProperties();
            
            switch (costTypeEnum)
            {
                case UpgradeData.DataType.Float:
                    currentCostProperty = EditorGUILayout.FloatField("Current Cost", currentCostProperty is float floatCost ? floatCost : 0f);
                    break;
                case UpgradeData.DataType.Int:
                    currentCostProperty = EditorGUILayout.IntField("Current Cost", currentCostProperty is int intCost ? intCost : 0);
                    break;
            }
        }
    
        // Debug.Log($"Current Upgrade Property: {currentUpgradeProperty} of type {currentUpgradeProperty.GetType()}\nfloat = {currentUpgradeProperty is float} ({typeof(float)}\nint = {currentUpgradeProperty is int} ({typeof(int)}");
        EditorGUI.indentLevel--;
        GUI.enabled = true;
        EditorGUILayout.EndVertical();
        
        GUILine();
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUI.backgroundColor = darkGrey;
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Upgrade Data", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
        
        // Draw field for data type
        EditorGUI.indentLevel++;
        upgradeTypeProperty.enumValueIndex = (int)(UpgradeData.DataType)EditorGUILayout.EnumPopup("Upgrade Data Type", upgradeTypeEnum);
        upgradeTypeEnum = (UpgradeData.DataType)upgradeTypeProperty.enumValueIndex;
        costTypeProperty.enumValueIndex = (int)(UpgradeData.DataType)EditorGUILayout.EnumPopup("Cost Data Type", costTypeEnum);
        costTypeEnum = (UpgradeData.DataType)costTypeProperty.enumValueIndex;
        
        
        // Show fields based on the Upgrade and Cost data type selected
        switch (upgradeTypeEnum)
        {
            case UpgradeData.DataType.Float:
                GUI.SetNextControlName("baseValueFloatField");
                EditorGUILayout.PropertyField(baseFloatValueProperty, new GUIContent("Base Value"));

                GUI.SetNextControlName("UpgradeFloatDataField");
                upgradeValueFloatProperty.objectReferenceValue = EditorGUILayout.ObjectField("Upgrade Holder", upgradeValueFloatProperty.objectReferenceValue, typeof(FloatData), false) as FloatData;
                break;

            case UpgradeData.DataType.Int:
                GUI.SetNextControlName("baseValueIntField");
                EditorGUILayout.PropertyField(baseIntValueProperty, new GUIContent("Base Value"));

                GUI.SetNextControlName("UpgradeIntDataField");
                upgradeValueIntProperty.objectReferenceValue = EditorGUILayout.ObjectField("Upgrade Holder", upgradeValueIntProperty.objectReferenceValue, typeof(IntData), false) as IntData;
                break;
        }
        switch (costTypeEnum)
        {
            case UpgradeData.DataType.Float:
                GUI.SetNextControlName("CostFloatDataField");
                costValueFloatProperty.objectReferenceValue = EditorGUILayout.ObjectField("Cost Holder", costValueFloatProperty.objectReferenceValue, typeof(FloatData), false) as FloatData;
                break;

            case UpgradeData.DataType.Int:
                GUI.SetNextControlName("CostIntDataField");
                costValueIntProperty.objectReferenceValue = EditorGUILayout.ObjectField("Cost Holder", costValueIntProperty.objectReferenceValue, typeof(IntData), false) as IntData;
                break;
        }
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        
        GUILine();
        
        EditorGUILayout.BeginVertical(GUI.skin.box);
        GUI.backgroundColor = darkGrey;
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField("Json Settings", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
        
        // Draw field for json key
        EditorGUI.indentLevel++;
        GUI.enabled = false;
        EditorGUILayout.Toggle("Json is loaded:", jsonLoadState);
        GUI.enabled = true;
        EditorGUILayout.PropertyField(jsonFileProperty, new GUIContent("Json File"));
        
        // Set control names for focus detection
        GUI.SetNextControlName("ValueKeyField");
        EditorGUILayout.PropertyField(valueKeyProperty, new GUIContent("Value Key"));
        
        GUI.SetNextControlName("CostKeyField");
        EditorGUILayout.PropertyField(costKeyProperty, new GUIContent("Value Key"));
        
        EditorGUI.indentLevel--;
        
        if (blobNeedsUpdate)
        {
            jsonBlob.stringValue = obj.FindProperty("_jsonBlob").stringValue;
            blobNeedsUpdate = false;
        }
        
        // Draw json blob if json blob is not empty
        if (!string.IsNullOrEmpty(jsonBlob.stringValue) || blobNeedsUpdate)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = darkGrey;
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Loaded Values", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
            blobScrollPosition = EditorGUILayout.BeginScrollView(blobScrollPosition, GUILayout.Height(100));
            GUI.enabled = false;
            GUI.contentColor = darkGreen;
            GUI.backgroundColor = darkerGrey;
            EditorGUILayout.TextArea(jsonBlob.stringValue, GUILayout.ExpandHeight(true));
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.white;
            GUI.enabled = true;
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("No JSON data loaded.", MessageType.Info);
        }
        EditorGUILayout.EndVertical();
            
        // Check for changes in the value key field and cost key field and trigger the load if they change or Enter is pressed
        if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
        {
            switch (currentFocusedControl)
            {
                case "ValueKeyField":
                    previousValueKey.stringValue = PerformActionOnValueChange(valueKeyProperty.stringValue,
                        previousValueKey.stringValue, () => upgradeData.LoadOnGUIChange(true, false), allowDebugProperty.boolValue);
                    break;
                case "CostKeyField":
                    previousCostKey.stringValue = PerformActionOnValueChange(costKeyProperty.stringValue,
                        previousCostKey.stringValue, () => upgradeData.LoadOnGUIChange(false, true), allowDebugProperty.boolValue);
                    break;
            }
            blobNeedsUpdate = true;
            Event.current.Use();
        }
            
        switch (_lastFocusedControl)
        {
            case "ValueKeyField":
                if (currentFocusedControl == "ValueKeyField") break;
                previousValueKey.stringValue = PerformActionOnValueChange(valueKeyProperty.stringValue,
                    previousValueKey.stringValue, () => upgradeData.LoadOnGUIChange(true, false), allowDebugProperty.boolValue);
                blobNeedsUpdate = true;
                break;
            case "CostKeyField":
                if (currentFocusedControl == "CostKeyField") break;
                if (costKeyProperty.stringValue == previousCostKey.stringValue) break;
                previousCostKey.stringValue = PerformActionOnValueChange(costKeyProperty.stringValue,
                    previousCostKey.stringValue, () => upgradeData.LoadOnGUIChange(false, true), allowDebugProperty.boolValue);
                blobNeedsUpdate = true;
                break;
        }
        
        if (Event.current.type != EventType.Repaint)
        {
            serializedObject.ApplyModifiedProperties();
        }

        if (target is INeedButton needButton)
        {
            var actions = needButton.GetButtonActions();
            foreach (var action in actions)
            {
                if (GUILayout.Button(action.Item2))
                {
                    action.Item1.Invoke();
                }
            }
        }
        
        // Apply changes if anything has changed
        if (!GUI.changed) return;
        
        // Set the last focused control
        _lastFocusedControl = currentFocusedControl;
       
        if (upgradeTypeProperty.enumValueIndex != (int)upgradeTypeEnum)
        {
            debugString += "Upgrade Type changed.\n" +
                            $"Upgrade Type: {(UpgradeData.DataType)upgradeTypeProperty.enumValueIndex}, Was: {upgradeTypeEnum}\n";
            upgradeData.LoadOnGUIChange(upgradeChanged: true, costChanged: false);
            blobNeedsUpdate = true;
        }else if (costTypeProperty.enumValueIndex != (int)costTypeEnum)
        {
            debugString += "Cost Type changed.\n" +
                           $"Cost Type: {(UpgradeData.DataType)costTypeProperty.enumValueIndex}, Was: {costTypeEnum}\n";
            upgradeData.LoadOnGUIChange(upgradeChanged: false, costChanged: true);
            blobNeedsUpdate = true;
        }
        else if (jsonFileProperty.objectReferenceValue != obj.FindProperty("_jsonFile").objectReferenceValue)
        {
            debugString += "Json File changed.\n" +
                           $"Json File: {jsonFileProperty.objectReferenceValue}, Was: {obj.FindProperty("_jsonFile").objectReferenceValue}\n";
            upgradeData.LoadOnGUIChange(upgradeChanged: true, costChanged: true);
            blobNeedsUpdate = true;
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        if (blobNeedsUpdate)
        {
            debugString += "Blob needs update.\n";
            Repaint();
        }
        
        if (!string.IsNullOrEmpty(debugString) && allowDebugProperty.boolValue)
            Debug.Log("Changes detected.\n" + debugString, target);
    }
}
#endif