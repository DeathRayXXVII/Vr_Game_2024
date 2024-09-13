using UnityEngine;

[CreateAssetMenu(fileName = "PrefabData", menuName = "Data/Entity/PrefabData")]
public class PrefabData : ScriptableObject
{
    [SerializeField] private GameObject gameObject;
    [SerializeField] private int spawnPriority;
    
    public GameObject prefab
    {
        get => gameObject;
        set => gameObject = value;
    }
    
    public int priority
    {
        get => spawnPriority;
        set => spawnPriority = value;
    }
    
    public static implicit operator GameObject(PrefabData data)
    {
        return data.prefab;
    }

    public override string ToString()
    {
        return prefab.name;
    }
}