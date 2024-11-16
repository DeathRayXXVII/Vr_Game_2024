using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static ZPTools.Utility.UtilityFunctions;

[CustomEditor(typeof(UpgradeData), true)]
public class UpgradeDataEditor : Editor
{
    private const string StyleSheetPath = "Assets/Scripts/CustomEditor/Editor/UpdgradeDataStyleSheet.uss";
    private enum UIDataType
    {
        Int,
        Float
    }
    
    UpgradeData upgradeData;
    private System.Func<SerializedProperty> UpgradeTypeProperty => () => serializedObject.FindProperty("_upgradeDataType");
    private System.Func<SerializedProperty> CostTypeProperty => () => serializedObject.FindProperty("_costDataType");
    private System.Func<UIDataType> upgradeTypeEnum => () => (UIDataType)UpgradeTypeProperty().enumValueIndex;
    private System.Func<UIDataType> costTypeEnum => () => (UIDataType)CostTypeProperty().enumValueIndex;
    private bool allowDebug => serializedObject.FindProperty("_allowDebug").boolValue;
    
    private static bool HasChanged<T>(T checkValue, T previousValue)
    {
        var checkIsNull = checkValue == null;
        var previousIsNull = previousValue == null;
        // If both are null, they are equal (not changed).
        if (checkIsNull && previousIsNull) return false;

        // At least one is not null and if one is null and the other is not, they are not equal (changed).
        if (checkIsNull || previousIsNull) return true;

        // Both are not null, so compare the values.
        return !checkValue.Equals(previousValue);
    }
    
    private VisualElement UISpacer(int height) => new() { style = { height = height } };

    private VisualElement UIGroup(string title)
    {
        var groupContainer = new VisualElement();
        groupContainer.AddToClassList("panel");

        var header = new Label(title);
        header.AddToClassList("panel-header");
        
        groupContainer.Add(header);
        
        return groupContainer;
    } 

    private static VisualElement UIBody()
    {
        var body = new VisualElement();
        body.AddToClassList("panel-body");
        
        return body;
    }
    
    private bool _isInitializing;
    private void RefreshInspector(VisualElement rootElement)
    {
        if (_isInitializing)
            return;
        
        _isInitializing = true;
        
        try
        {
            // Force update to the serialized object, ensuring all fields are up to date.
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        
            // Clear and rebuild the inspector to reflect the latest changes.
            var refreshedGui = CreateInspectorGUI();
            rootElement.Clear();
            rootElement.Add(refreshedGui);
        }
        finally
        {
            _isInitializing = false;
        }
    }
    
    private void RedrawElement(VisualElement parentElement, VisualElement targetElement, System.Func<VisualElement, VisualElement> drawMethod)
    {
        if (targetElement == null)
        {
            Debug.LogWarning($"Target element is null. Cannot redraw.");
            return;
        }

        if (!parentElement.Contains(targetElement))
        {
            Debug.LogWarning($"Target element {targetElement.name} is not a child of the parent element {parentElement.name}. Cannot redraw.");
            return;
        }

        // Store the index of the current element to reinsert the new one at the same position.
        int index = parentElement.IndexOf(targetElement);
        parentElement.Remove(targetElement);

        // Draw the new panel and insert it at the original index.
        var newPanel = drawMethod.Invoke(parentElement);
        parentElement.Insert(index, newPanel);

    }

    private VisualElement _inspector;
    public override VisualElement CreateInspectorGUI()
    {
        _isInitializing = true;
        var inspector = new VisualElement();
        _inspector = inspector;
        
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
        if (styleSheet != null)
        {
            inspector.styleSheets.Add(styleSheet);
        }
        else
        {
            Debug.LogWarning($"Style sheet not found at path: {StyleSheetPath}");
        }
        inspector = BuildUI(inspector);
        _isInitializing = false;
        return inspector;
    }

    private VisualElement _upperMarginPanel;
    private VisualElement _readOnlyPanel;
    private VisualElement _upgradeDataPanel;
    private VisualElement _jsonDataPanel;
    private VisualElement _buttonActionsPanel;
    
