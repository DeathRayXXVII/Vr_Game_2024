using UnityEngine;

[CreateAssetMenu(fileName = "PrefabData", menuName = "Data/Entity/PrefabData")]
public class PrefabData : ScriptableObject
{
    public GameObject prefab;

    [SerializeField] private int spawnPriority;

    public int priority
    {
        get => spawnPriority;
        set => spawnPriority = value;
    }

    public override string ToString()
    {
        return prefab.name;
    }
}