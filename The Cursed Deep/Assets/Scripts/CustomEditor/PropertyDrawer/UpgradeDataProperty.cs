// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEditor.UIElements;
// using UnityEngine.UIElements;
// using UnityEngine;
// using ZPTools;
// using ZPTools.Interface;
//
// [CustomPropertyDrawer(typeof(UpgradeData), true)]
// public class UpgradeDataEditor : PropertyDrawer
// {
//     // private string _lastFocusedControl = "";
//     private Vector2 blobScrollPosition = Vector2.zero;
//
//     private void GUILine(VisualElement container)
//     {
//         var line = new VisualElement
//         {
//             style =
//             {
//                 height = 1,
//                 backgroundColor = new Color(0.3f, 0.3f, 0.3f)
//             }
//         };
//         container.Add(line);
//     }
//
//     private bool HasChanged<T>(T checkValue, T previousValue) => !checkValue.Equals(previousValue);
//
//     private T PerformActionOnValueChange<T>(T checkValue, T previousValue, SerializedProperty property, System.Action action, bool allowDebug = false)
//     {
//         if (!HasChanged(checkValue, previousValue)) return previousValue;
//         if (allowDebug) Debug.Log($"Value changed from {previousValue} to {checkValue}", property.serializedObject.targetObject);
//         action();
//         return checkValue;
//     }
//
//     public override VisualElement CreatePropertyGUI(SerializedProperty property)
//     {
//         var inspector = new VisualElement();
//
//         Color darkGrey = new Color(0.3f, 0.3f, 0.3f);
//         Color darkerGrey = new Color(0.15f, 0.15f, 0.15f);
//         Color darkGreen = new Color(0.3f, 0.6f, 0.3f);
//         
//         SerializedObject obj = property.serializedObject;
//         UpgradeData upgradeData = (UpgradeData)property.objectReferenceValue;
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
//         // var debugString = "";
//
//         // Draw the default script field
//         var scriptField = new PropertyField(scriptProperty);
//         scriptField.SetEnabled(false);
//         inspector.Add(scriptField);
//         
//         var spaceSmall = new VisualElement
//         {
//             style =
//             {
//                 height = 5
//             }
//         };
//         inspector.Add(spaceSmall);
//
//         // var allowDebugField = new PropertyField(allowDebugProperty, "Allow Debug");
//         // inspector.Add(allowDebugField);
//
//         var readOnlyContainer = new VisualElement();
//         readOnlyContainer.style.backgroundColor = darkGrey;
//         readOnlyContainer.Add(new Label("Read Only") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
//         inspector.Add(readOnlyContainer);
//
//         var upgradeLevelField = new IntegerField("Upgrade Level") { value = upgradeLevelProperty };
//         upgradeLevelField.SetEnabled(false);
//         readOnlyContainer.Add(upgradeLevelField);
//
//         switch (upgradeTypeEnum)
//         {
//             case DataType.Float:
//                 var currentUpgradeFloatField = new FloatField("Current Upgrade") { value = (float)currentUpgradeProperty };
//                 currentUpgradeFloatField.SetEnabled(false);
//                 readOnlyContainer.Add(currentUpgradeFloatField);
//                 break;
//             case DataType.Int:
//                 var currentUpgradeIntField = new IntegerField("Current Upgrade") { value = (int)currentUpgradeProperty };
//                 currentUpgradeIntField.SetEnabled(false);
//                 readOnlyContainer.Add(currentUpgradeIntField);
//                 break;
//         }
//
//         try
//         {
//             switch (costTypeEnum)
//             {
//                 case DataType.Float:
//                     var currentCostFloatField = new FloatField("Current Cost") { value = (float)currentCostProperty };
//                     currentCostFloatField.SetEnabled(false);
//                     readOnlyContainer.Add(currentCostFloatField);
//                     break;
//                 case DataType.Int:
//                     var currentCostIntField = new IntegerField("Current Cost") { value = (int)currentCostProperty };
//                     currentCostIntField.SetEnabled(false);
//                     readOnlyContainer.Add(currentCostIntField);
//                     break;
//             }
//         }
//         catch (System.InvalidCastException)
//         {
//             var currentType = costTypeEnum;
//             costTypeProperty.enumValueIndex = costTypeEnum == DataType.Float ? (int)DataType.Int : (int)DataType.Float;
//             costTypeEnum = (DataType)costTypeProperty.enumValueIndex;
//             Debug.LogWarning($"Invalid expected type: {currentType}, attempting to switch type to {costTypeEnum}.",
//                 property.serializedObject.targetObject);
//             obj.ApplyModifiedProperties();
//
//             switch (costTypeEnum)
//             {
//                 case DataType.Float:
//                     var currentCostFloatField = new FloatField("Current Cost") { value = (float)currentCostProperty };
//                     currentCostFloatField.SetEnabled(false);
//                     readOnlyContainer.Add(currentCostFloatField);
//                     break;
//                 case DataType.Int:
//                     var currentCostIntField = new IntegerField("Current Cost") { value = (int)currentCostProperty };
//                     currentCostIntField.SetEnabled(false);
//                     readOnlyContainer.Add(currentCostIntField);
//                     break;
//             }
//         }
//
//         inspector.Add(spaceSmall);
//
//         var upgradeDataContainer = new VisualElement();
//         upgradeDataContainer.style.backgroundColor = darkGrey;
//         upgradeDataContainer.Add(new Label("Upgrade Data") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
//         inspector.Add(upgradeDataContainer);
//
//         var upgradeTypeField = new EnumField("Upgrade Data Type", upgradeTypeEnum);
//         upgradeTypeField.RegisterValueChangedCallback(evt =>
//         {
//             upgradeTypeProperty.enumValueIndex = (int)(DataType)evt.newValue;
//             upgradeTypeEnum = (DataType)upgradeTypeProperty.enumValueIndex;
//             obj.ApplyModifiedProperties();
//         });
//         upgradeDataContainer.Add(upgradeTypeField);
//
//         var costTypeField = new EnumField("Cost Data Type", costTypeEnum);
//         costTypeField.RegisterValueChangedCallback(evt =>
//         {
//             costTypeProperty.enumValueIndex = (int)(DataType)evt.newValue;
//             costTypeEnum = (DataType)costTypeProperty.enumValueIndex;
//             obj.ApplyModifiedProperties();
//         });
//         upgradeDataContainer.Add(costTypeField);
//
//         switch (upgradeTypeEnum)
//         {
//             case DataType.Float:
//                 var baseValueFloatField = new PropertyField(baseFloatProperty, "Base Value");
//                 upgradeDataContainer.Add(baseValueFloatField);
//
//                 var upgradeFloatDataField = new ObjectField("Upgrade Holder") { objectType = typeof(FloatData), value = baseUpgradeFloatProperty.objectReferenceValue };
//                 upgradeFloatDataField.RegisterValueChangedCallback(evt =>
//                 {
//                     baseUpgradeFloatProperty.objectReferenceValue = evt.newValue;
//                     obj.ApplyModifiedProperties();
//                 });
//                 upgradeDataContainer.Add(upgradeFloatDataField);
//                 break;
//
//             case DataType.Int:
//                 var baseValueIntField = new PropertyField(baseUpgradeIntProperty, "Base Value");
//                 upgradeDataContainer.Add(baseValueIntField);
//
//                 var upgradeIntDataField = new ObjectField("Upgrade Holder") { objectType = typeof(IntData), value = upgradeIntProperty.objectReferenceValue };
//                 upgradeIntDataField.RegisterValueChangedCallback(evt =>
//                 {
//                     upgradeIntProperty.objectReferenceValue = evt.newValue;
//                     obj.ApplyModifiedProperties();
//                 });
//                 upgradeDataContainer.Add(upgradeIntDataField);
//                 break;
//         }
//
//         switch (costTypeEnum)
//         {
//             case DataType.Float:
//                 var costFloatDataField = new ObjectField("Cost Holder") { objectType = typeof(FloatData), value = costValueFloatProperty.objectReferenceValue };
//                 costFloatDataField.RegisterValueChangedCallback(evt =>
//                 {
//                     costValueFloatProperty.objectReferenceValue = evt.newValue;
//                     obj.ApplyModifiedProperties();
//                 });
//                 upgradeDataContainer.Add(costFloatDataField);
//                 break;
//
//             case DataType.Int:
//                 var costIntDataField = new ObjectField("Cost Holder") { objectType = typeof(IntData), value = costValueIntProperty.objectReferenceValue };
//                 costIntDataField.RegisterValueChangedCallback(evt =>
//                 {
//                     costValueIntProperty.objectReferenceValue = evt.newValue;
//                     obj.ApplyModifiedProperties();
//                 });
//                 upgradeDataContainer.Add(costIntDataField);
//                 break;
//         }
//
//         inspector.Add(spaceSmall);
//
//         var jsonSettingsContainer = new VisualElement();
//         jsonSettingsContainer.style.backgroundColor = darkGrey;
//         jsonSettingsContainer.Add(new Label("Json Settings") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
//         inspector.Add(jsonSettingsContainer);
//
//         var jsonLoadedToggle = new Toggle("Json is loaded:") { value = jsonLoadState };
//         jsonLoadedToggle.SetEnabled(false);
//         jsonSettingsContainer.Add(jsonLoadedToggle);
//
//         var jsonFileField = new PropertyField(jsonFileProperty, "Json File");
//         jsonSettingsContainer.Add(jsonFileField);
//
//         var valueKeyField = new PropertyField(valueKeyProperty, "Value Key");
//         jsonSettingsContainer.Add(valueKeyField);
//
//         var costKeyField = new PropertyField(costKeyProperty, "Cost Key");
//         jsonSettingsContainer.Add(costKeyField);
//
//         if (!string.IsNullOrEmpty(jsonBlob.stringValue) || blobNeedsUpdate)
//         {
//             var loadedValuesContainer = new VisualElement();
//             loadedValuesContainer.style.backgroundColor = darkGrey;
//             loadedValuesContainer.Add(new Label("Loaded Values") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
//             jsonSettingsContainer.Add(loadedValuesContainer);
//
//             var jsonBlobField = new TextField { value = jsonBlob.stringValue, multiline = true };
//             jsonBlobField.SetEnabled(false);
//             loadedValuesContainer.Add(jsonBlobField);
//         }
//         else
//         {
//             var noJsonDataLabel = new Label("No JSON data loaded.") { style = { unityFontStyleAndWeight = FontStyle.Italic } };
//             jsonSettingsContainer.Add(noJsonDataLabel);
//         }
//
//         return inspector;
//     }
// }
// #endif