using UnityEngine;

namespace UI.DialogueSystem
{
    [System.Serializable]
    public class Response 
    {
        [SerializeField] private string responseText;
        [SerializeField] private string id;
        [SerializeField] private bool isPurchasable;
        [SerializeField] private DialogueData dialogueData;
        [SerializeField] private DialoguePurcheseHandler purchaseHandler;
        public string ResponseText => responseText;
        public string Id => id;
        public DialogueData DialogueData => dialogueData;
        public bool IsPurchasable => isPurchasable;
        public DialoguePurcheseHandler PurchaseHandler => purchaseHandler;
    }
}
