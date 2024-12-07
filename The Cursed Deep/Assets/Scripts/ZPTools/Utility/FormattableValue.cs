using UnityEngine;

namespace ZPTools.Utility
{
    [System.Serializable]
    public class FormattableValue
    {
        public enum ValueType { String, Int, IntData, Float, FloatData, Bool }
        
        public ValueType valueType;

        [SerializeField] private string stringValue;
        [SerializeField] private int intValue;
        [SerializeField] private IntData intDataValue;
        [SerializeField] private float floatValue;
        [SerializeField] private FloatData floatDataValue;
        [SerializeField] private bool boolValue;

        public object GetValue()
        {
            return valueType switch
            {
                ValueType.String => stringValue,
                ValueType.Int => intValue,
                ValueType.IntData => intDataValue.value,
                ValueType.Float => floatValue,
                ValueType.FloatData => floatDataValue.value,
                ValueType.Bool => boolValue,
                _ => null,
            };
        }
    }
}