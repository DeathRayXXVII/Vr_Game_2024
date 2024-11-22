using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using ZPTools.Utility;
using static ZPTools.Utility.UtilityFunctions;

[CustomEditor(typeof(UpgradeData), true)]
public class UpgradeDataEditor : Editor
{
    private const string StyleSheetPath = "Assets/Scripts/CustomEditor/Editor/UpdgradeDataStyleSheet.uss";
    private readonly bool allowDebug = false;
    
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
    
    private T GetValueByUIDataType<T>(UIDataType dataType, T intValue, T floatValue)
    {
        return dataType switch
        {
            UIDataType.Int => intValue,
            UIDataType.Float => floatValue,
            _ => throw new System.ArgumentOutOfRangeException(nameof(dataType), $"Unexpected UIDataType value: {dataType}")
        };
    }
    
    private VisualElement UISpacer(int height) => new() { style = { height = height } };

    private VisualElement UIGroup(string title, string containerName, bool disableHeader = false)
    {
        VisualElement groupContainer = new() { name = containerName};
        groupContainer.AddToClassList("panel");

        Label header = new(title);
        header.AddToClassList("panel-header");
        header.SetEnabled(!disableHeader);
        
        groupContainer.Add(header);
        
        return groupContainer;
    } 

    private static VisualElement UIBody(string name)
    {
        VisualElement body = new() { name = name};
        body.AddToClassList("panel-body");
        
        return body;
    }
    
    private void FocusField(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return;
        
        var field = _inspector?.Q<VisualElement>(fieldName);
        if (allowDebug) Debug.Log($"Focusing field: {fieldName}, Success: {field != null}");
        
        field?.Focus();
    }
    
    private bool _isInitializing;
    private void RefreshInspector(string focusName = "")
    {
        if (_isInitializing)
            return;
        
        _isInitializing = true;
        
        string focusedElementName;
        if (string.IsNullOrEmpty(focusName))
        {
            var currentFocus = _inspector.focusController?.focusedElement as VisualElement;
            focusedElementName = currentFocus?.name;
        }
        else if (focusName == "None")
        {
            focusedElementName = "";
        }
        else
        {
            focusedElementName = focusName;
        }
        EditorApplication.delayCall += () =>
        {
            try
            {
                // Force update to the serialized object, ensuring all fields are up to date.
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            
                // Clear and rebuild the inspector to reflect the latest changes.
                _inspector.Clear();
                _inspector.Add(BuildUI(new VisualElement { name = "InspectorRoot" }));
            }
            finally
            {
                    _isInitializing = false;
                    _inspector.MarkDirtyRepaint();
                    
                    if (!string.IsNullOrEmpty(focusedElementName))
                        FocusField(focusedElementName);
            }
        };
    }
    
    private void RedrawElement(VisualElement parentElement, VisualElement targetElement, System.Func<VisualElement> drawMethod)
    {
        if (targetElement == null)
        {
            // Debug.LogWarning("Target element is null. Cannot redraw." +
            //                  "Parent's Queryable Elements: " +
            //                  $"{string.Join(", ", parentElement.Query<VisualElement>().ToList().ConvertAll(e => e.name ?? "<unnamed>"))}");
            return;
        }

        if (!parentElement.Contains(targetElement))
        {
            Debug.LogWarning(
                $"Target element '{targetElement.name}' is not a child of the parent element '{parentElement.name}'. Cannot redraw.\n" +
                "Parent's Queryable Elements: " +
                $"{string.Join(", ", parentElement.Query<VisualElement>().ToList().ConvertAll(e => e.name ?? "<unnamed>"))}");
            return;
        }

        // Log details before removing the element
        if (allowDebug) Debug.Log($"Attempting to remove target element '{targetElement.name}' from parent '{parentElement.name}'.");

        // Store the index of the current element to reinsert the new one at the same position.
        var index = parentElement.IndexOf(targetElement);

        if (index < 0)
        {
            Debug.LogWarning($"Target element '{targetElement.name}' not found in parent element '{parentElement.name}'. Index retrieval failed." +
                             "Parent's Queryable Elements: " +
                             $"{string.Join(", ", parentElement.Query<VisualElement>().ToList().ConvertAll(e => e.name ?? "<unnamed>"))}");
            return;
        }

        // Remove and replace with the new element
        try
        {
            parentElement.Remove(targetElement);
            if (allowDebug) Debug.Log($"Removed target element '{targetElement.name}' from parent '{parentElement.name}' successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to remove target element '{targetElement.name}' from parent '{parentElement.name}'. Exception: {e.Message}");
            return;
        }

        // Draw the new panel and insert it at the original index.
        var newPanel = drawMethod.Invoke();
        if (newPanel == null)
        {
            Debug.LogError("New panel could not be created by drawMethod. Skipping insertion.");
            return;
        }
        parentElement.Insert(index, newPanel);
        if (allowDebug) Debug.Log($"Inserted new panel at index {index} for parent '{parentElement.name}'.");
    }
    
