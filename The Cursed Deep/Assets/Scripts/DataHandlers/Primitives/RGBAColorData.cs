using UnityEngine;
using ZPTools;

[CreateAssetMenu (fileName = "RGBAColorData", menuName = "Data/Primitive/RGBAColorData")]
public class RGBAColorData : ScriptableObject
{
    [SerializeField] private Color _color;
    
    public Color color
    {
        get => _color;
        set => _color = value;
    }
    
    public static implicit operator Color(RGBAColorData data)
    {
        return data.color;
    }
}