using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UI.DialogueSystem;
using UnityEngine.Events;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameAction action;
    [SerializeField] private GameObject dialogueBox;
    public  TMP_Text textLabel;
    [SerializeField] private InputActionReference inputAction;
    [SerializeField] private float autoAdvancedDelay = 5f;
    [SerializeField] private bool autoAdvance;
    [SerializeField] private UnityEvent OnOpenDialogue, OnCloseDialogue;
    public DialogueData dialogueData;
    
    public bool IsOpen { get; private set;}
    public bool StartClosed { get; private set; }
    
    public TypewriterEffect typewriterEffect;
    private ResponseHandler responseHandler;
    
    private void Start()
    {
        typewriterEffect = GetComponent<TypewriterEffect>();
        responseHandler = GetComponent<ResponseHandler>();
        StartClosed = true;
        CloseDialogueBox();
    }
    
    public void ShowDialogue(DialogueData dialogueObj)
    {
        dialogueBox.SetActive(true);
        StartCoroutine(StepThroughDialogue(dialogueObj));
    }
    public void AddResponseEvents(ResponseEvent[] responseEvents) => responseHandler.AddResponseEvents(responseEvents);
    
    private IEnumerator StepThroughDialogue(DialogueData dialogueObj)
    {
        IsOpen = true;
        for (int i = 0; i < dialogueObj.Dialogue.Length; i++)
        {
            string dialogue = dialogueObj.Dialogue[i];
            yield return RunTypingEffect(dialogue);
            textLabel.text = dialogue;
            if (i == dialogueObj.Dialogue.Length - 1 && dialogueObj.hasResponses) break;
            
            yield return null;
            if (autoAdvance)
            {
                yield return new WaitForSeconds(autoAdvancedDelay);
                
            }
            else
            {
                yield return new WaitUntil(() => inputAction.action.triggered);
            }
        }
        if (dialogueObj.hasResponses && dialogueObj.Responses.Length > 0)
        {
            responseHandler.ShowResponses(dialogueObj.Responses);
        }
        else
        {
            yield return new WaitUntil(() => inputAction.action.triggered);
            CloseDialogueBox(dialogueObj);
        }
        IsOpen = false;
    }
    
    private IEnumerator RunTypingEffect(string dialogue)
    {
        typewriterEffect.Run(dialogue, textLabel);
        while (typewriterEffect.IsRunning)
        {
            yield return null;
            if (inputAction.action.triggered)
            {
                typewriterEffect.Stop();
            }
        }
    }

    private void CloseDialogueBox()
    {
        dialogueBox.SetActive(false);
        textLabel.text = string.Empty;
    }
    public void CloseDialogueBox(DialogueData dialogueObj)
    {
        dialogueBox.SetActive(false);
        textLabel.text = string.Empty;
        dialogueObj.LastDialogueEvent(action);
    }
    
    public void OnEnable()
    {
        if (inputAction != null)
        {
            inputAction.action.Enable();
        }
        
    }
    public void OnDisable()
    {
        if (inputAction != null)
        {
            inputAction.action.Disable();
        }
        
    }
}
