using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using ZPTools;
using ZPTools.Interface;

[CustomEditor(typeof(UpgradeData), true)]
public class UpgradeDataEditor : Editor
{
    private enum UiDataType
    {
        Int,
        Float,
    }

    Color darkGrey = new(0.3f, 0.3f, 0.3f);
    Color darkerGrey = new(0.1f, 0.1f, 0.1f);
    StyleColor darkGreen = new(new Color(0.3f, 0.6f, 0.3f));
    
    const int fieldSpacing = 2;
    
    private string _lastFocusedControl = "";
    private Vector2 blobScrollPosition = Vector2.zero;
    

    UpgradeData upgradeData;
    private SerializedProperty upgradeTypeProperty;
    private SerializedProperty costTypeProperty;
    private UiDataType upgradeTypeEnum;
    private UiDataType costTypeEnum;

    public override VisualElement CreateInspectorGUI()
    {
        var inspector = new VisualElement();
        
        upgradeData = (UpgradeData)target;
        upgradeTypeProperty = serializedObject.FindProperty("_upgradeDataType");
        costTypeProperty = serializedObject.FindProperty("_costDataType");
        upgradeTypeEnum = (UiDataType)upgradeTypeProperty.enumValueIndex;
        costTypeEnum = (UiDataType)costTypeProperty.enumValueIndex;

        SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");

        SerializedProperty allowDebugProperty = serializedObject.FindProperty("_allowDebug");

        // Draw the default script field
        var scriptField = new PropertyField(scriptProperty);
        scriptField.SetEnabled(false);
        inspector.Add(scriptField);
        
        inspector.Add(UISpacer(30));
        var allowDebugField = new PropertyField(allowDebugProperty, "Allow Debug");
        inspector.Add(allowDebugField);
        inspector.Add(UISpacer(10));
        
        inspector.Add(ReadOnlyDataElement());
        inspector.Add(UpgradeDataElement());
        inspector.Add(JsonDataElement());
        
        return inspector;
    }
    
    private VisualElement UISpacer(int height) => new() { style = { height = height } };

    private VisualElement UIGroup(string title)
    {
        const int containerBorder = 1;
        const int containerSpacing = 3;
        const float headerPadding = 1.5f;
        const int headerFontSize = 12;
        
        var groupContainer = new VisualElement { style =
            {
                backgroundColor = darkGrey,
                borderLeftColor = Color.black,
                borderRightColor = Color.black,
                borderBottomColor = Color.black,
                borderTopColor = Color.black,
                borderTopWidth = containerBorder,
                borderRightWidth = containerBorder,
                borderLeftWidth = containerBorder,
                borderBottomWidth = containerBorder,
                marginBottom = containerSpacing
            }
        };
        var header = new Label(title) { style =
            {
                backgroundColor = darkerGrey,
                unityFontStyleAndWeight = FontStyle.Bold,
                fontSize = headerFontSize,
                paddingTop = headerPadding,
                paddingLeft = headerPadding,
                paddingBottom = headerPadding,
            }
        };
        
        groupContainer.Add(header);
        
        return groupContainer;
    } 

    private static VisualElement UIBody()
    {
        const int bodyIndent = 15;
        const int bodyPadding = 2;

        var body = new VisualElement { style =
            {
                marginTop = bodyPadding,
                marginRight = bodyIndent / 2,
                marginLeft = bodyIndent,
                marginBottom = bodyPadding,
                paddingTop = bodyPadding,
                paddingRight = bodyPadding,
                paddingLeft = bodyPadding,
                paddingBottom = bodyPadding,
            }
        };
        
        return body;
    }

