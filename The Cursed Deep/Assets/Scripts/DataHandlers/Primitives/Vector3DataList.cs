using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "Vector3DataList", menuName = "Data/Collection/Vector3DataList")]
public class Vector3DataList : ScriptableObject
{
    public List<Vector3Data> vector3DList;
}
