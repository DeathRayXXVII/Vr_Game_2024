using System.Collections.Generic;
using TMPro;
using UI.DialogueSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ResponseHandler : MonoBehaviour
{
   [SerializeField] private RectTransform responseBox;
   [SerializeField] private RectTransform responseButtonTemplate;
   [SerializeField] private RectTransform responseContainer;
   [SerializeField] private PurchaseHandlerManager purchaseHandlerManager;

   private DialogueUI dialogueUI;
   private ResponseEvent[] responseEvents;

   private List<GameObject> tempResponseButtons = new List<GameObject>();
   private Queue<GameObject> responseButtonPool = new Queue<GameObject>();

   private void Start()
   {
      dialogueUI = GetComponent<DialogueUI>();
      if (purchaseHandlerManager == null)
      {
         purchaseHandlerManager = FindObjectOfType<PurchaseHandlerManager>();
         if (purchaseHandlerManager == null)
         {
            Debug.LogError("PurchaseHandlerManager is not assigned and could not be found in the scene.");
         }
      }
   }

   public void AddResponseEvents(ResponseEvent[] events)
   {
      responseEvents = events;
   }

   public void ShowResponses(Response[] responses)
   {
      foreach (GameObject button in tempResponseButtons)
      {
         button.SetActive(false);
         responseButtonPool.Enqueue(button);
      }
      tempResponseButtons.Clear();

      float responseBoxHeight = 0;
      for (int i = 0; i < responses.Length; i++)
      {
         Response response = responses[i];
         int responseIndex = i;

         GameObject responseButton;
         if (responseButtonPool.Count > 0)
         {
            responseButton = responseButtonPool.Dequeue();
         }
         else
         {
            responseButton = Instantiate(responseButtonTemplate.gameObject, responseContainer);
         }

         responseButton.SetActive(true);
         responseButton.GetComponentInChildren<TMP_Text>().text = response.ResponseText;
         responseButton.GetComponent<XRSimpleInteractable>().selectEntered.RemoveAllListeners();
         responseButton.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnPickedResponse);  
         responseButton.GetComponent<Button>().onClick.AddListener(() => OnPickedResponse());
         //collider, onSelect
         tempResponseButtons.Add(responseButton);

         responseBoxHeight += responseButtonTemplate.sizeDelta.y;
      }
      responseBox.sizeDelta = new Vector2(responseBox.sizeDelta.x, responseBoxHeight);
      responseBox.gameObject.SetActive(true);
   }

   // private void OnPickedResponse(Response response, int responseIndex)
   private void OnPickedResponse(SelectEnterEventArgs args)
   {
      if (response == null)
      {
         Debug.LogError("Response is null.");
         return;
      }

      responseBox.gameObject.SetActive(false);

      foreach (GameObject button in tempResponseButtons)
      {
         button.SetActive(false);
         responseButtonPool.Enqueue(button);
      }
      tempResponseButtons.Clear();

      if (responseEvents != null && responseIndex < responseEvents.Length)
      {
         responseEvents[responseIndex]?.OnPickedResponse?.Invoke();
      }

      responseEvents = null;

      if (response.IsPurchasable)
      {
         if (purchaseHandlerManager == null)
         {
            Debug.LogError("PurchaseHandlerManager is not assigned.");
            return;
         }

         if (string.IsNullOrEmpty(response.Id))
         {
            Debug.LogError("PurchaseHandlerId is null or empty.");
            return;
         }

         var purchaseHandler = purchaseHandlerManager.GetHandler(response.Id);
         if (purchaseHandler != null)
         {
            purchaseHandler.Purchase(response);
         }
         else
         {
            Debug.LogError($"PurchaseHandler with ID {response.Id} not found.");
         }
      }
      else if (response.DialogueData != null)
      {
         dialogueUI.ShowDialogue(response.DialogueData);
      }
      else
      {
         dialogueUI.CloseDialogueBox(response.DialogueData);
      }
   }
}