using UnityEngine;

namespace ShipGame.Manager
{
    [System.Serializable]
    public class LevelSelection : MonoBehaviour
    {
        private int _id;
        public bool isBossLevel;
        public CreepData enemyData;
        public SocketMatchInteractor socket;
        
        [SerializeField] private TextMeshProBehavior _infoTextMesh;
        
        public int id { get => _id; set => _id = value; }
        
    }
}