    private VisualElement _inspector;
    public override VisualElement CreateInspectorGUI()
    {
        _isInitializing = true;
        _inspector ??= new VisualElement { name = "Editor" };
        _inspector.Add(BuildUI(new VisualElement { name = "InspectorRoot" }));
            
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
        if (styleSheet != null)
        {
            _inspector.styleSheets.Add(styleSheet);
        }
        else
        {
            Debug.LogWarning($"Style sheet not found at path: {StyleSheetPath}");
        }
        
        EditorApplication.delayCall += () =>
        {
            _isInitializing = false;
        };
        return _inspector;
    }
    
    private VisualElement BuildUI(VisualElement rootElement)
    {
        upgradeData ??= (UpgradeData)target;
        
        if (JsonBlob().stringValue != null)
        {
            var jsonUpToDate = upgradeData.HashFileChangeDetector?.HasChanged();
            jsonUpToDate ??= false;
            
            var json = JsonFileProperty().objectReferenceValue as TextAsset;
            if (json != null)
            {
                Newtonsoft.Json.Linq.JObject jsonData = Newtonsoft.Json.Linq.JObject.Parse(json.text);
                bool validUpgradeKey = ValidateJsonKey(UpgradeKey().stringValue, jsonData);
                bool validCostKey = ValidateJsonKey(CostKey().stringValue, jsonData);
                jsonUpToDate = validUpgradeKey && validCostKey;
            }
            
            if (jsonUpToDate == false)
            {
                upgradeData.ForceJsonReload();
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }
        }
        
        rootElement.Add(UpperMargin());
        rootElement.Add(ReadOnlyPanel());
        rootElement.Add(UpgradeDataPanel());
        rootElement.Add(JsonDataPanel());
        rootElement.Add(ButtonActionsPanel());
        
        return rootElement;
    }
    
    private System.Func<SerializedProperty> ScriptProperty => () => serializedObject.FindProperty("m_Script");
    private System.Func<SerializedProperty> AllowDebugProperty => () => serializedObject.FindProperty("_allowDebug");
    private VisualElement UpperMargin()
    {
        VisualElement upperMargin = new() { name = "UpperMarginPanel" };
        
        // Draw the default script field
        PropertyField scriptField = new(ScriptProperty()) {name = "ScriptField"};
        scriptField.BindProperty(ScriptProperty());
        scriptField.SetEnabled(false);
        upperMargin.Add(scriptField);
        
        upperMargin.Add(UISpacer(30));
        PropertyField allowDebugField = new(AllowDebugProperty(), "Allow Debug") { name = "AllowDebugField" };
        allowDebugField.BindProperty(AllowDebugProperty());
        upperMargin.Add(allowDebugField);
        
        return upperMargin;
    }
    
