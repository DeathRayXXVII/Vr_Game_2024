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
    
    private void OnValidate()
    {
        if (!gameObject) Debug.LogError("Prefab is null. Please assign a value.", this);
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