using UnityEngine;
using System.Collections.Generic;

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

        public void CheckSocketedID(GameObject socketedObject)
        {
            _currentID = socketedObject.GetComponent<IDBehavior>()?.id;
            if (_currentID == null) return;

            foreach (var t in validIDs)
            {
                t.value = t.id == _currentID;
            }
        }

        public void FireCannon()
        {
            for (var i = 0; i < validIDs.Count; i++)
            {
                if (!validIDs[i].value) continue;
                validIDs[i].action.RaiseAction();
                break;
            }
        }
    }
}