    private System.Func<SerializedProperty> upgradeLevelProperty => () => serializedObject.FindProperty("_upgradeLevel");
    private bool maxLevelProperty => upgradeData.maxLevelReached;
    private object currentUpgradeProperty => upgradeData?.upgradeValue;
    private object currentCostProperty => upgradeData?.upgradeCost;
    private VisualElement ReadOnlyPanel()
    {
        var readOnlyContainer = UIGroup("Read Only", "ReadOnlyPanel", true);
        var body = UIBody("ReadOnlyBody");
        
        VisualElement maxLevelObjectField =  DrawDataObjectField("Max Level Data Holder", () => serializedObject.FindProperty("_maxLevelData"), typeof(BoolData));
        body.Add(maxLevelObjectField);
        
        VisualElement maxLevelField = DrawMaxLevelElement("Max Level");
        maxLevelField.SetEnabled(false);
        body.Add(maxLevelField);
        
        VisualElement upgradeLevelField = DrawLevelCounterElement("Upgrade Level", upgradeLevelProperty);
        if (upgradeLevelField is PropertyField propertyField)
        {
            propertyField.BindProperty(upgradeLevelProperty());
        }
        upgradeLevelField.SetEnabled(false);
        body.Add(upgradeLevelField);
        
        VisualElement currentUpgradeField =
            DrawCurrentField("Current Upgrade", upgradeTypeEnum(), currentUpgradeProperty);
        currentUpgradeField.SetEnabled(false);
        body.Add(currentUpgradeField);
        
        VisualElement currentCostField =
            DrawCurrentField("Current Cost", costTypeEnum(), currentCostProperty, true);
        currentCostField.SetEnabled(false);
        body.Add(currentCostField);
        
        readOnlyContainer.Add(body);
        return readOnlyContainer;
    }
    
    private VisualElement DrawLevelCounterElement(string label, System.Func<SerializedProperty> property)
    {        
        VisualElement levelField;
        string fieldName = $"{label.Replace(" ", "")}Field";
        
        if (JsonFileProperty().objectReferenceValue == null)
        {
            levelField = new TextField(label) { value = "—", name = fieldName };
        }
        else
        {           
            levelField = new PropertyField(property(), label) { name = fieldName };
        }
        levelField.AddToClassList("panel-field");
        
        return levelField;
    }
    
    private VisualElement DrawMaxLevelElement(string label)
    {
        var fieldName = $"{label.Replace(" ", "")}Field";
        
        VisualElement maxLevelField = new Toggle(label) { value = maxLevelProperty, name = fieldName };
        maxLevelField.AddToClassList("panel-field");
        
        return maxLevelField;
    }
    
    private VisualElement DrawCurrentField(string label, UIDataType dataType, object currentProperty,
        bool changeOnMaxLevel = false)
    {
        VisualElement currentField;
        var fieldName = $"{label.Replace(" ", "")}Field";
        
        if (currentProperty == null)
        {
            currentField = new TextField(label) { value = "null", name = fieldName };
            currentField.Q("unity-text-input")?.AddToClassList("null-field");
            currentField.AddToClassList("panel-field");
            return currentField;
        }

        if (changeOnMaxLevel && upgradeData.maxLevelReached)
        {
            currentField = new TextField(label) { value = "MAX LEVEL REACHED", name = fieldName };
            currentField.Q("unity-text-input")?.AddToClassList("max-field");
            currentField.AddToClassList("panel-field");
            return currentField;
        }
            
        switch (dataType)
        {
            case UIDataType.Float:
                if (currentProperty is float floatProperty)
                {
                    currentField = new FloatField(label) { value = floatProperty, name = fieldName };
                }
                else
                {
                    currentField = new TextField(label) { value = "null", name = fieldName };
                    currentField.Q("unity-text-input")?.AddToClassList("null-field");
                }
                break;
            
            case UIDataType.Int:
                if (currentProperty is int intProperty)
                {
                    currentField = new IntegerField(label) { value = intProperty, name = fieldName };
                }
                else
                {
                    currentField = new TextField(label) { value = "null", name = fieldName };
                    currentField.Q("unity-text-input")?.AddToClassList("null-field");
                }
                break;
            
            default:
                throw new System.ArgumentOutOfRangeException(nameof(dataType), $"Unexpected UIDataType value: {dataType}");
        }
        
        currentField.AddToClassList("panel-field");
        return currentField;
    }
    
    
    private System.Func<SerializedProperty> BaseUpgradeFloatProperty => () => serializedObject.FindProperty("_baseUpgradeFloat");
    private System.Func<SerializedProperty> BaseUpgradeIntProperty => () => serializedObject.FindProperty("_baseUpgradeInt");
    
    private System.Func<SerializedProperty> upgradeFloatProperty => () => serializedObject.FindProperty("_upgradeFloatContainer");
    private System.Func<SerializedProperty> UpgradeIntProperty => () => serializedObject.FindProperty("_upgradeIntContainer");
    