    private VisualElement BuildUI(VisualElement rootElement)
    {
        upgradeData ??= (UpgradeData)target;
        _upperMarginPanel = UpperMargin(rootElement);
        _readOnlyPanel = ReadOnlyPanel(rootElement);
        _upgradeDataPanel = UpgradeDataPanel(rootElement);
        _jsonDataPanel = JsonDataPanel(rootElement);
        _buttonActionsPanel = ButtonActionsPanel(rootElement);
        
        
        rootElement.Add(_upperMarginPanel);
        rootElement.Add(_readOnlyPanel);
        rootElement.Add(_upgradeDataPanel);
        rootElement.Add(_jsonDataPanel);
        rootElement.Add(_buttonActionsPanel);
        
        return rootElement;
    }
    
    
    private System.Func<SerializedProperty> ScriptProperty => () => serializedObject.FindProperty("m_Script");
    private System.Func<SerializedProperty> AllowDebugProperty => () => serializedObject.FindProperty("_allowDebug");
    private VisualElement UpperMargin(VisualElement rootElement)
    {
        var upperMargin = new VisualElement();
        
        // Draw the default script field
        var scriptField = new PropertyField(ScriptProperty());
        scriptField.BindProperty(ScriptProperty());
        scriptField.SetEnabled(false);
        upperMargin.Add(scriptField);
        
        upperMargin.Add(UISpacer(30));
        var allowDebugField = new PropertyField(AllowDebugProperty(), "Allow Debug");
        allowDebugField.BindProperty(AllowDebugProperty());
        upperMargin.Add(allowDebugField);
        
        return upperMargin;
    }
    
    
    private int upgradeLevelProperty => upgradeData.upgradeLevel;
    private object currentUpgradeProperty => upgradeData.upgradeValue;
    private object currentCostProperty => upgradeData.upgradeCost;
    private VisualElement ReadOnlyPanel(VisualElement rootElement)
    {
        var readOnlyContainer = UIGroup("Read Only");
        var body = UIBody();
        readOnlyContainer.Add(body);

        var upgradeLevelField = new IntegerField("Upgrade Level") { value = upgradeLevelProperty };
        upgradeLevelField.BindProperty(serializedObject.FindProperty("_upgradeLevel"));
        body.Add(upgradeLevelField);
        
        body.Add(DrawCurrentField("Current Upgrade", upgradeTypeEnum(), currentUpgradeProperty));
        body.Add(DrawCurrentField("Current Cost", costTypeEnum(), currentCostProperty));
        readOnlyContainer.SetEnabled(false);
        
        return readOnlyContainer;
    }
    
    private static VisualElement DrawCurrentField(string label, UIDataType dataType, object currentProperty)
    {
        VisualElement currentField;
        if (currentProperty == null)
        {
            Debug.LogWarning($"Current property is null for field '{label}'. Defaulting to 'null' text field.");
            return new TextField(label) { value = "null" };
        }
        Debug.Log($"Current Property Type: {currentProperty.GetType()}");
        switch (dataType)
        {
            case UIDataType.Float:
                if (currentProperty is float floatProperty)
                {
                    currentField = new FloatField(label) { value = floatProperty };
                }
                else
                {
                    currentField = new TextField(label) { value = "null" };
                }

                break;
            case UIDataType.Int:
                if (currentProperty is int intProperty)
                {
                    currentField = new IntegerField(label) { value = intProperty };
                }
                else
                {
                    currentField = new TextField(label) { value = "null" };
                }

                break;
            default:
                throw new System.ArgumentOutOfRangeException();
        }
        return currentField;
    }
    
    
    private System.Func<SerializedProperty> BaseUpgradeFloatProperty => () => serializedObject.FindProperty("_baseUpgradeFloat");
    private System.Func<SerializedProperty> BaseUpgradeIntProperty => () => serializedObject.FindProperty("_baseUpgradeInt");
    
    private System.Func<SerializedProperty> upgradeFloatProperty => () => serializedObject.FindProperty("_upgradeFloatContainer");
    private System.Func<SerializedProperty> UpgradeIntProperty => () => serializedObject.FindProperty("_upgradeIntContainer");
    
