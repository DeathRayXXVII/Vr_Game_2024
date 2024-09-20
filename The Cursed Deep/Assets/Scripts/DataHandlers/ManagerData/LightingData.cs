using UnityEngine;

[CreateAssetMenu (fileName = "LightingData", menuName = "Data/ManagerData/LightingData")]
public class LightingData : ScriptableObject
{
    public Gradient ambientColor;
    public Gradient directionalColor;
    public Gradient fogColor;
}