    private System.Func<SerializedProperty> CostFloatProperty => () => serializedObject.FindProperty("_costFloatContainer");
    private System.Func<SerializedProperty> CostIntProperty => () => serializedObject.FindProperty("_costIntContainer");
    private System.Func<SerializedProperty> CostListIndexProperty => () => serializedObject.FindProperty("_zeroBasedCostList");
    
    private VisualElement UpgradeDataPanel()
    {
        var upgradeDataContainer = UIGroup("Upgrade Data", "UpgradeDataPanel");
        var body = UIBody("UpgradeDataBody");
        upgradeDataContainer.Add(body);
        
        body.Add(DrawEnumField("Upgrade Data Type", UpgradeTypeProperty));
        
        var conditionalProperty = upgradeTypeEnum() == UIDataType.Float ? BaseUpgradeFloatProperty : BaseUpgradeIntProperty;
        VisualElement baseUpgradeField = DrawBaseValueField("Upgrade Base", conditionalProperty);
        body.Add(baseUpgradeField);
        
        conditionalProperty = upgradeTypeEnum() == UIDataType.Float ? upgradeFloatProperty : UpgradeIntProperty;
        var conditionalType = upgradeTypeEnum() == UIDataType.Float ? typeof(FloatData) : typeof(IntData);
        VisualElement upgradeObjectField = DrawDataObjectField("Upgrade Data Holder", conditionalProperty, conditionalType);
        body.Add(upgradeObjectField);
        
        body.Add(DrawEnumField("Cost Data Type", CostTypeProperty));
        
        conditionalProperty = costTypeEnum() == UIDataType.Float ? CostFloatProperty : CostIntProperty;
        conditionalType = costTypeEnum() == UIDataType.Float ? typeof(FloatData) : typeof(IntData);
        VisualElement costObjectField = DrawDataObjectField("Cost Data Holder", conditionalProperty, conditionalType);
        body.Add(costObjectField);
        
        Toggle costListIndexToggle = new("Zero Based Cost List?") { value = CostListIndexProperty().boolValue, name = "CostListIndexToggleField" };
        costListIndexToggle.AddToClassList("panel-field");
        if (JsonFileProperty().objectReferenceValue == null || !upgradeData.isLoaded)
            costListIndexToggle.AddToClassList("hidden");
        costListIndexToggle.RegisterValueChangedCallback(changeEvent =>
        {
            if (allowDebug) Debug.Log($"Cost List Index Toggle Changed: {HasChanged(changeEvent.newValue, CostListIndexProperty().boolValue)}\nInitializing: {_isInitializing}\nNew Value: {changeEvent.newValue}\nPrevious Value: {CostListIndexProperty().boolValue}");
            if (!HasChanged(changeEvent.newValue, CostListIndexProperty().boolValue) || _isInitializing) return;
            
            CostListIndexProperty().boolValue = changeEvent.newValue;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            
            var inspector = _inspector?.Q<VisualElement>("InspectorRoot");
            var readOnlyPanel = inspector?.Q<VisualElement>("ReadOnlyPanel");
                    
            RedrawElement(_inspector, readOnlyPanel, ReadOnlyPanel);
        });
        body.Add(costListIndexToggle);

        if (JsonFileProperty().objectReferenceValue == null || !upgradeData.isLoaded)
        {
            upgradeDataContainer.AddToClassList("hidden");
        }

        if (JsonFileProperty().objectReferenceValue == null)
        {
            BaseUpgradeFloatProperty().floatValue = 0f;
            BaseUpgradeIntProperty().intValue = 0;
            
            upgradeFloatProperty().objectReferenceValue = null;
            UpgradeIntProperty().objectReferenceValue = null;
            
            CostFloatProperty().objectReferenceValue = null;
            CostIntProperty().objectReferenceValue = null;
            
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        return upgradeDataContainer;
    }
    
    private VisualElement DrawEnumField(string label, System.Func<SerializedProperty> property)
    {
        EnumField enumField = new(label, (UIDataType)property().enumValueIndex) { name = $"{label.Replace(" ", "")}Field"};
        enumField.AddToClassList("panel-field");
        
        if (JsonFileProperty().objectReferenceValue == null || !upgradeData.isLoaded)
            enumField.AddToClassList("hidden");
        
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
            
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            
            EditorApplication.delayCall += () =>
            {
                var inspector = _inspector?.Q<VisualElement>("InspectorRoot");
                
                var upgradeDataPanelBody = inspector?.Q<VisualElement>("UpgradeDataBody");
                var baseUpgradeField = upgradeDataPanelBody?.Q<PropertyField>("UpgradeBaseField");
                var upgradeDataHolderField = upgradeDataPanelBody?.Q<ObjectField>("UpgradeDataHolderField");
                var costDataHolderField = upgradeDataPanelBody?.Q<ObjectField>("CostDataHolderField");
                
                var readOnlyPanelBody = inspector?.Q<VisualElement>("ReadOnlyBody");
                var currentUpgradeField = readOnlyPanelBody?.Q<VisualElement>("CurrentUpgradeField");
                var currentCostField = readOnlyPanelBody?.Q<VisualElement>("CurrentCostField");
                
                var JsonDataPanelBody = inspector?.Q<VisualElement>("JsonDataPanelBody");
                var loadedJsonDataField = JsonDataPanelBody?.Q<VisualElement>("JsonBlobField");
                
                // Redraw the UI as data has potentially changed.
                if (label.Contains("Upgrade"))
                {
                    // Redraw the upgrade data holder field
                    RedrawElement(upgradeDataPanelBody, upgradeDataHolderField, 
                        () => DrawDataObjectField("Upgrade Data Holder",
                            GetValueByUIDataType((UIDataType)enumField.value, UpgradeIntProperty, upgradeFloatProperty),
                            GetValueByUIDataType((UIDataType)enumField.value, typeof(IntData), typeof(FloatData))));
                    
                    // Redraw the current upgrade field
                    RedrawElement(readOnlyPanelBody, currentUpgradeField,  
                        () => DrawCurrentField("Current Upgrade", (UIDataType)enumField.value, currentUpgradeProperty));
                
                    // Redraw the upgrade data holder field
                    RedrawElement(upgradeDataPanelBody, baseUpgradeField, 
                        () => DrawBaseValueField("Upgrade Base", 
                            GetValueByUIDataType((UIDataType)enumField.value, BaseUpgradeIntProperty, BaseUpgradeFloatProperty)));
                }
                else if (label.Contains("Cost"))
                {
                    // Redraw the current cost field
                    RedrawElement(readOnlyPanelBody, currentCostField,  
                        () => DrawCurrentField("Current Cost", (UIDataType)enumField.value, currentCostProperty));
                
                    // Redraw the cost data holder field
                    RedrawElement(upgradeDataPanelBody, costDataHolderField,
                        () => DrawDataObjectField("Cost Data Holder",
                            GetValueByUIDataType((UIDataType)enumField.value, CostIntProperty, CostFloatProperty), 
                            GetValueByUIDataType((UIDataType)enumField.value, typeof(IntData), typeof(FloatData))));
                }
                
                // Redraw the loaded JSON data field
                RedrawElement(JsonDataPanelBody, loadedJsonDataField, DrawLoadedJsonData);
            };
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
        PropertyField baseValueField = new(property, label) { name = $"{label.Replace(" ", "")}Field" };
        baseValueField.BindProperty(property);
        baseValueField.AddToClassList("panel-field");
        
        if (JsonFileProperty().objectReferenceValue == null || !upgradeData.isLoaded)
        {
            if (upgradeTypeEnum() == UIDataType.Float)
                property.floatValue = 0f;
            else
                property.intValue = 0;
            serializedObject.ApplyModifiedProperties();
            
            baseValueField.AddToClassList("hidden");
        }
        
        var previousValue = upgradeTypeEnum() == UIDataType.Float ? property.floatValue : property.intValue;
        baseValueField.RegisterValueChangeCallback(changeEvent =>
        {
            var newValue = upgradeTypeEnum() == UIDataType.Float ? changeEvent.changedProperty.floatValue : changeEvent.changedProperty.intValue;
            // if (allowDebug)
            if (allowDebug) Debug.Log($"Base Value Field Changed: \"{label}\", Updating Field: {HasChanged(newValue, previousValue)}\nInitializing: {_isInitializing}\nNew Value: {newValue}\nPrevious Value: {newValue}");
            
            if (!HasChanged(newValue, previousValue) || _isInitializing) return;
            // Apply and update the serialized changes.
            upgradeData.UpdateData();
            
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            previousValue = newValue;
            
            EditorApplication.delayCall += () =>
            {
                var inspector = _inspector?.Q<VisualElement>("InspectorRoot");
                var readOnlyPanelBody = inspector?.Q<VisualElement>("ReadOnlyBody");
                var currentUpgradeField = readOnlyPanelBody?.Q<VisualElement>("CurrentUpgradeField");
                
                if (readOnlyPanelBody != null && currentUpgradeField != null)
                {
                    RedrawElement(readOnlyPanelBody, currentUpgradeField, 
                        () => DrawCurrentField("Current Upgrade", upgradeTypeEnum(), currentUpgradeProperty));
                }
                else
                {
                    Debug.LogWarning("ReadOnlyPanelBody or CurrentUpgradeField was null during delayed redraw.");
                }
            };
        });
        
        return baseValueField;
    }
    
    private ObjectField DrawDataObjectField(string label, System.Func<SerializedProperty> property, System.Type objType)
    {
        SerializedProperty sProperty = property();
        ObjectField dataObjectField = new(label)
        {
            objectType = objType,
            name = $"{label.Replace(" ", "")}Field",
        };
        if (sProperty.objectReferenceValue != null)
        {
            dataObjectField.value = sProperty.objectReferenceValue;
        }
        dataObjectField.AddToClassList("panel-field");
        
        if (JsonFileProperty().objectReferenceValue == null || !upgradeData.isLoaded)
            dataObjectField.AddToClassList("hidden");
        
        dataObjectField.BindProperty(sProperty);
        dataObjectField.RegisterValueChangedCallback(changeEvent =>
        {
            if (allowDebug) Debug.Log($"Data Object Change Event, Updating Field: {HasChanged(changeEvent.newValue, changeEvent.previousValue)}\nInitializing: {_isInitializing}\nNew Value: {changeEvent.newValue}\nPrevious Value: {changeEvent.previousValue}");
            if (!HasChanged(changeEvent.newValue, changeEvent.previousValue) || _isInitializing) return;
            sProperty.objectReferenceValue = changeEvent.newValue;

            if (sProperty.objectReferenceValue != null)
            {
                upgradeData.UpdateData();
            }
            
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
    private VisualElement JsonDataPanel()
    {
        var jsonDataContainer = UIGroup("Json Settings", "JsonDataPanel");
        var body = UIBody("JsonDataBody");
        jsonDataContainer.Add(body);

        Toggle jsonLoadedToggle = new("Json is loaded:") { value = upgradeData.isLoaded, name = "LoadedToggleField" };
        jsonLoadedToggle.SetEnabled(false);
        body.Add(jsonLoadedToggle);

        ObjectField jsonFileField = new("Json File")
        {
            objectType = typeof(TextAsset),
            name = "JsonFileField",
        };
        jsonFileField.BindProperty(JsonFileProperty());
        var previousJsonFile = JsonFileProperty().objectReferenceValue as TextAsset;
        jsonFileField.RegisterValueChangedCallback(changeEvent =>
        {
            if (allowDebug)
                Debug.Log(
                    $"Json File Change Event, Updating Field: {HasChanged(changeEvent.newValue, previousJsonFile)}\nInitializing: {_isInitializing}\nNew Value: {changeEvent.newValue}\nPrevious Value: {previousJsonFile}");
            if (!HasChanged(changeEvent.newValue, previousJsonFile) || _isInitializing) return;
            
            JsonFileProperty().objectReferenceValue = changeEvent.newValue;
            
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            // Reload JSON data from the new file
            upgradeData.ForceJsonReload();
            
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            
            RefreshInspector("None");
        });

        body.Add(jsonFileField);

        body.Add(DrawJsonKeyField("Upgrade Key", UpgradeKey, PreviousUpgradeKey, UpgradeIntProperty, upgradeFloatProperty));
        body.Add(DrawJsonKeyField("Cost Key", CostKey, PreviousCostKey, CostIntProperty, CostFloatProperty));
        
        body.Add(DrawLoadedJsonData());
        
        return jsonDataContainer;
    }
    
    private VisualElement DrawJsonKeyField(string label, System.Func<SerializedProperty> property, System.Func<SerializedProperty> previousKeyProperty,
        System.Func<SerializedProperty> intContainerProperty, System.Func<SerializedProperty> floatContainerProperty)
    {
        TextField keyField = new(label) { name = $"{label.Replace(" ", "")}Field" };
        keyField.BindProperty(property());
        keyField.AddToClassList("panel-field");
        
        if (JsonFileProperty().objectReferenceValue == null)
        {
            CostKey().stringValue = "";
            serializedObject.ApplyModifiedProperties();
            keyField.AddToClassList("hidden");
        }

        string previousKeyValue = previousKeyProperty().stringValue;
        keyField.RegisterCallback<FocusOutEvent>(_ =>
        {
            if (allowDebug)
                Debug.Log(
                    $"Focus Out Event for Cost Key Field, Updating Field: {HasChanged(keyField.value, previousKeyValue)}\nInitializing: {_isInitializing}\nNew Value: {keyField.value}\nPrevious Value: {previousKeyValue}");
            HandleKeyChange(keyField, ref previousKeyValue, previousKeyProperty, intContainerProperty, floatContainerProperty);
        });

        keyField.RegisterCallback<KeyDownEvent>(eventContext =>
        {
            if (allowDebug && eventContext.keyCode is (KeyCode.Return or KeyCode.KeypadEnter))
                Debug.Log(
                    $"Key Down Event for Cost Key Field, Updating Field: {eventContext.keyCode is (KeyCode.Return or KeyCode.KeypadEnter) || HasChanged(keyField.value, previousKeyValue)}\nInitializing: {_isInitializing}\nNew Value: {keyField.value}\nPrevious Value: {previousKeyValue}");
            if (eventContext.keyCode is not (KeyCode.Return or KeyCode.KeypadEnter)) return;
            HandleKeyChange(keyField, ref previousKeyValue, previousKeyProperty, intContainerProperty, floatContainerProperty, true);
        });
        
        return keyField;
    }

    private void HandleKeyChange(TextField newKey, ref string previousKeyValue, System.Func<SerializedProperty> previousKeyProperty,
        System.Func<SerializedProperty> intContainerProperty, System.Func<SerializedProperty> floatContainerProperty, bool keyDown = false)
    {
        if (!HasChanged(newKey.value, previousKeyValue) || _isInitializing) return;
        
        if(string.IsNullOrEmpty(newKey.value))
        {
            floatContainerProperty().objectReferenceValue = null;
            intContainerProperty().objectReferenceValue = null;
            
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
        previousKeyValue = previousKeyProperty().stringValue;

        upgradeData.ForceJsonReload();
            
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();

        switch (keyDown)
        {
            case true when newKey.name == "UpgradeKeyField" && serializedObject.FindProperty("upgradeIsLoaded").boolValue:
                RefreshInspector("CostKeyField");
                break;
            case true when newKey.name == "CostKeyField" && serializedObject.FindProperty("costIsLoaded").boolValue:
                RefreshInspector("None");
                break;
            default:
                RefreshInspector();
                break;
        }
    }

    private VisualElement DrawLoadedJsonData()
    {
        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        
        var jsonBlob = JsonBlob().stringValue;
        VisualElement loadedJsonDataContainer = new() { name = "JsonBlobField" };
        if (allowDebug) 
            Debug.Log($"Drawing Json Blob: {!string.IsNullOrEmpty(jsonBlob)}\nJson Blob: {JsonBlob().stringValue}");
        return !string.IsNullOrEmpty(jsonBlob) ?
            DrawJsonBlob(loadedJsonDataContainer, jsonBlob) :
            DrawJsonHelpBox(loadedJsonDataContainer, JsonFileProperty, UpgradeKey, CostKey);
    }
    
    private static VisualElement DrawJsonBlob(VisualElement containerElement, string jsonBlob)
    {
        Label containerLabel = new("Loaded Values From JSON");
        containerLabel.AddToClassList("bold");
        containerElement.Add(containerLabel);

        ScrollView scrollView = new(ScrollViewMode.Vertical) { horizontalScrollerVisibility = ScrollerVisibility.Hidden };
        scrollView.AddToClassList("scroll-view");
        
        TextField jsonBlobField = new()
        {
            value = jsonBlob, 
            multiline = true,
            isReadOnly = true,
            name = "JsonBlobText"
        };
        jsonBlobField.Q("unity-text-input").AddToClassList("text-blob");
        scrollView.Add(jsonBlobField);
            
        containerElement.Add(scrollView);
        
        return containerElement;
    }
    
    private VisualElement DrawJsonHelpBox(VisualElement containerElement, System.Func<SerializedProperty> jsonProperty,
        System.Func<SerializedProperty> upgradeKeyProp, System.Func<SerializedProperty> costKeyProp)
    {
        string helpText = "No JSON data loaded, due to:";
        string keyText = "";
        var json = jsonProperty().objectReferenceValue as TextAsset;
        
        if (json == null || jsonProperty().propertyType != SerializedPropertyType.ObjectReference || jsonProperty().objectReferenceValue == null)
        {
            helpText += "\n\t- No JSON file selected.";
        }
        else 
        {
            Newtonsoft.Json.Linq.JObject jsonData = Newtonsoft.Json.Linq.JObject.Parse(json.text);
            bool validUpgradeKey = ValidateJsonKey(upgradeKeyProp().stringValue, jsonData);
            bool validCostKey = ValidateJsonKey(costKeyProp().stringValue, jsonData);
            if (!validUpgradeKey) helpText += "\n\t- Upgrade Key is not in JSON file.";
            if (!validCostKey) helpText += "\n\t- Cost Key is not in JSON file.";
            if (!validUpgradeKey || !validCostKey) keyText = $"Possible keys: \n   {string.Join("\n   ", GetJsonKeys(jsonData))}";
        }
        HelpBox noJsonDataLabel = new(helpText, HelpBoxMessageType.Error);
        containerElement.Add(noJsonDataLabel);
        if (!string.IsNullOrEmpty(keyText))
            containerElement.Add(new HelpBox(keyText, HelpBoxMessageType.Info));
        
        return containerElement;
    }
    
    private VisualElement ButtonActionsPanel()
    {
        var buttonActionsContainer = UIGroup("Button Actions", "ButtonActionsPanel");
        System.Collections.Generic.List<(System.Action, string)> buttonActions = upgradeData.GetButtonActions();
        foreach (var buttonAction in buttonActions)
        {
            VisualElement buttonField = new();
            buttonField.AddToClassList("button-element");
            
            Label label = new(buttonAction.Item2);
            label.AddToClassList("button-label");
            
            Button button;
            if (buttonAction.Item2.Contains("Update"))
            {
                button = new Button(() =>
                {
                    buttonAction.Item1();
                    
                    var inspectorRoot = _inspector?.Q<VisualElement>("InspectorRoot");
                    var readOnlyPanel = inspectorRoot?.Q<VisualElement>("ReadOnlyPanel");
                    var jsonDataPanelBody = inspectorRoot?.Q<VisualElement>("JsonDataBody");
                    var loadedJsonDataField = jsonDataPanelBody?.Q<VisualElement>("JsonBlobField");
                    
                    RedrawElement(inspectorRoot, readOnlyPanel, ReadOnlyPanel);
                    RedrawElement(jsonDataPanelBody, loadedJsonDataField, DrawLoadedJsonData);
                });
            }
            else
            {
                button = new Button(() =>
                {
                    buttonAction.Item1();
                    
                    var inspectorRoot = _inspector?.Q<VisualElement>("InspectorRoot");
                    var readOnlyPanel = inspectorRoot?.Q<VisualElement>("ReadOnlyPanel");
                    
                    RedrawElement(inspectorRoot, readOnlyPanel, ReadOnlyPanel);
                });
                if (JsonFileProperty().objectReferenceValue == null || !upgradeData.isLoaded)
                    button.SetEnabled(false);
            }
            button.AddToClassList("button");
            buttonField.Add(button);
            button.Add(label);
            
            buttonActionsContainer.Add(buttonField);
        }
        if (JsonFileProperty().objectReferenceValue == null)
            buttonActionsContainer.AddToClassList("hidden");
        
        return buttonActionsContainer;
    }
}
