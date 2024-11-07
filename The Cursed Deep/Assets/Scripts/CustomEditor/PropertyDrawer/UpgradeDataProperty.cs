#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;
using ZPTools;
using ZPTools.Interface;

[CustomPropertyDrawer(typeof(UpgradeData), true)]
public class UpgradeDataEditor : PropertyDrawer
{
    // private string _lastFocusedControl = "";
    private Vector2 blobScrollPosition = Vector2.zero;

    private void GUILine(VisualElement container)
    {
        var line = new VisualElement();
        line.style.height = 1;
        line.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        container.Add(line);
    }

    private bool HasChanged<T>(T checkValue, T previousValue) => !checkValue.Equals(previousValue);

    private T PerformActionOnValueChange<T>(T checkValue, T previousValue, SerializedProperty property, System.Action action, bool allowDebug = false)
    {
        if (!HasChanged(checkValue, previousValue)) return previousValue;
        if (allowDebug) Debug.Log($"Value changed from {previousValue} to {checkValue}", property.serializedObject.targetObject);
        action();
        return checkValue;
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();

        Color darkGrey = new Color(0.3f, 0.3f, 0.3f);
        Color darkerGrey = new Color(0.15f, 0.15f, 0.15f);
        Color darkGreen = new Color(0.3f, 0.6f, 0.3f);
        
        SerializedObject obj = property.serializedObject;
        UpgradeData upgradeData = (UpgradeData)property.objectReferenceValue;

        SerializedProperty scriptProperty = obj.FindProperty("m_Script");

        SerializedProperty upgradeTypeProperty = obj.FindProperty("_upgradeDataType");
        SerializedProperty costTypeProperty = obj.FindProperty("_costDataType");
        DataType upgradeTypeEnum = (DataType)upgradeTypeProperty.enumValueIndex;
        DataType costTypeEnum = (DataType)costTypeProperty.enumValueIndex;

        SerializedProperty allowDebugProperty = obj.FindProperty("_allowDebug");

        int upgradeLevelProperty = upgradeData.upgradeLevel;
        object currentUpgradeProperty = upgradeData.upgradeValue;
        object currentCostProperty = upgradeData.upgradeCost;
        SerializedProperty baseFloatProperty = obj.FindProperty("_baseUpgradeFloat");
        SerializedProperty baseUpgradeIntProperty = obj.FindProperty("_baseUpgradeInt");
        SerializedProperty baseUpgradeFloatProperty = obj.FindProperty("_upgradeFloatContainer");
        SerializedProperty upgradeIntProperty = obj.FindProperty("_upgradeIntContainer");
        SerializedProperty costValueFloatProperty = obj.FindProperty("_costFloatContainer");
        SerializedProperty costValueIntProperty = obj.FindProperty("_costIntContainer");

        bool jsonLoadState = upgradeData.isLoaded;
        SerializedProperty jsonFileProperty = obj.FindProperty("_jsonFile");
        SerializedProperty valueKeyProperty = obj.FindProperty("_upgradeKey");
        SerializedProperty previousValueKey = obj.FindProperty("_previousUpgradeKey");
        SerializedProperty costKeyProperty = obj.FindProperty("_costKey");
        SerializedProperty previousCostKey = obj.FindProperty("_previousCostKey");

        SerializedProperty jsonBlob = obj.FindProperty("_jsonBlob");
        bool blobNeedsUpdate = false;

        // var debugString = "";

        // Draw the default script field
        var scriptField = new PropertyField(scriptProperty);
        scriptField.SetEnabled(false);
        root.Add(scriptField);
        
        var spaceSmall = new VisualElement
        {
            style =
            {
                height = 5
            }
        };
        root.Add(spaceSmall);

        var allowDebugField = new PropertyField(allowDebugProperty, "Allow Debug");
        root.Add(allowDebugField);

        var readOnlyContainer = new VisualElement();
        readOnlyContainer.style.backgroundColor = darkGrey;
        readOnlyContainer.Add(new Label("Read Only") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
        root.Add(readOnlyContainer);

        var upgradeLevelField = new IntegerField("Upgrade Level") { value = upgradeLevelProperty };
        upgradeLevelField.SetEnabled(false);
        readOnlyContainer.Add(upgradeLevelField);

        switch (upgradeTypeEnum)
        {
            case DataType.Float:
                var currentUpgradeFloatField = new FloatField("Current Upgrade") { value = (float)currentUpgradeProperty };
                currentUpgradeFloatField.SetEnabled(false);
                readOnlyContainer.Add(currentUpgradeFloatField);
                break;
            case DataType.Int:
                var currentUpgradeIntField = new IntegerField("Current Upgrade") { value = (int)currentUpgradeProperty };
                currentUpgradeIntField.SetEnabled(false);
                readOnlyContainer.Add(currentUpgradeIntField);
                break;
        }

        try
        {
            switch (costTypeEnum)
            {
                case DataType.Float:
                    var currentCostFloatField = new FloatField("Current Cost") { value = (float)currentCostProperty };
                    currentCostFloatField.SetEnabled(false);
                    readOnlyContainer.Add(currentCostFloatField);
                    break;
                case DataType.Int:
                    var currentCostIntField = new IntegerField("Current Cost") { value = (int)currentCostProperty };
                    currentCostIntField.SetEnabled(false);
                    readOnlyContainer.Add(currentCostIntField);
                    break;
            }
        }
        catch (System.InvalidCastException)
        {
            var currentType = costTypeEnum;
            costTypeProperty.enumValueIndex = costTypeEnum == DataType.Float ? (int)DataType.Int : (int)DataType.Float;
            costTypeEnum = (DataType)costTypeProperty.enumValueIndex;
            Debug.LogWarning($"Invalid expected type: {currentType}, attempting to switch type to {costTypeEnum}.",
                property.serializedObject.targetObject);
            obj.ApplyModifiedProperties();

            switch (costTypeEnum)
            {
                case DataType.Float:
                    var currentCostFloatField = new FloatField("Current Cost") { value = (float)currentCostProperty };
                    currentCostFloatField.SetEnabled(false);
                    readOnlyContainer.Add(currentCostFloatField);
                    break;
                case DataType.Int:
                    var currentCostIntField = new IntegerField("Current Cost") { value = (int)currentCostProperty };
                    currentCostIntField.SetEnabled(false);
                    readOnlyContainer.Add(currentCostIntField);
                    break;
            }
        }

        root.Add(spaceSmall);

        var upgradeDataContainer = new VisualElement();
        upgradeDataContainer.style.backgroundColor = darkGrey;
        upgradeDataContainer.Add(new Label("Upgrade Data") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
        root.Add(upgradeDataContainer);

        var upgradeTypeField = new EnumField("Upgrade Data Type", upgradeTypeEnum);
        upgradeTypeField.RegisterValueChangedCallback(evt =>
        {
            upgradeTypeProperty.enumValueIndex = (int)(DataType)evt.newValue;
            upgradeTypeEnum = (DataType)upgradeTypeProperty.enumValueIndex;
            obj.ApplyModifiedProperties();
        });
        upgradeDataContainer.Add(upgradeTypeField);

        var costTypeField = new EnumField("Cost Data Type", costTypeEnum);
        costTypeField.RegisterValueChangedCallback(evt =>
        {
            costTypeProperty.enumValueIndex = (int)(DataType)evt.newValue;
            costTypeEnum = (DataType)costTypeProperty.enumValueIndex;
            obj.ApplyModifiedProperties();
        });
        upgradeDataContainer.Add(costTypeField);

        switch (upgradeTypeEnum)
        {
            case DataType.Float:
                var baseValueFloatField = new PropertyField(baseFloatProperty, "Base Value");
                upgradeDataContainer.Add(baseValueFloatField);

                var upgradeFloatDataField = new ObjectField("Upgrade Holder") { objectType = typeof(FloatData), value = baseUpgradeFloatProperty.objectReferenceValue };
                upgradeFloatDataField.RegisterValueChangedCallback(evt =>
                {
                    baseUpgradeFloatProperty.objectReferenceValue = evt.newValue;
                    obj.ApplyModifiedProperties();
                });
                upgradeDataContainer.Add(upgradeFloatDataField);
                break;

            case DataType.Int:
                var baseValueIntField = new PropertyField(baseUpgradeIntProperty, "Base Value");
                upgradeDataContainer.Add(baseValueIntField);

                var upgradeIntDataField = new ObjectField("Upgrade Holder") { objectType = typeof(IntData), value = upgradeIntProperty.objectReferenceValue };
                upgradeIntDataField.RegisterValueChangedCallback(evt =>
                {
                    upgradeIntProperty.objectReferenceValue = evt.newValue;
                    obj.ApplyModifiedProperties();
                });
                upgradeDataContainer.Add(upgradeIntDataField);
                break;
        }

        switch (costTypeEnum)
        {
            case DataType.Float:
                var costFloatDataField = new ObjectField("Cost Holder") { objectType = typeof(FloatData), value = costValueFloatProperty.objectReferenceValue };
                costFloatDataField.RegisterValueChangedCallback(evt =>
                {
                    costValueFloatProperty.objectReferenceValue = evt.newValue;
                    obj.ApplyModifiedProperties();
                });
                upgradeDataContainer.Add(costFloatDataField);
                break;

            case DataType.Int:
                var costIntDataField = new ObjectField("Cost Holder") { objectType = typeof(IntData), value = costValueIntProperty.objectReferenceValue };
                costIntDataField.RegisterValueChangedCallback(evt =>
                {
                    costValueIntProperty.objectReferenceValue = evt.newValue;
                    obj.ApplyModifiedProperties();
                });
                upgradeDataContainer.Add(costIntDataField);
                break;
        }

        root.Add(spaceSmall);

        var jsonSettingsContainer = new VisualElement();
        jsonSettingsContainer.style.backgroundColor = darkGrey;
        jsonSettingsContainer.Add(new Label("Json Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
        root.Add(jsonSettingsContainer);

        var jsonLoadedToggle = new Toggle("Json is loaded:") { value = jsonLoadState };
        jsonLoadedToggle.SetEnabled(false);
        jsonSettingsContainer.Add(jsonLoadedToggle);

        var jsonFileField = new PropertyField(jsonFileProperty, "Json File");
        jsonSettingsContainer.Add(jsonFileField);

        var valueKeyField = new PropertyField(valueKeyProperty, "Value Key");
        jsonSettingsContainer.Add(valueKeyField);

        var costKeyField = new PropertyField(costKeyProperty, "Cost Key");
        jsonSettingsContainer.Add(costKeyField);

        if (!string.IsNullOrEmpty(jsonBlob.stringValue) || blobNeedsUpdate)
        {
            var loadedValuesContainer = new VisualElement();
            loadedValuesContainer.style.backgroundColor = darkGrey;
            loadedValuesContainer.Add(new Label("Loaded Values") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            jsonSettingsContainer.Add(loadedValuesContainer);

            var jsonBlobField = new TextField { value = jsonBlob.stringValue, multiline = true };
            jsonBlobField.SetEnabled(false);
            loadedValuesContainer.Add(jsonBlobField);
        }
        else
        {
            var noJsonDataLabel = new Label("No JSON data loaded.") { style = { unityFontStyleAndWeight = FontStyle.Italic } };
            jsonSettingsContainer.Add(noJsonDataLabel);
        }

        return root;
    }
}
#endif
// private VisualElement PropertyContainer;
//
// public override VisualElement CreatePropertyGUI(SerializedProperty Property)
// {
//     VisualElement root = new();
//     // root.AddToClassList("panel"); // Add a xml class to the root element
//
//     if (Property.objectReferenceValue != null)
//     {
//         
//     }
//
//     return root;
// }
//
// private void HandleSOChange(VisualElement ContentRoot, SerializedProperty Property, ChangeEvent<Object> ChangeEvent)
// {
//     if (ChangeEvent.newValue == null && PropertyContainer != null)
//     {
//         ContentRoot.Remove(PropertyContainer);
//         PropertyContainer = null;
//     }
//     else if (ChangeEvent.newValue != null && PropertyContainer == null)
//     {
//         ContentRoot.Add(BuildPropertyUI(Property));
//     }
// }
//
//
// private VisualElement BuildPropertyUI(SerializedProperty Property)
// {
//     PropertyContainer = new VisualElement();
//
//     SerializedObject serializedObject = new(Property.objectReferenceValue);
//
//     IntegerField maxAmmoField = new("Max Ammo");
//     maxAmmoField.BindProperty(serializedObject.FindProperty("MaxAmmo"));
//     PropertyContainer.Add(maxAmmoField);
//
//     IntegerField clipSizeField = new("Clip Size");
//     clipSizeField.BindProperty(serializedObject.FindProperty("ClipSize"));
//     PropertyContainer.Add(clipSizeField);
//
//     // CurrentAmmoField = new("Current Ammo");
//     // CurrentAmmoField.BindProperty(serializedObject.FindProperty("CurrentAmmo"));
//     //
//     // CurrentClipAmmoField = new("Current Clip Ammo");
//     // CurrentClipAmmoField.BindProperty(serializedObject.FindProperty("CurrentClipAmmo"));
//     // if (!EditorApplication.isPlaying && UseMaxAmmoAsCurrentAmmo)
//     // {
//     //     CurrentClipAmmoField.AddToClassList("hidden");
//     //     CurrentAmmoField.AddToClassList("hidden");
//     // }
//
//     Toggle defaultToMaxToggle = new("Default to Max Ammo");
//     // defaultToMaxToggle.value = UseMaxAmmoAsCurrentAmmo;
//     // defaultToMaxToggle.RegisterValueChangedCallback((changeEvent) =>
//     // {
//     //     if (!EditorApplication.isPlaying)
//     //     {
//     //         if (changeEvent.newValue)
//     //         {
//     //             CurrentClipAmmoField.AddToClassList("hidden");
//     //             CurrentAmmoField.AddToClassList("hidden");
//     //         }
//     //         else
//     //         {
//     //             CurrentClipAmmoField.RemoveFromClassList("hidden");
//     //             CurrentAmmoField.RemoveFromClassList("hidden");
//     //         }
//     //     }
//     // });
//     // PropertyContainer.Add(defaultToMaxToggle);
//     //
//     // Label overrideLabel = new("Current Clip Info");
//     // PropertyContainer.Add(CurrentAmmoField);
//     // PropertyContainer.Add(CurrentClipAmmoField);
//     //
//     // EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
//
//     return PropertyContainer;
// } 




    
//     private string _lastFocusedControl = "";
//     private Vector2 blobScrollPosition = Vector2.zero; 
//     
//     private void GUILine()
//     {
//         GUILayout.Space(-8f);
//         EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
//         GUILayout.Space(-8f);
//     }
//     
//     private bool HasChanged<T>(T checkValue, T previousValue) => !checkValue.Equals(previousValue);
//     
//     private T PerformActionOnValueChange<T>(T checkValue, T previousValue, System.Action action, bool allowDebug = false)
//     {
//         if (!HasChanged(checkValue, previousValue)) return previousValue;
//         if (allowDebug) Debug.Log($"Value changed from {previousValue} to {checkValue}", target);
//         action();
//         return checkValue;
//     }
//     
//     public override void OnInspectorGUI()
//     { 
//         Color darkGrey = new Color(0.3f, 0.3f, 0.3f);
//         Color darkerGrey = new Color(0.15f, 0.15f, 0.15f);
//         Color darkGreen = new Color(0.3f, 0.6f, 0.3f);
//         
//         SerializedObject obj = serializedObject;
//         UpgradeData upgradeData = (UpgradeData)target;
//         
//         SerializedProperty scriptProperty = obj.FindProperty("m_Script");
//         
//         SerializedProperty upgradeTypeProperty = obj.FindProperty("_upgradeDataType");
//         SerializedProperty costTypeProperty = obj.FindProperty("_costDataType");
//         DataType upgradeTypeEnum = (DataType)upgradeTypeProperty.enumValueIndex;
//         DataType costTypeEnum = (DataType)costTypeProperty.enumValueIndex;
//         
//         SerializedProperty allowDebugProperty = obj.FindProperty("_allowDebug");
//         
//         int upgradeLevelProperty = upgradeData.upgradeLevel;
//         object currentUpgradeProperty = upgradeData.upgradeValue;
//         object currentCostProperty = upgradeData.upgradeCost;
//         SerializedProperty baseFloatProperty = obj.FindProperty("_baseUpgradeFloat");
//         SerializedProperty baseUpgradeIntProperty = obj.FindProperty("_baseUpgradeInt");
//         SerializedProperty baseUpgradeFloatProperty = obj.FindProperty("_upgradeFloatContainer");
//         SerializedProperty upgradeIntProperty = obj.FindProperty("_upgradeIntContainer");
//         SerializedProperty costValueFloatProperty = obj.FindProperty("_costFloatContainer");
//         SerializedProperty costValueIntProperty = obj.FindProperty("_costIntContainer");
//         
//         bool jsonLoadState = upgradeData.isLoaded;
//         SerializedProperty jsonFileProperty = obj.FindProperty("_jsonFile");
//         SerializedProperty valueKeyProperty = obj.FindProperty("_upgradeKey");
//         SerializedProperty previousValueKey = obj.FindProperty("_previousUpgradeKey");
//         SerializedProperty costKeyProperty = obj.FindProperty("_costKey");
//         SerializedProperty previousCostKey = obj.FindProperty("_previousCostKey");
//         
//         SerializedProperty jsonBlob = obj.FindProperty("_jsonBlob");
//         bool blobNeedsUpdate = false;
//         
//         var debugString = "";
//         // Monitor focus
//         string currentFocusedControl = GUI.GetNameOfFocusedControl();
//         
//         // Draw the default script field
//         EditorGUI.BeginDisabledGroup(true);
//         EditorGUILayout.PropertyField(scriptProperty);
//         EditorGUI.EndDisabledGroup();
//         
//         GUILayout.Space(15f);
//         
//         EditorGUILayout.PropertyField(allowDebugProperty, new GUIContent("Allow Debug"));
//         EditorGUILayout.BeginVertical(GUI.skin.box);
//         GUILine();
//         
//         GUI.backgroundColor = darkGrey;
//         EditorGUILayout.BeginVertical(GUI.skin.box);
//         EditorGUILayout.LabelField("Read Only", EditorStyles.boldLabel);
//         EditorGUILayout.EndVertical();
//         GUI.backgroundColor = Color.white;
//         
//         // Draw read only fields
//         GUI.enabled = false;
//         EditorGUI.indentLevel++;
//         EditorGUILayout.IntField("Upgrade Level", upgradeLevelProperty);
//         
//         // EditorGUI.BeginChangeCheck();
//         switch (upgradeTypeEnum)
//         {
//             case DataType.Float:
//                 currentUpgradeProperty = (float)EditorGUILayout.FloatField("Current Upgrade", currentUpgradeProperty is float floatUpgrade ? floatUpgrade : 0f);
//                 break;
//             case DataType.Int:
//                 currentUpgradeProperty = (int)EditorGUILayout.IntField("Current Upgrade", currentUpgradeProperty is int intUpgrade ? intUpgrade : 0);
//                 break;
//         }
//         
//         // if (EditorGUI.EndChangeCheck())
//         // {
//         // }
//         try
//         {
//             switch (costTypeEnum)
//             {
//                 case DataType.Float:
//                     currentCostProperty = EditorGUILayout.FloatField("Current Cost", currentCostProperty is float floatCost ? floatCost : 0f);
//                     break;
//                 case DataType.Int:
//                     currentCostProperty = EditorGUILayout.IntField("Current Cost", currentCostProperty is int intCost ? intCost : 0);
//                     break;
//             }
//         }
//         catch (System.InvalidCastException)
//         {
//             var currentType = costTypeEnum;
//             costTypeProperty.enumValueIndex = costTypeEnum == DataType.Float ? (int)DataType.Int : (int)DataType.Float;
//             costTypeEnum = (DataType)costTypeProperty.enumValueIndex;
//             Debug.LogWarning(
//                 $"Invalid expected type: {currentType}, attempting to switch type to {costTypeEnum}.",
//                 this);
//             obj.ApplyModifiedProperties();
//             
//             switch (costTypeEnum)
//             {
//                 case DataType.Float:
//                     // EditorGUILayout.FloatField("Current Cost", currentCostProperty is float floatCost ? floatCost : 0f);
//                     EditorGUILayout.FloatField("Current Cost", (float)currentCostProperty);
//                     break;
//                 case DataType.Int:
//                     // EditorGUILayout.IntField("Current Cost", currentCostProperty is int intCost ? intCost : 0);
//                     EditorGUILayout.IntField("Current Cost", (int)currentCostProperty);
//                     break;
//             }
//         }
//
//         var output = currentCostProperty;
//         // Debug.Log($"Current Upgrade Property: {output} of type {output.GetType()}\nfloat = {output is float} ({typeof(float)}\nint = {output is int} ({typeof(int)}", this);
//         
//         EditorGUI.indentLevel--;
//         GUI.enabled = true;
//         EditorGUILayout.EndVertical();
//         
//         GUILine();
//         
//         EditorGUILayout.BeginVertical(GUI.skin.box);
//         GUI.backgroundColor = darkGrey;
//         EditorGUILayout.BeginVertical(GUI.skin.box);
//         EditorGUILayout.LabelField("Upgrade Data", EditorStyles.boldLabel);
//         EditorGUILayout.EndVertical();
//         GUI.backgroundColor = Color.white;
//         
//         // Draw field for data type
//         EditorGUI.indentLevel++;
//         upgradeTypeProperty.enumValueIndex = (int)(DataType)EditorGUILayout.EnumPopup("Upgrade Data Type", upgradeTypeEnum);
//         upgradeTypeEnum = (DataType)upgradeTypeProperty.enumValueIndex;
//         costTypeProperty.enumValueIndex = (int)(DataType)EditorGUILayout.EnumPopup("Cost Data Type", costTypeEnum);
//         costTypeEnum = (DataType)costTypeProperty.enumValueIndex;
//         
//         
//         // Show fields based on the Upgrade and Cost data type selected
//         switch (upgradeTypeEnum)
//         {
//             case DataType.Float:
//                 GUI.SetNextControlName("baseValueFloatField");
//                 EditorGUILayout.PropertyField(baseFloatProperty, new GUIContent("Base Value"));
//
//                 GUI.SetNextControlName("UpgradeFloatDataField");
//                 baseUpgradeFloatProperty.objectReferenceValue = EditorGUILayout.ObjectField("Upgrade Holder", baseUpgradeFloatProperty.objectReferenceValue, typeof(FloatData), false) as FloatData;
//                 break;
//
//             case DataType.Int:
//                 GUI.SetNextControlName("baseValueIntField");
//                 EditorGUILayout.PropertyField(baseUpgradeIntProperty, new GUIContent("Base Value"));
//
//                 GUI.SetNextControlName("UpgradeIntDataField");
//                 upgradeIntProperty.objectReferenceValue = EditorGUILayout.ObjectField("Upgrade Holder", upgradeIntProperty.objectReferenceValue, typeof(IntData), false) as IntData;
//                 break;
//         }
//         switch (costTypeEnum)
//         {
//             case DataType.Float:
//                 GUI.SetNextControlName("CostFloatDataField");
//                 costValueFloatProperty.objectReferenceValue = EditorGUILayout.ObjectField("Cost Holder", costValueFloatProperty.objectReferenceValue, typeof(FloatData), false) as FloatData;
//                 break;
//
//             case DataType.Int:
//                 GUI.SetNextControlName("CostIntDataField");
//                 costValueIntProperty.objectReferenceValue = EditorGUILayout.ObjectField("Cost Holder", costValueIntProperty.objectReferenceValue, typeof(IntData), false) as IntData;
//                 break;
//         }
//         EditorGUI.indentLevel--;
//         EditorGUILayout.EndVertical();
//         
//         GUILine();
//         
//         EditorGUILayout.BeginVertical(GUI.skin.box);
//         GUI.backgroundColor = darkGrey;
//         EditorGUILayout.BeginVertical(GUI.skin.box);
//         EditorGUILayout.LabelField("Json Settings", EditorStyles.boldLabel);
//         EditorGUILayout.EndVertical();
//         GUI.backgroundColor = Color.white;
//         
//         // Draw field for json key
//         EditorGUI.indentLevel++;
//         GUI.enabled = false;
//         EditorGUILayout.Toggle("Json is loaded:", jsonLoadState);
//         GUI.enabled = true;
//         EditorGUILayout.PropertyField(jsonFileProperty, new GUIContent("Json File"));
//         
//         // Set control names for focus detection
//         GUI.SetNextControlName("ValueKeyField");
//         EditorGUILayout.PropertyField(valueKeyProperty, new GUIContent("Value Key"));
//         
//         GUI.SetNextControlName("CostKeyField");
//         EditorGUILayout.PropertyField(costKeyProperty, new GUIContent("Value Key"));
//         
//         EditorGUI.indentLevel--;
//         
//         if (blobNeedsUpdate)
//         {
//             jsonBlob.stringValue = obj.FindProperty("_jsonBlob").stringValue;
//             blobNeedsUpdate = false;
//         }
//         
//         // Draw json blob if json blob is not empty
//         if (!string.IsNullOrEmpty(jsonBlob.stringValue) || blobNeedsUpdate)
//         {
//             EditorGUILayout.BeginVertical(GUI.skin.box);
//             GUI.backgroundColor = darkGrey;
//             EditorGUILayout.BeginVertical(GUI.skin.box);
//             EditorGUILayout.LabelField("Loaded Values", EditorStyles.boldLabel);
//             EditorGUILayout.EndVertical();
//             GUI.backgroundColor = Color.white;
//             blobScrollPosition = EditorGUILayout.BeginScrollView(blobScrollPosition, GUILayout.Height(100));
//             GUI.enabled = false;
//             GUI.contentColor = darkGreen;
//             GUI.backgroundColor = darkerGrey;
//             EditorGUILayout.TextArea(jsonBlob.stringValue, GUILayout.ExpandHeight(true));
//             GUI.backgroundColor = Color.white;
//             GUI.contentColor = Color.white;
//             GUI.enabled = true;
//             EditorGUILayout.EndScrollView();
//             EditorGUILayout.EndVertical();
//         }
//         else
//         {
//             EditorGUILayout.HelpBox("No JSON data loaded.", MessageType.Info);
//         }
//         EditorGUILayout.EndVertical();
//             
//         // Check for changes in the value key field and cost key field and trigger the load if they change or Enter is pressed
//         if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
//         {
//             switch (currentFocusedControl)
//             {
//                 case "ValueKeyField":
//                     jsonBlob.boolValue = blobNeedsUpdate = true;
//                     previousValueKey.stringValue = PerformActionOnValueChange(valueKeyProperty.stringValue,
//                         previousValueKey.stringValue, () => upgradeData.LoadOnGUIChange(true, false), allowDebugProperty.boolValue);
//                     break;
//                 case "CostKeyField":
//                     jsonBlob.boolValue = blobNeedsUpdate = true;
//                     previousCostKey.stringValue = PerformActionOnValueChange(costKeyProperty.stringValue,
//                         previousCostKey.stringValue, () => upgradeData.LoadOnGUIChange(false, true), allowDebugProperty.boolValue);
//                     break;
//             }
//             blobNeedsUpdate = true;
//             Event.current.Use();
//         }
//             
//         switch (_lastFocusedControl)
//         {
//             case "ValueKeyField":
//                 if (currentFocusedControl == "ValueKeyField") break;
//                 jsonBlob.boolValue = blobNeedsUpdate = true;
//                 previousValueKey.stringValue = PerformActionOnValueChange(valueKeyProperty.stringValue,
//                     previousValueKey.stringValue, () => upgradeData.LoadOnGUIChange(true, false), allowDebugProperty.boolValue);
//                 break;
//             case "CostKeyField":
//                 if (currentFocusedControl == "CostKeyField") break;
//                 if (costKeyProperty.stringValue == previousCostKey.stringValue) break;
//                 jsonBlob.boolValue = blobNeedsUpdate = true;
//                 previousCostKey.stringValue = PerformActionOnValueChange(costKeyProperty.stringValue,
//                     previousCostKey.stringValue, () => upgradeData.LoadOnGUIChange(false, true), allowDebugProperty.boolValue);
//                 break;
//         }
//         
//         if (Event.current.type != EventType.Repaint)
//         {
//             serializedObject.ApplyModifiedProperties();
//         }
//
//         if (target is INeedButton needButton)
//         {
//             var actions = needButton.GetButtonActions();
//             foreach (var action in actions)
//             {
//                 if (GUILayout.Button(action.Item2))
//                 {
//                     action.Item1.Invoke();
//                 }
//             }
//         }
//         
//         // Apply changes if anything has changed
//         if (!GUI.changed) return;
//         
//         // Set the last focused control
//         _lastFocusedControl = currentFocusedControl;
//        
//         if (upgradeTypeProperty.enumValueIndex != (int)upgradeTypeEnum)
//         {
//             debugString += "Upgrade Type changed.\n" +
//                             $"Upgrade Type: {(DataType)upgradeTypeProperty.enumValueIndex}, Was: {upgradeTypeEnum}\n";
//             jsonBlob.boolValue = blobNeedsUpdate = true;
//             upgradeData.LoadOnGUIChange(upgradeChanged: true, costChanged: false);
//         }else if (costTypeProperty.enumValueIndex != (int)costTypeEnum)
//         {
//             debugString += "Cost Type changed.\n" +
//                            $"Cost Type: {(DataType)costTypeProperty.enumValueIndex}, Was: {costTypeEnum}\n";
//             jsonBlob.boolValue = blobNeedsUpdate = true;
//             upgradeData.LoadOnGUIChange(upgradeChanged: false, costChanged: true);
//         }
//         else if (jsonFileProperty.objectReferenceValue != obj.FindProperty("_jsonFile").objectReferenceValue)
//         {
//             debugString += "Json File changed.\n" +
//                            $"Json File: {jsonFileProperty.objectReferenceValue}, Was: {obj.FindProperty("_jsonFile").objectReferenceValue}\n";
//             jsonBlob.boolValue = blobNeedsUpdate = true;
//             upgradeData.LoadOnGUIChange(upgradeChanged: true, costChanged: true);
//         }
//         
//         if (!string.IsNullOrEmpty(debugString) && allowDebugProperty.boolValue)
//             Debug.Log("Changes detected.\n" + debugString, target);
//
//         if (blobNeedsUpdate)
//         {
//             debugString += "Blob needs update.\n";
//         }
//         serializedObject.ApplyModifiedProperties();
//         EditorUtility.SetDirty(target);
//         Repaint();
//     }