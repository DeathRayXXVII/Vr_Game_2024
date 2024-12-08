using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ZPTools.Utility
{
    [System.Serializable]
    public class StringFactory
    {
        private bool _allowDebug = false;
        
        // The format string that contains placeholders like {0}, {1}, etc.
        [SerializeField, TextArea] private string formatString;
        [SerializeField] private FormattableValue[] formatValues;
        private Object _context;
        public Object debugContext { get => _context; set => _context = value; }

        public StringFactory(string formatString, Object contextObj = null, params FormattableValue[] formatValues)
        {
            this.formatString = formatString;
            this.formatValues = formatValues;
            _context = contextObj;
        }

        public string formattedString => FormatString(formatString, formatValues);

        public string FormatString(string input, FormattableValue[] args)
        {
            if (string.IsNullOrEmpty(input))
            {
                Debug.LogError("[ERROR] Input string is null or empty.", _context);
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
                if (formattedArgs.Length > 0)
                {
                    // Handle placeholders using regex
                    input = HandlePlaceholders(input, formattedArgs);

                    // Perform standard string formatting
                    input = string.Format(input, formattedArgs);
                }
                
                if (_allowDebug) Debug.Log($"[DEBUG] Final Formatted string: {input}", _context);
                return input;
            }
            catch (System.FormatException e)
            {
                Debug.LogError($"[ERROR] FormatException: {e.Message}", _context);
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
                if (_allowDebug) Debug.Log($"[DEBUG] Plural match: {match.Value}");
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
                if (_allowDebug) Debug.Log($"[DEBUG] Singular match: {match.Value}", _context);
                if (!match.Groups[1].Success) return match.Value;
                int index = int.Parse(match.Groups[1].Value);
                if (index >= formattedArgs.Length) return match.Value;
                float value = System.Convert.ToSingle(formattedArgs[index]);
                return Mathf.Approximately(value, 1) ? "" : "s";
            });

            // Handle currency placeholders
            input = currencyRegex.Replace(input, match =>
            {
                if (_allowDebug) Debug.Log($"[DEBUG] Currency match: {match.Value}", _context);
                if (!match.Groups[1].Success) return match.Value;
                int index = int.Parse(match.Groups[1].Value);
                if (index >= formattedArgs.Length) return match.Value;
                float value = System.Convert.ToSingle(formattedArgs[index]);
                return value.ToString("C2", CultureInfo.InvariantCulture);
            });

            input = currencyWholeRegex.Replace(input, match =>
            {
                if (_allowDebug) Debug.Log($"[DEBUG] Whole Currency match: {match.Value}", _context);
                if (!match.Groups[1].Success) return match.Value;
                int index = int.Parse(match.Groups[1].Value);
                if (index >= formattedArgs.Length) return match.Value;
                float value = System.Convert.ToSingle(formattedArgs[index]);
                return Mathf.Floor(value).ToString("C0", CultureInfo.InvariantCulture);
            });

            input = currencyDecimalRegex.Replace(input, match =>
            {
                if (_allowDebug) Debug.Log($"[DEBUG] Decimal Currency match: {match.Value}", _context);
                if (!match.Groups[1].Success) return match.Value;
                int index = int.Parse(match.Groups[1].Value);
                if (index >= formattedArgs.Length) return match.Value;
                float value = System.Convert.ToSingle(formattedArgs[index]);
                return value.ToString("C2", CultureInfo.InvariantCulture);
            });
            
            if (_allowDebug) Debug.Log($"[DEBUG] Formatted string after REGEX: {input}", _context);
            // If no matches are found, return the original string
            return input;
        }
    }
}
