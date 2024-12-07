using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ZPTools.Utility
{
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

        public string formattedString => FormatString(formatString, formatValues);

        public string FormatString(string input, FormattableValue[] args)
        {
            if (string.IsNullOrEmpty(input))
            {
                Debug.LogError("Input string is null or empty.");
                return string.Empty;
            }

            if (args == null || args.Length == 0)
            {
                return input;
            }

            // Convert values to an object list for formatting
            var formattedArgs = args.Select(t => t.GetValue()).ToArray();

            try
            {
                // Handle placeholders using regex
                input = HandlePlaceholders(input, formattedArgs);

                // Perform standard string formatting
                input = string.Format(input, formattedArgs);
                
                Debug.Log($"Final Formatted string: {input}");
                return input;
            }
            catch (System.FormatException e)
            {
                Debug.LogError($"FormatException: {e.Message}");
                return input;
            }
        }
        
        // Regex private variables for lazy initialization (only initialized when needed)
        private Regex _singularRegex;
        private Regex _pluralRegex;
        private Regex _currencyRegex;
        private Regex _currencyWholeRegex;
        private Regex _currencyDecimalRegex;
        
        // Lazy initialization of regex patterns, higher overhead every use but lower initial overhead
        // Use this for patterns that are not used often
        private Regex singularRegex => _singularRegex ??= new Regex(@"{(\d+):singular}");
        private Regex pluralRegex => _pluralRegex ??= new Regex(@"{(\d+):plural}");
        private Regex currencyRegex => _currencyRegex ??= new Regex(@"{(\d+):currency}");
        private Regex currencyWholeRegex => _currencyWholeRegex ??= new Regex(@"{(\d+):wholeCurrency}");
        private Regex currencyDecimalRegex => _currencyDecimalRegex ??= new Regex(@"{(\d+):decimalCurrency}");
        
        
        // Compiled regex patterns, lower overhead every use but higher initial overhead
        // Use this for patterns that are used often
        // private readonly Regex singularRegex = new(@"{(\d+):singular}", RegexOptions.Compiled);
        // private readonly Regex pluralRegex = new(@"{(\d+):plural}", RegexOptions.Compiled);
        // private readonly Regex currencyRegex = new(@"{(\d+):currency}", RegexOptions.Compiled);
        // private readonly Regex currencyWholeRegex = new(@"{(\d+):wholeCurrency}", RegexOptions.Compiled);
        // private readonly Regex currencyDecimalRegex = new(@"{(\d+):decimalCurrency}", RegexOptions.Compiled);

        private string HandlePlaceholders(string input, object[] formattedArgs)
        {
            // Handle singular and plural placeholders
            input = pluralRegex.Replace(input, match =>
            {
                Debug.Log($"Plural match: {match.Value}");
                // If the match doesn't have a group, return the original match
                if (!match.Groups[1].Success) return match.Value;
                // Get the index of the placeholder
                int index = int.Parse(match.Groups[1].Value);
                // If the index is out of bounds, return the original match
                if (index >= formattedArgs.Length) return match.Value;
                // Get the value of the placeholder
                float value = System.Convert.ToSingle(formattedArgs[index]);
                // If the value is 1, return the singular form, otherwise return the plural form
                return Mathf.Approximately(value, 1) ? "" : "s";
            });

            input = singularRegex.Replace(input, match =>
            {
                Debug.Log($"Singular match: {match.Value}");
                if (!match.Groups[1].Success) return match.Value;
                int index = int.Parse(match.Groups[1].Value);
                if (index >= formattedArgs.Length) return match.Value;
                float value = System.Convert.ToSingle(formattedArgs[index]);
                return Mathf.Approximately(value, 1) ? "" : "s";
            });

            // Handle currency placeholders
            input = currencyRegex.Replace(input, match =>
            {
                Debug.Log($"Currency match: {match.Value}");
                if (!match.Groups[1].Success) return match.Value;
                int index = int.Parse(match.Groups[1].Value);
                if (index >= formattedArgs.Length) return match.Value;
                float value = System.Convert.ToSingle(formattedArgs[index]);
                return value.ToString("C2", CultureInfo.InvariantCulture);
            });

            input = currencyWholeRegex.Replace(input, match =>
            {
                Debug.Log($"Whole Currency match: {match.Value}");
                if (!match.Groups[1].Success) return match.Value;
                int index = int.Parse(match.Groups[1].Value);
                if (index >= formattedArgs.Length) return match.Value;
                float value = System.Convert.ToSingle(formattedArgs[index]);
                return Mathf.Floor(value).ToString("C0", CultureInfo.InvariantCulture);
            });

            input = currencyDecimalRegex.Replace(input, match =>
            {
                Debug.Log($"Decimal Currency match: {match.Value}");
                if (!match.Groups[1].Success) return match.Value;
                int index = int.Parse(match.Groups[1].Value);
                if (index >= formattedArgs.Length) return match.Value;
                float value = System.Convert.ToSingle(formattedArgs[index]);
                return value.ToString("C2", CultureInfo.InvariantCulture);
            });
            
            Debug.Log($"Formatted string after REGEX: {input}");
            // If no matches are found, return the original string
            return input;
        }
    }
}
