using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "ShipData", menuName = "Data/ManagerData/ShipData")]
public class ShipData : ScriptableObject
{
    public GameObject currentShip;
    public GameObject currentCannons;
    public GameObject currentCannonBalls;
    public List<GameObject> cannonPositions;
    public int cannonDamage;
    public int spawnLanes;
}
