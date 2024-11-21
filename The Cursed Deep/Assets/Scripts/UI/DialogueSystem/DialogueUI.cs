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
    public TMP_Text textLabel;
    [SerializeField] private InputActionReference inputAction;
    [SerializeField] private float autoCloseDelay = 5f;
    private const float elementDelay = 1f;
    private WaitForSeconds waitElementAdvance;
    [SerializeField] private bool autoClose;
    [SerializeField] private UnityEvent OnOpenDialogue, OnTypingFinish, OnCloseDialogue;
    public DialogueData dialogueData;
    
    public bool IsOpen { get; private set;}
    public bool StartClosed { get; private set; }
    
    public TypewriterEffect typewriterEffect;
    private ResponseHandler responseHandler;
    
    private void Start()
    {
        waitElementAdvance = new WaitForSeconds(elementDelay);
        typewriterEffect ??= GetComponent<TypewriterEffect>();
        responseHandler ??= GetComponent<ResponseHandler>();
        StartClosed = true;
        CloseDialogueBox();
    }
    
    public void SetAutoClose(bool state) => autoClose = state;
    
    public void ShowDialogue(DialogueData dialogueObj)
    {
        if (dialogueObj.locked) return;
        dialogueObj.Activated();
        dialogueBox.SetActive(true);
        StartCoroutine(StepThroughDialogue(dialogueObj));
    }
    public void AddResponseEvents(ResponseEvent[] responseEvents) => responseHandler.AddResponseEvents(responseEvents);
    
    private IEnumerator StepThroughDialogue(DialogueData dialogueObj)
    {
        IsOpen = true;
        
        var formattedDialogueArray = dialogueObj.Dialogue;
        var lastDialogue = dialogueObj.lastDialogueIndex;
        
        OnOpenDialogue.Invoke();
        for (int i = 0; i < dialogueObj.length; i++)
        {
            string dialogue = formattedDialogueArray[i];
            yield return RunTypingEffect(dialogue);
            
            if (typewriterEffect != null  && !typewriterEffect.IsRunning) OnTypingFinish.Invoke();
            textLabel.text = dialogue;
            if (i == lastDialogue && dialogueObj.hasResponses) break;
            
            yield return null;
            if (i != lastDialogue)
            {
                yield return waitElementAdvance;
            }
        }
        if (dialogueObj.hasResponses && dialogueObj.Responses.Length > 0)
        {
            responseHandler.ShowResponses(dialogueObj.Responses);
            IsOpen = false;
            yield break;
        }

        if (autoClose)
        {
            float autoCloseEndTime = Time.time + autoCloseDelay;

            while (Time.time < autoCloseEndTime)
            {
                if (inputAction.action.triggered)
                {
                    CloseDialogueBox(dialogueObj);
                    IsOpen = false;
                    yield break;
                }
                yield return null;
            }

            CloseDialogueBox(dialogueObj);
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
        dialogueBox?.SetActive(false);
        textLabel.text = string.Empty;
    }
    
    public void CloseDialogueBox(DialogueData dialogueObj)
    {
        dialogueBox?.SetActive(false);
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