    private System.Func<SerializedProperty> CostFloatProperty => () => serializedObject.FindProperty("_costFloatContainer");
    private System.Func<SerializedProperty> CostIntProperty => () => serializedObject.FindProperty("_costIntContainer");
    private VisualElement UpgradeDataPanel(VisualElement rootElement)
    {
        var upgradeDataContainer = UIGroup("Upgrade Data");
        var body = UIBody();
        upgradeDataContainer.Add(body);
        
        var upgradeFieldGroup = new VisualElement();
        
        body.Add(DrawEnumField("Upgrade Data Type", UpgradeTypeProperty));
        
        PropertyField baseUpgradeField = null;
        ObjectField upgradeObjectField = null;
        
        switch (upgradeTypeEnum())
        {
            case UIDataType.Float:
                baseUpgradeField = DrawBaseValueField("Upgrade Base", BaseUpgradeFloatProperty);
                upgradeObjectField = DrawDataObjectField("Upgrade Data Holder", upgradeFloatProperty, typeof(FloatData));
                break;
            case UIDataType.Int:
                baseUpgradeField = DrawBaseValueField("Upgrade Base", BaseUpgradeIntProperty);
                upgradeObjectField = DrawDataObjectField("Upgrade Data Holder", UpgradeIntProperty, typeof(IntData));
                break;
        }
        body.Add(baseUpgradeField);
        body.Add(upgradeObjectField);
        
        body.Add(DrawEnumField("Cost Data Type", CostTypeProperty));
        
        ObjectField costObjectField = null;
        
        switch (costTypeEnum())
        {
            case UIDataType.Float:
                costObjectField = DrawDataObjectField("Cost Data Holder", CostFloatProperty, typeof(FloatData));
                break;
            case UIDataType.Int:
                costObjectField = DrawDataObjectField("Cost Data Holder", CostIntProperty, typeof(IntData));
                break;
        }

        if (costObjectField != null)
        {
            costObjectField.BindProperty(costTypeEnum() == UIDataType.Float ? CostFloatProperty() : CostIntProperty()); // Rebind ObjectField
            costObjectField.AddToClassList("panel-field");
            body.Add(costObjectField);
        }
        
        return upgradeDataContainer;
    }
    
    private VisualElement DrawEnumField(string label, System.Func<SerializedProperty> property)
    {
        var enumField = new EnumField(label, (UIDataType)property().enumValueIndex);
        enumField.AddToClassList("panel-field");
        
        enumField.RegisterValueChangedCallback(changeEvent =>
        {
            if (allowDebug) Debug.Log($"Enum Change Event for '{label}', Updating Field: {HasChanged((int)(UIDataType)changeEvent.newValue, property().enumValueIndex)}\nInitializing: {_isInitializing}\nNew Value: {changeEvent.newValue}\nPrevious Value: {(UIDataType)property().enumValueIndex}");
            if (!HasChanged((int)(UIDataType)changeEvent.newValue, property().enumValueIndex) || _isInitializing) return;
            
            // Update the SerializedProperty to reflect the new value.
            property().enumValueIndex = (int)(UIDataType)changeEvent.newValue;
            enumField.value = (UIDataType)property().enumValueIndex;

            // Apply and update serialized changes.
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            // Reload the JSON data from the new file.
            upgradeData.ForceJsonReload();

            // Rebuild the UI most data has potentially changed.
            RefreshInspector(_inspector);
        });
        return enumField;
    }
    
    private PropertyField DrawBaseValueField(string label, System.Func<SerializedProperty> propertyFunc)
    {
        // Get the SerializedProperty using the provided function.
        SerializedProperty property = propertyFunc();
        if (property == null)
        {
            Debug.LogWarning($"Serialized property '{label}' is null. Cannot draw the field.");
            return null;
        }

        // Create a PropertyField for the SerializedProperty.
        PropertyField baseValueField = new PropertyField(property, label);
        baseValueField.BindProperty(property); 
        baseValueField.AddToClassList("panel-field");
        
        var previousValue = property.floatValue;
        baseValueField.RegisterValueChangeCallback(changeEvent =>
        {
            if (allowDebug)
            { Debug.Log($"Base Value Field Changed: {label}\nNew Value: {changeEvent.changedProperty.floatValue}\nPrevious Value: {changeEvent.changedProperty.floatValue}"); }
            
            if (!HasChanged(changeEvent.changedProperty.floatValue, previousValue) || _isInitializing) return;
            // Apply and update the serialized changes.
            serializedObject.ApplyModifiedProperties();
            previousValue = changeEvent.changedProperty.floatValue;
            
            RefreshInspector(_inspector);
        });

        return baseValueField;
    }

    
    private ObjectField DrawDataObjectField(string label, System.Func<SerializedProperty> property, System.Type objType)
    {
        var dataObjectField = new ObjectField(label)
        {
            objectType = objType
        };
        if (property().objectReferenceValue != null)
        {
            dataObjectField.value = property().objectReferenceValue;
        }
        dataObjectField.AddToClassList("panel-field");
        dataObjectField.RegisterValueChangedCallback(changeEvent =>
        {
            if (allowDebug) Debug.Log($"Event4, Updating Field: {HasChanged(changeEvent.newValue, changeEvent.previousValue)}\nInitializing: {_isInitializing}\nNew Value: {changeEvent.newValue}\nPrevious Value: {changeEvent.previousValue}");
            if (!HasChanged(changeEvent.newValue, changeEvent.previousValue) || _isInitializing) return;
            property().objectReferenceValue = changeEvent.newValue;
            serializedObject.ApplyModifiedProperties();
        });
        return dataObjectField;
    }
    
