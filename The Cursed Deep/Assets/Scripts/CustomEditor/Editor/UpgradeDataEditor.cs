using UnityEngine;
using UnityEditor;
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
    Color transparent = new(0, 0, 0, 0);
    StyleColor darkGreen = new(new Color(0.3f, 0.6f, 0.3f));
    
    const int fieldSpacing = 2;
    
    UpgradeData upgradeData;
    private SerializedProperty upgradeTypeProperty;
    private SerializedProperty costTypeProperty;
    private UiDataType upgradeTypeEnum;
    private UiDataType costTypeEnum;
    
    private void UpdateEditor()
    {
        // serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        SceneView.RepaintAll();
    }
    
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
        inspector.Add(ButtonActionsElement());
        
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
        SerializedProperty upgradeKeyProperty = serializedObject.FindProperty("_upgradeKey");
        SerializedProperty previousValueKey = serializedObject.FindProperty("_previousUpgradeKey");
        SerializedProperty costKeyProperty = serializedObject.FindProperty("_costKey");
        SerializedProperty previousCostKey = serializedObject.FindProperty("_previousCostKey");
        
        bool upgradeIsLoaded = serializedObject.FindProperty("upgradeIsLoaded").boolValue;
        bool costIsLoaded = serializedObject.FindProperty("costIsLoaded").boolValue;

        SerializedProperty jsonBlob = serializedObject.FindProperty("_jsonBlob");
        var jsonSettingsContainer = UIGroup("Json Settings");
        
        var body = UIBody();
        jsonSettingsContainer.Add(body);

        var jsonLoadedToggle = new Toggle("Json is loaded:") { value = jsonLoadState };
        jsonLoadedToggle.SetEnabled(false);
        body.Add(jsonLoadedToggle);

        var jsonFileField = new PropertyField(jsonFileProperty, "Json File");
        body.Add(jsonFileField);

        var valueKeyField = new PropertyField(upgradeKeyProperty, "Upgrade Key");
        body.Add(valueKeyField);

        var costKeyField = new PropertyField(costKeyProperty, "Cost Key");
        body.Add(costKeyField);

        if (!string.IsNullOrEmpty(jsonBlob.stringValue))
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

            var scrollView = new ScrollView(ScrollViewMode.Vertical) { style = { maxHeight = 150 },
                horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scrollView.Add(jsonBlobField);
            loadedValuesContainer.Add(scrollView);
        }
        else
        {
            var noJsonDataLabel = new HelpBox(
                $"No JSON data loaded.\n" +
                $"{(!upgradeIsLoaded ? "Upgrade Key is not in JSON file.\n" : "")}" +
                $"{(!costIsLoaded ? "Cost Key is not in JSON file.\n" : "")}",
                HelpBoxMessageType.Info);
            body.Add(noJsonDataLabel);
        }
        
        return jsonSettingsContainer;
    }
    
    private VisualElement ButtonActionsElement()
    {
        var buttonActionsContainer = UIGroup("Button Actions");
        System.Collections.Generic.List<(System.Action, string)> buttonActions = upgradeData.GetButtonActions();
        foreach (var buttonAction in buttonActions)
        {
            var buttonField = new VisualElement { style = { marginBottom = fieldSpacing, position = Position.Relative, height = 20 } };
            var label = new Label(buttonAction.Item2) { style = { unityFontStyleAndWeight = FontStyle.Bold, flexGrow = 1, alignSelf = Align.FlexStart, marginLeft = 10, marginTop = fieldSpacing, marginBottom = 0 } };
            var button = new Button(() =>
            {
                buttonAction.Item1();
                UpdateEditor();
            })
            {
                style = { position = Position.Absolute, left = 0, right = 0, top = 0, bottom = 0, backgroundColor = transparent }
            };
            buttonField.Add(label);
            buttonField.Add(button);
            buttonActionsContainer.Add(buttonField);
        }
        
        return buttonActionsContainer;
    }

    private bool HasChanged<T>(T checkValue, T previousValue) => !checkValue.Equals(previousValue);
}
