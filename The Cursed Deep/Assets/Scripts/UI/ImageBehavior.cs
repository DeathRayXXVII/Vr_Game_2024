#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ImageBehavior : MonoBehaviour
{
    public UnityEvent startEvent;

    [SerializeField] private Image _fillImage;
    [SerializeField] private Slider _slider;
    
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color halfHealthColor = Color.yellow;
    [SerializeField] private Color noHealthColor = Color.red;
    
    [SerializeField] private FloatData maxHealth;
    [SerializeField] private FloatData currentHealth;
    [SerializeField, SteppedRange(0, 0.1f, 0.01f)] private float hideImageThreshold = 0.02f;
    
#if UNITY_EDITOR
    [SerializeField, SteppedRange(0, 1f, 0.01f)] private float testSlider = 1f;

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            // Delay the editor update to avoid unsafe UI operations
            EditorApplication.delayCall += SafeEditorUpdate;
        }
    }

    private void SafeEditorUpdate()
    {
        if (this == null) return;

        if (_slider != null)
        {
            _slider.value = Mathf.Clamp(testSlider, 0f, 1f);
        }

        if (_fillImage != null && maxHealth != null)
        {
            UpdateImage(testSlider * maxHealth.value);
        }
    }
#endif

    private void Awake()
    {
        _fillImage ??= GetComponent<Image>();
        _slider ??= GetComponent<Slider>();
    }

    private void Start()
    {
        UpdateImage(currentHealth ?? 0f);
        startEvent.Invoke();
    }

    public void UpdateImage(FloatData data)
    {
        if (data == null)
        {
            Debug.LogError("Current value is null", this);
            return;
        }
        
        UpdateImage(data.value);
    }

    public void UpdateImage(float data)
    {
        if (_slider == null)
        {
            Debug.LogError("Missing slider.", this);
            return;
        }
        if (_fillImage == null)
        {
            Debug.LogError("Missing fill image.", this);
            return;
        }

        if (maxHealth == null)
        {
            Debug.LogError("Max value is null", this);
            return;
        }

        //imageObj.fillAmount = (data.value / maxHealth.value);
        var result = data / maxHealth.value;
        if (result < hideImageThreshold) result = 0f;
        
        _slider.value = result;
        noHealthColor.a = result == 0f ? 0f : 1f;
        
        if (result > 0.5f)
        {
            _fillImage.color = Color.Lerp(halfHealthColor, fullHealthColor, (result - 0.5f) * 2f);
        }
        else
        {
            _fillImage.color = Color.Lerp(noHealthColor, halfHealthColor, result * 2f);
        }
    }
}
