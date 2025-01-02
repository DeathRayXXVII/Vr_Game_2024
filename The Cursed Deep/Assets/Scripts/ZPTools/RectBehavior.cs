using UnityEngine;

[DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
public class RectBehavior : MonoBehaviour
{
    private RectTransform _rectTransform;
    
    private float currentWidth => _rectTransform.sizeDelta.x;
    private float currentHeight => _rectTransform.sizeDelta.y;
    
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
    
    public void SetSize(float width, float height)
    {
        _rectTransform.sizeDelta = new Vector2(width, height);
    }
    
    public void SetHeight(float height)
    {
        _rectTransform.sizeDelta = new Vector2(currentWidth, height);
    }
}
