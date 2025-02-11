using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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
        [SerializeField] private XRGrabInteractable socketObject;

        private ID _currentID;

        public void CheckSocketedID()
        {
            var socketedObject = socketObject.gameObject;
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
            for (var i = 0; i < validIDs.Count; i++)
            {
                if (!validIDs[i].value) continue;
                validIDs[i].action.RaiseAction();
                break;
            }
        }
    }
}