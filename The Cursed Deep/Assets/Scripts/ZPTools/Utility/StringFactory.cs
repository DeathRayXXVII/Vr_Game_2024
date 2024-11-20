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

    [System.Serializable]
    public class StringFactory
    {
        // The format string that contains placeholders like {0}, {1}, etc.
        [SerializeField, TextArea] private string formatString;
        [SerializeField] private FormattableValue[] formatValues;

        public StringFactory(string formatString, params FormattableValue[] formatValues)
        {
            this.formatString = formatString;
            this.formatValues = formatValues;
        }

        public string FormattedString => FormatString(formatString, formatValues);

        public string FormatString(string input, FormattableValue[] args)
        {
            if (string.IsNullOrEmpty(input))
            {
                Debug.LogError("Input string is null or empty.");
                return string.Empty;
            }
            // Return the original string if no arguments are given
            if (args == null || args.Length == 0)
            {
                Debug.Log($"No arguments given, returning original string: {input}");
                return input;
            }

            // Convert values to an object array for formatting
            object[] formattedArgs = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                formattedArgs[i] = args[i].GetValue();
            }

            try
            {
                var formattedString = string.Format(input, formattedArgs);
                Debug.Log("Formatted string: " + formattedString);
                return string.Format(input, formattedArgs);
            }
            catch (System.FormatException e)
            {
                Debug.LogError("FormatException: " + e.Message);
                return input; // Return the original string if there's an issue with formatting
            }
        }
    }
}