    private System.Func<SerializedProperty> JsonFileProperty => () => serializedObject.FindProperty("_jsonFile");
    private System.Func<SerializedProperty> UpgradeKey => () => serializedObject.FindProperty("_upgradeKey");
    private System.Func<SerializedProperty> PreviousUpgradeKey => () => serializedObject.FindProperty("_previousUpgradeKey");
    private System.Func<SerializedProperty> CostKey => () => serializedObject.FindProperty("_costKey");
    private System.Func<SerializedProperty> PreviousCostKey => () => serializedObject.FindProperty("_previousCostKey");
    private System.Func<SerializedProperty> JsonBlob => () => serializedObject.FindProperty("_jsonBlob");
    private System.Func<SerializedProperty> upgradeIsLoaded => () => serializedObject.FindProperty("upgradeIsLoaded");
    private System.Func<SerializedProperty> costIsLoaded => () => serializedObject.FindProperty("upgradeIsLoaded");
    bool jsonLoadState => upgradeData.isLoaded;
    private VisualElement JsonDataPanel(VisualElement rootElement)
    {
        var jsonSettingsContainer = UIGroup("Json Settings");
        
        var body = UIBody();
        jsonSettingsContainer.Add(body);

        var jsonLoadedToggle = new Toggle("Json is loaded:") { value = jsonLoadState };
        jsonLoadedToggle.SetEnabled(false);
        body.Add(jsonLoadedToggle);

        ObjectField jsonFileField = new ObjectField("Json File")
        {
            objectType = typeof(TextAsset)
        };
        jsonFileField.BindProperty(JsonFileProperty());
        var previousJsonFile = JsonFileProperty().objectReferenceValue as TextAsset;
        jsonFileField.RegisterValueChangedCallback(changeEvent =>
        {
            if (allowDebug) Debug.Log($"Event7, Updating Field: {HasChanged(changeEvent.newValue, previousJsonFile)}\nInitializing: {_isInitializing}\nNew Value: {changeEvent.newValue}\nPrevious Value: {previousJsonFile}");
            if (!HasChanged(changeEvent.newValue, previousJsonFile) || _isInitializing) return;

            // Reload JSON data from the new file
            upgradeData.ForceJsonReload();

            // Rebuild the UI as data has potentially changed
            RefreshInspector(rootElement);
        });
        body.Add(jsonFileField);

        var valueKeyField = new TextField("Upgrade Key");
        valueKeyField.BindProperty(UpgradeKey());
        var previousUpgradeKeyValue = PreviousUpgradeKey().stringValue;

        // Register focus loss event
        valueKeyField.RegisterCallback<FocusOutEvent>(eventContext =>
        {
            if (!HasChanged(valueKeyField.value, previousUpgradeKeyValue) || _isInitializing) return;

            previousUpgradeKeyValue = valueKeyField.value;
            upgradeData.ForceJsonReload();
            RefreshInspector(rootElement);
        });

        // Register Enter key press event
        valueKeyField.RegisterCallback<KeyDownEvent>(eventContext =>
        {
            if (eventContext.keyCode != KeyCode.Return && eventContext.keyCode != KeyCode.KeypadEnter || _isInitializing) return;
            if (!HasChanged(valueKeyField.value, previousUpgradeKeyValue)) return;

            previousUpgradeKeyValue = valueKeyField.value;
            upgradeData.ForceJsonReload();
            RefreshInspector(rootElement);
        });
        body.Add(valueKeyField);

        var costKeyField = new TextField("Cost Key");
        costKeyField.BindProperty(CostKey());
        var previousCostKeyValue = PreviousCostKey().stringValue;
        
        costKeyField.RegisterCallback<FocusOutEvent>(eventContext =>
        {
            if (!HasChanged(costKeyField.value, previousCostKeyValue) || _isInitializing) return;

            previousCostKeyValue = costKeyField.value;
            upgradeData.ForceJsonReload();
            serializedObject.ApplyModifiedProperties();
        });

        costKeyField.RegisterCallback<KeyDownEvent>(eventContext =>
        {
            if (eventContext.keyCode is not (KeyCode.Return or KeyCode.KeypadEnter) ||
                !HasChanged(costKeyField.value, previousCostKeyValue) || _isInitializing) return;
            previousCostKeyValue = costKeyField.value;
            upgradeData.ForceJsonReload();
            serializedObject.ApplyModifiedProperties();
        });
        body.Add(costKeyField);
        
        var jsonBlob = JsonBlob().stringValue;
        if (!string.IsNullOrEmpty(jsonBlob))
        {
            var loadedValuesContainer = new VisualElement();
            loadedValuesContainer.AddToClassList("default-element");
            
            var containerLabel = new Label("Loaded Values");
            containerLabel.AddToClassList("bold");
            loadedValuesContainer.Add(containerLabel);
            
            body.Add(loadedValuesContainer);

            var jsonBlobField = new TextField()
            {
                value = jsonBlob, 
                multiline = true,
                isReadOnly = true,
            };
            jsonBlobField.AddToClassList("text-blob");

            var scrollView = new ScrollView(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
            scrollView.AddToClassList("scroll-view");
            scrollView.Add(jsonBlobField);
            
            loadedValuesContainer.Add(scrollView);
        }
        else
        {
            DrawJsonHelpBox(jsonSettingsContainer, JsonFileProperty(), UpgradeKey(), CostKey());
        }
        
        return jsonSettingsContainer;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="containerElement"></param>
    /// <param name="jsonProperty"></param>
    /// <param name="upgradeKeyProp"></param>
    /// <param name="costKeyProp"></param>
    private void DrawJsonHelpBox(VisualElement containerElement, SerializedProperty jsonProperty, SerializedProperty upgradeKeyProp, SerializedProperty costKeyProp)
    {
        string helpText = "No JSON data loaded, due to:";
        
        if (jsonProperty == null || jsonProperty.propertyType != SerializedPropertyType.ObjectReference || jsonProperty.objectReferenceValue == null)
        {
            helpText += "\n\t- No JSON file selected.";
        }
        else 
        {
            if (!ValidateJsonKey(upgradeKeyProp.stringValue, upgradeData.jsonData)) helpText += "\n\t- Upgrade Key is not in JSON file.";
            if (!ValidateJsonKey(costKeyProp.stringValue, upgradeData.jsonData)) helpText += "\n\t- Cost Key is not in JSON file.";
        }
            
        HelpBox noJsonDataLabel = new HelpBox(helpText, HelpBoxMessageType.Info);
        containerElement.Add(noJsonDataLabel);
    }
    
    private VisualElement ButtonActionsPanel(VisualElement rootElement)
    {
        var buttonActionsContainer = UIGroup("Button Actions");
        System.Collections.Generic.List<(System.Action, string)> buttonActions = upgradeData.GetButtonActions();
        foreach (var buttonAction in buttonActions)
        {
            var buttonField = new VisualElement();
            buttonField.AddToClassList("button-element");
            
            var label = new Label(buttonAction.Item2);
            label.AddToClassList("button-label");
            
            Button button;
            if (buttonAction.Item2.Contains("Update"))
            {
                button = new Button(() =>
                {
                    buttonAction.Item1();
                    RedrawElement(_inspector, _jsonDataPanel, JsonDataPanel);
                });
            }
            else
            {
                button = new Button(() => buttonAction.Item1());
            }
            button.AddToClassList("button");
            buttonField.Add(button);
            button.Add(label);
            
            buttonActionsContainer.Add(buttonField);
        }
        
        return buttonActionsContainer;
    }
}
