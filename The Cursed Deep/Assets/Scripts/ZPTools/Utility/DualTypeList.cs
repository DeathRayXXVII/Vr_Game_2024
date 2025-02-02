using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ZPTools.Utility
{
    [System.Serializable]
    public class DualTypeList
    {
        [SerializeField] private List<float> _floatList;
        [SerializeField] private List<int> _intList;
        
        public DataType.EnumDataTypes listType;
        
        public DualTypeList(DataType.EnumDataTypes initialListType)
        {
            _floatList = new List<float>();
            _intList = new List<int>();
            listType = initialListType;
        }

        private long ClampLongToIntRange(long value) => value < int.MinValue ? int.MinValue : 
            value > int.MaxValue ? int.MaxValue : value;
    
        private int ConvertToInt(object value)
        {
            long longValue;

            switch (value)
            {
                case int intValue: return intValue;
            
                case float floatValue: longValue = (long)floatValue;
                    break;
                case double doubleValue: longValue = (long)doubleValue;
                    break;
                case long longVal: longValue = longVal;
                    break;

                default:
                    throw new System.ArgumentException($"Unsupported data type or value mismatch\nReceived type: {value.GetType()}\nValue: {value}");
            }
        
            return (int)ClampLongToIntRange(longValue);;
        }
    
        private float ConvertToFloat(object value)
        {
            return value switch
            {
                float floatValue => floatValue,
                int intValue => intValue,
                double doubleValue => (float)doubleValue,
                long longValue => longValue,
                _ => throw new System.ArgumentException(
                    $"Unsupported data type or value mismatch\nReceived type: {value.GetType()}\nValue: {value}")
            };
        }
    
        public void AddValue(object value)
        {
            switch (listType)
            {
                // Conversions to float, when float value is expected
                case DataType.EnumDataTypes.Float:
                    _floatList.Add(ConvertToFloat(value));
                    // Debug.Log($"Adding value: {value} to float list: {this}\nConverted value: {ConvertToFloat(value)}\nContents: {string.Join(", ", _floatList)}");
                    break;
        
                // Conversion to int, when Int value is expected
                case DataType.EnumDataTypes.Int:
                    _intList.Add(ConvertToInt(value));
                    // Debug.Log($"Adding value: {value} to int list: {this}\nConverted value: {ConvertToInt(value)}\nContents: {string.Join(", ", _intList)}");
                    break;
        
                default:
                    throw new System.ArgumentException($"Unsupported data type or value mismatch\nExpected type: {listType}\nReceived type: {value.GetType()}\nValue: {value}");
            }
        }

    
        private void HandleAddRange<T>(List<T> values)
        {
            // Debug.Log($"Adding range of {values.Count} values\n{string.Join(", ", values)}");
            if (values.Count == 0) return;
        
            foreach (var value in values)
            {
                AddValue(value);
            }
        }
    
        public void AddRange(List<object> values) => HandleAddRange(values);
        public void AddRange(JArray values) => HandleAddRange(values.ToObject<List<object>>());

        public int Count()
        {
            return listType switch
            {
                DataType.EnumDataTypes.Float => _floatList.Count,
                DataType.EnumDataTypes.Int => _intList.Count,
                _ => 0
            };
        }

        public int GetLastIndex() => Count() - 1;
    
        private bool IsValidIndex(int index) => index >= 0 && index < Count();

        public object GetValue(int index) => listType switch
        {
            DataType.EnumDataTypes.Float => IsValidIndex(index) ? _floatList[index] : 0f,
            DataType.EnumDataTypes.Int => IsValidIndex(index) ? _intList[index] : 0,
            _ => throw new System.ArgumentOutOfRangeException() 
        };

        public T GetValue<T>(int index)
        {
            if (typeof(T) == typeof(float) && listType == DataType.EnumDataTypes.Float)
            {
                return (T)(object)_floatList[index];  // Safe cast if types match
            }
            else if (typeof(T) == typeof(int) && listType == DataType.EnumDataTypes.Int)
            {
                return (T)(object)_intList[index];  // Safe cast if types match
            }
            else
            {
                throw new System.InvalidCastException(
                    $"Cannot cast list of type '{listType}' to '{typeof(T)}'. Please ensure type consistency.");
            }
        }


        public void Clear()
        {
            switch (listType)
            {
                case DataType.EnumDataTypes.Float:
                    if (_floatList.Count != 0) _floatList.Clear();
                    break;
                case DataType.EnumDataTypes.Int:
                    if (_intList.Count != 0) _intList.Clear();
                    break;
            }
        }

        public static implicit operator List<object>(DualTypeList dualTypeList)
        {
            return dualTypeList.listType switch
            {
                DataType.EnumDataTypes.Float => dualTypeList._floatList.Cast<object>().ToList(),
                DataType.EnumDataTypes.Int => dualTypeList._intList.Cast<object>().ToList(),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }
    
        public new string ToString()
        {
            return listType switch
            {
                DataType.EnumDataTypes.Float => 
                    _floatList == null ? "List is null" : _floatList.Count == 0 ? "List is empty" : string.Join(", ", _floatList),
                DataType.EnumDataTypes.Int => 
                    _intList == null ? "List is null" : _intList.Count == 0 ? "List is empty" : string.Join(", ", _intList),
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }
    }
}