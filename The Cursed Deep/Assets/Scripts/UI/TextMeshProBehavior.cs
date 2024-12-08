using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using ZPTools.Utility;

[RequireComponent(typeof(TextMeshProUGUI)), DisallowMultipleComponent]
public class TextMeshProBehavior : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textObj;
    [SerializeField] private bool _updateOnStart;
    [SerializeField] private StringFactory _text;
    public UnityEvent startEvent;

    private void Awake()
    {
        ValidateTMProObject();
        _text.debugContext = this;
    }

    private void Start()
    {
        startEvent.Invoke();
        if (_updateOnStart) UpdateLabel();
    }
    
    private bool ValidateTMProObject()
    {
        if (_textObj != null) return true;
        _textObj = GetComponent<TextMeshProUGUI>();
        
        if (_textObj != null) return true;
        Debug.LogError("TextMeshPro Object is null, please assign a TextMeshPro object to this component");
        
        return false;
    }
    
    private static string FormatTime(float num)
    {
        float hour = Mathf.FloorToInt(num / 3600);
        float minutes = Mathf.FloorToInt(num / 60) % 60;
        float seconds = Mathf.FloorToInt(num % 60);
        float milliseconds = Mathf.FloorToInt((num * 100 ) % 100);
        return num switch
        {
            > 3600 => $"{hour:00}:{minutes:00}:{seconds:00}",
            < 60 => $"{seconds:00} sec",
            _ => $"{minutes:00}:{seconds:00}"
        };
    }

    public void UpdateLabel() => HandleUpdateLabel(_text.formattedString);
    public void UpdateLabel(string text) => HandleUpdateLabel(text);
    public void UpdateLabel(FloatData obj) => HandleUpdateLabel(obj);
    public void UpdateLabel(IntData obj) => HandleUpdateLabel(obj);
    public void UpdateLabel(DoubleData obj) => HandleUpdateLabel(obj);
    public void UpdateTextToTimeFormat(FloatData obj) => HandleUpdateLabel(FormatTime(obj.value));

    private void HandleUpdateLabel(object value)
    {
        if (!ValidateTMProObject()) return;
        
        switch (value)
        {
            case float floatValue:
                _textObj.text = floatValue.ToString(CultureInfo.InvariantCulture);
                break;
            case FloatData floatData:
                _textObj.text = floatData.value.ToString(CultureInfo.InvariantCulture);
                break;
            case int intValue:
                _textObj.text = intValue.ToString(CultureInfo.InvariantCulture);
                break;
            case IntData intData:
                _textObj.text = intData.value.ToString(CultureInfo.InvariantCulture);
                break;
            case DoubleData doubleData:
                _textObj.text = doubleData.value.ToString(CultureInfo.InvariantCulture);
                break;
            case string str:
                _textObj.text = str;
                break;
            default:
                Debug.LogError("Value is not a valid type for updating TextMeshPro text", this);
                break;
        }
    }
}