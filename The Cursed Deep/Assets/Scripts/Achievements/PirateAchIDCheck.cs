using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Achievements
{
    public class PirateAchIDCheck : MonoBehaviour
    {
        [System.Serializable]
        
        public class PossibleMatch
        {
            public ID id;
            public bool value;
            public GameAction action;
        }
        [SerializeField] private List<PossibleMatch> validIDs;

        private ID _currentID;

        public void CheckSocketedID(GameObject obj)
        {
            var socketedObject = obj;
            if (socketedObject == null) return;
            
            _currentID = socketedObject.GetComponent<IDBehavior>()?.id;
            if (_currentID == null) return;

            foreach (var t in validIDs)
            {
                t.value = t.id == _currentID;
            }
        }

        public void FireCannon()
        {
            foreach (var t in validIDs.Where(t => t.value))
            {
                t.action.RaiseAction();
                break;
            }
        }
    }
}