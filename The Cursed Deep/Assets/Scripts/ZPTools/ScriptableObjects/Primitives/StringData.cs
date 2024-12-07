using UnityEngine;
using ZPTools.Utility;

namespace ZPTools.ScriptableObjects.Primitives
{
    [CreateAssetMenu(fileName = "StringData", menuName = "Data/Primitives/StringData")]
    public class StringData : ScriptableObject
    {
        [SerializeField] private StringFactory _text;
        public string text => _text.formattedString;
    }
}
