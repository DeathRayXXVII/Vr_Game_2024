using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextMeshProBehavior : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textObj;
    public UnityEvent startEvent;
    
    private bool ValidateTMProObject()
    {
        if (_textObj != null) return true;
        _textObj = GetComponent<TextMeshProUGUI>();
        
        if (_textObj != null) return true;
        Debug.LogError("TextMeshPro Object is null, please assign a TextMeshPro object to this component");
        
        return false;
    }

    private void Awake()
    {
        ValidateTMProObject();
    }

    private void Start()
    {
        startEvent.Invoke();
    }

    public void UpdateLabel(string text)
    {
        if (!ValidateTMProObject()) return;
        _textObj.text = text.ToString(CultureInfo.InvariantCulture);
    }
    
    public void UpdateLabel(FloatData obj)
    {
        if (!ValidateTMProObject()) return;
        _textObj.text = obj.value.ToString(CultureInfo.InvariantCulture);
    }

    public void UpdateLabel(IntData obj)
    {
        if (!ValidateTMProObject()) return;
        _textObj.text = obj.value.ToString(CultureInfo.InvariantCulture);
    }    
    
    public void UpdateLabel(DoubleData obj)
    {
        if (!ValidateTMProObject()) return;
        _textObj.text = obj.value.ToString(CultureInfo.InvariantCulture);
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
    
    public void UpdateTextToTimeFormat(FloatData obj)
    {
        if (!ValidateTMProObject()) return;
        _textObj.text = FormatTime(obj.value);
    }
}