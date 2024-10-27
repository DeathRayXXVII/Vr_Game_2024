using UnityEngine;

namespace UI.DialogueSystem
{
    [System.Serializable]
    public class Response 
    {
        [SerializeField] private string responseText;
        [SerializeField] private string id;
        [SerializeField] private DialogueData dialogueData;
        [SerializeField] private bool isPurchasable;
        [SerializeField] private DialogueData purchaseDialogue;
        [SerializeField] public GameAction purchaseAction;
        public string ResponseText => responseText;
        public string Id => id;
        public DialogueData DialogueData => dialogueData;
        public DialogueData PurchaseDialogue => purchaseDialogue;
        public bool IsPurchasable => isPurchasable;
        public GameAction PurchaseAction => purchaseAction;
    }
}