    private VisualElement ReadOnlyDataElement()
    {
        int upgradeLevelProperty = upgradeData.upgradeLevel;
        object currentUpgradeProperty = upgradeData.upgradeValue;
        object currentCostProperty = upgradeData.upgradeCost;
        
        var readOnlyContainer = UIGroup("Read Only");
        var body = UIBody();
        readOnlyContainer.Add(body);

        var upgradeLevelField = new IntegerField("Upgrade Level") { value = upgradeLevelProperty };
        upgradeLevelField.SetEnabled(false);
        body.Add(upgradeLevelField);

        switch (upgradeTypeEnum)
        {
            case UiDataType.Float:
                var currentUpgradeFloatField = new FloatField("Current Upgrade") { value = currentUpgradeProperty != null ? (float)currentUpgradeProperty : 0f };
                currentUpgradeFloatField.SetEnabled(false);
                body.Add(currentUpgradeFloatField);
                break;
            case UiDataType.Int:
                var currentUpgradeIntField = new IntegerField("Current Upgrade") { value = currentUpgradeProperty != null ? (int)currentUpgradeProperty : 0 };
                currentUpgradeIntField.SetEnabled(false);
                body.Add(currentUpgradeIntField);
                break;
        }

        try
        {
            switch (costTypeEnum)
            {
                case UiDataType.Float:
                    var currentCostFloatField = new FloatField("Current Cost") { value = (float)currentCostProperty };
                    currentCostFloatField.SetEnabled(false);
                    body.Add(currentCostFloatField);
                    break;
                case UiDataType.Int:
                    var currentCostIntField = new IntegerField("Current Cost") { value = (int)currentCostProperty };
                    currentCostIntField.SetEnabled(false);
                    body.Add(currentCostIntField);
                    break;
            }
        }
        catch (System.InvalidCastException)
        {
            var currentType = costTypeEnum;
            costTypeProperty.enumValueIndex = costTypeEnum == UiDataType.Float ? (int)UiDataType.Int : (int)UiDataType.Float;
            costTypeEnum = (UiDataType)costTypeProperty.enumValueIndex;
            Debug.LogWarning($"Invalid expected type: {currentType}, attempting to switch type to {costTypeEnum}.",
                target);
            serializedObject.ApplyModifiedProperties();

            switch (costTypeEnum)
            {
                case UiDataType.Float:
                    var currentCostFloatField = new FloatField("Current Cost") { value = (float)currentCostProperty };
                    currentCostFloatField.SetEnabled(false);
                    body.Add(currentCostFloatField);
                    break;
                case UiDataType.Int:
                    var currentCostIntField = new IntegerField("Current Cost") { value = (int)currentCostProperty };
                    currentCostIntField.SetEnabled(false);
                    body.Add(currentCostIntField);
                    break;
            }
        }
        
        return readOnlyContainer;
    }
    
    private VisualElement UpgradeDataElement()
    {
        SerializedProperty baseFloatProperty = serializedObject.FindProperty("_baseUpgradeFloat");
        SerializedProperty baseUpgradeIntProperty = serializedObject.FindProperty("_baseUpgradeInt");
        SerializedProperty baseUpgradeFloatProperty = serializedObject.FindProperty("_upgradeFloatContainer");
        SerializedProperty upgradeIntProperty = serializedObject.FindProperty("_upgradeIntContainer");
        SerializedProperty costValueFloatProperty = serializedObject.FindProperty("_costFloatContainer");
        SerializedProperty costValueIntProperty = serializedObject.FindProperty("_costIntContainer");
        
        var upgradeDataContainer = UIGroup("Upgrade Data");
        var body = UIBody();
        upgradeDataContainer.Add(body);
        
        var upgradeTypeField = new EnumField("Upgrade Data Type", upgradeTypeEnum) { style = { marginBottom = fieldSpacing } };
        upgradeTypeField.RegisterValueChangedCallback(evt =>
        {
            upgradeTypeProperty.enumValueIndex = (int)(UiDataType)evt.newValue;
            upgradeTypeEnum = (UiDataType)upgradeTypeProperty.enumValueIndex;
            serializedObject.ApplyModifiedProperties();
        });
        body.Add(upgradeTypeField);

        var costTypeField = new EnumField("Cost Data Type", costTypeEnum) { style = { marginBottom = fieldSpacing } };
        costTypeField.RegisterValueChangedCallback(evt =>
        {
            costTypeProperty.enumValueIndex = (int)(DataType)evt.newValue;
            costTypeEnum = (UiDataType)costTypeProperty.enumValueIndex;
            serializedObject.ApplyModifiedProperties();
        });
        body.Add(costTypeField);
        
        var baseValueFloatField = new PropertyField(baseFloatProperty, "Base Value") { style = { marginBottom = fieldSpacing } };
        body.Add(baseValueFloatField);

        switch (upgradeTypeEnum)
        {
            case UiDataType.Float:

                var upgradeFloatDataField = new ObjectField("Upgrade Holder")
                {
                    objectType = typeof(FloatData),
                    value = baseUpgradeFloatProperty.objectReferenceValue,
                    style = { marginBottom = fieldSpacing }
                };
                upgradeFloatDataField.RegisterValueChangedCallback(evt =>
                {
                    baseUpgradeFloatProperty.objectReferenceValue = evt.newValue;
                    serializedObject.ApplyModifiedProperties();
                });
                body.Add(upgradeFloatDataField);
                break;

            case UiDataType.Int:
                var baseValueIntField = new PropertyField(baseUpgradeIntProperty, "Base Value");
                body.Add(baseValueIntField);

                var upgradeIntDataField = new ObjectField("Upgrade Holder")
                {
                    objectType = typeof(IntData),
                    value = upgradeIntProperty.objectReferenceValue,
                    style = { marginBottom = fieldSpacing }
                };
                upgradeIntDataField.RegisterValueChangedCallback(evt =>
                {
                    upgradeIntProperty.objectReferenceValue = evt.newValue;
                    serializedObject.ApplyModifiedProperties();
                });
                body.Add(upgradeIntDataField);
                break;
        }

        switch (costTypeEnum)
        {
            case UiDataType.Float:
                var costFloatDataField = new ObjectField("Cost Holder")
                {
                    objectType = typeof(FloatData),
                    value = costValueFloatProperty.objectReferenceValue,
                    style = { marginBottom = fieldSpacing }
                };
                costFloatDataField.RegisterValueChangedCallback(evt =>
                {
                    costValueFloatProperty.objectReferenceValue = evt.newValue;
                    serializedObject.ApplyModifiedProperties();
                });
                body.Add(costFloatDataField);
                break;

            case UiDataType.Int:
                var costIntDataField = new ObjectField("Cost Holder")
                {
                    objectType = typeof(IntData),
                    value = costValueIntProperty.objectReferenceValue,
                    style = { marginBottom = fieldSpacing }
                };
                costIntDataField.RegisterValueChangedCallback(evt =>
                {
                    costValueIntProperty.objectReferenceValue = evt.newValue;
                    serializedObject.ApplyModifiedProperties();
                });
                body.Add(costIntDataField);
                break;
        }
        
        return upgradeDataContainer;
    }

    private VisualElement JsonDataElement()
    {
        bool jsonLoadState = upgradeData.isLoaded;
        SerializedProperty jsonFileProperty = serializedObject.FindProperty("_jsonFile");
        SerializedProperty valueKeyProperty = serializedObject.FindProperty("_upgradeKey");
        SerializedProperty previousValueKey = serializedObject.FindProperty("_previousUpgradeKey");
        SerializedProperty costKeyProperty = serializedObject.FindProperty("_costKey");
        SerializedProperty previousCostKey = serializedObject.FindProperty("_previousCostKey");

        SerializedProperty jsonBlob = serializedObject.FindProperty("_jsonBlob");
        bool blobNeedsUpdate = false;
        var jsonSettingsContainer = UIGroup("Json Settings");
        
        var body = UIBody();
        jsonSettingsContainer.Add(body);

        var jsonLoadedToggle = new Toggle("Json is loaded:") { value = jsonLoadState };
        jsonLoadedToggle.SetEnabled(false);
        body.Add(jsonLoadedToggle);

        var jsonFileField = new PropertyField(jsonFileProperty, "Json File");
        body.Add(jsonFileField);

        var valueKeyField = new PropertyField(valueKeyProperty, "Upgrade Key");
        body.Add(valueKeyField);

        var costKeyField = new PropertyField(costKeyProperty, "Cost Key");
        body.Add(costKeyField);

        if (!string.IsNullOrEmpty(jsonBlob.stringValue) || blobNeedsUpdate)
        {
            var loadedValuesContainer = new VisualElement() { style = { backgroundColor = darkGrey } };
            loadedValuesContainer.Add(new Label("Loaded Values") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            body.Add(loadedValuesContainer);

            var jsonBlobField = new TextField()
            {
                value = jsonBlob.stringValue, 
                multiline = true,
    
                style =
                {
                    backgroundColor = darkerGrey,
                    color = darkGreen
                },
                isReadOnly = true,
            };
            // jsonBlobField.SetEnabled(false);

            var scrollView = new ScrollView(ScrollViewMode.Vertical) { style = { maxHeight = 150 },
                horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scrollView.Add(jsonBlobField);
            loadedValuesContainer.Add(scrollView);
        }
        else
        {
            var noJsonDataLabel = new Label("No JSON data loaded.") { style = { unityFontStyleAndWeight = FontStyle.Italic } };
            body.Add(noJsonDataLabel);
        }
        
        return jsonSettingsContainer;
    }

    private bool HasChanged<T>(T checkValue, T previousValue) => !checkValue.Equals(previousValue);
}
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
//         UpgradeData.DataType upgradeTypeEnum = (UpgradeData.DataType)upgradeTypeProperty.enumValueIndex;
//         UpgradeData.DataType costTypeEnum = (UpgradeData.DataType)costTypeProperty.enumValueIndex;
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
//             case UpgradeData.DataType.Float:
//                 currentUpgradeProperty = (float)EditorGUILayout.FloatField("Current Upgrade", currentUpgradeProperty is float floatUpgrade ? floatUpgrade : 0f);
//                 break;
//             case UpgradeData.DataType.Int:
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
//                 case UpgradeData.DataType.Float:
//                     currentCostProperty = EditorGUILayout.FloatField("Current Cost", currentCostProperty is float floatCost ? floatCost : 0f);
//                     break;
//                 case UpgradeData.DataType.Int:
//                     currentCostProperty = EditorGUILayout.IntField("Current Cost", currentCostProperty is int intCost ? intCost : 0);
//                     break;
//             }
//         }
//         catch (System.InvalidCastException)
//         {
//             var currentType = costTypeEnum;
//             costTypeProperty.enumValueIndex = costTypeEnum == UpgradeData.DataType.Float ? (int)UpgradeData.DataType.Int : (int)UpgradeData.DataType.Float;
//             costTypeEnum = (UpgradeData.DataType)costTypeProperty.enumValueIndex;
//             Debug.LogWarning(
//                 $"Invalid expected type: {currentType}, attempting to switch type to {costTypeEnum}.",
//                 this);
//             obj.ApplyModifiedProperties();
//             
//             switch (costTypeEnum)
//             {
//                 case UpgradeData.DataType.Float:
//                     // EditorGUILayout.FloatField("Current Cost", currentCostProperty is float floatCost ? floatCost : 0f);
//                     EditorGUILayout.FloatField("Current Cost", (float)currentCostProperty);
//                     break;
//                 case UpgradeData.DataType.Int:
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
//         upgradeTypeProperty.enumValueIndex = (int)(UpgradeData.DataType)EditorGUILayout.EnumPopup("Upgrade Data Type", upgradeTypeEnum);
//         upgradeTypeEnum = (UpgradeData.DataType)upgradeTypeProperty.enumValueIndex;
//         costTypeProperty.enumValueIndex = (int)(UpgradeData.DataType)EditorGUILayout.EnumPopup("Cost Data Type", costTypeEnum);
//         costTypeEnum = (UpgradeData.DataType)costTypeProperty.enumValueIndex;
//         
//         
//         // Show fields based on the Upgrade and Cost data type selected
//         switch (upgradeTypeEnum)
//         {
//             case UpgradeData.DataType.Float:
//                 GUI.SetNextControlName("baseValueFloatField");
//                 EditorGUILayout.PropertyField(baseFloatProperty, new GUIContent("Base Value"));
//
//                 GUI.SetNextControlName("UpgradeFloatDataField");
//                 baseUpgradeFloatProperty.objectReferenceValue = EditorGUILayout.ObjectField("Upgrade Holder", baseUpgradeFloatProperty.objectReferenceValue, typeof(FloatData), false) as FloatData;
//                 break;
//
//             case UpgradeData.DataType.Int:
//                 GUI.SetNextControlName("baseValueIntField");
//                 EditorGUILayout.PropertyField(baseUpgradeIntProperty, new GUIContent("Base Value"));
//
//                 GUI.SetNextControlName("UpgradeIntDataField");
//                 upgradeIntProperty.objectReferenceValue = EditorGUILayout.ObjectField("Upgrade Holder", upgradeIntProperty.objectReferenceValue, typeof(IntData), false) as IntData;
//                 break;
//         }
//         switch (costTypeEnum)
//         {
//             case UpgradeData.DataType.Float:
//                 GUI.SetNextControlName("CostFloatDataField");
//                 costValueFloatProperty.objectReferenceValue = EditorGUILayout.ObjectField("Cost Holder", costValueFloatProperty.objectReferenceValue, typeof(FloatData), false) as FloatData;
//                 break;
//
//             case UpgradeData.DataType.Int:
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
//                             $"Upgrade Type: {(UpgradeData.DataType)upgradeTypeProperty.enumValueIndex}, Was: {upgradeTypeEnum}\n";
//             jsonBlob.boolValue = blobNeedsUpdate = true;
//             upgradeData.LoadOnGUIChange(upgradeChanged: true, costChanged: false);
//         }else if (costTypeProperty.enumValueIndex != (int)costTypeEnum)
//         {
//             debugString += "Cost Type changed.\n" +
//                            $"Cost Type: {(UpgradeData.DataType)costTypeProperty.enumValueIndex}, Was: {costTypeEnum}\n";
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
// }