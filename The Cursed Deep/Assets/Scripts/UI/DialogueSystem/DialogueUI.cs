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
    [SerializeField, SteppedRange(0, 25, 0.1f)] private float autoCloseDelay = 5f;
    private const float elementDelay = 1f;
    private WaitForSeconds waitElementAdvance;
    [SerializeField] private bool autoClose;
    [SerializeField] private UnityEvent OnOpenDialogue, OnTypingFinish, OnCloseDialogue;
    public DialogueData dialogueData;
    
    private Coroutine _dialogueCoroutine;
    
    public bool IsOpen { get; private set;}
    
    public TypewriterEffect typewriterEffect;
    private ResponseHandler responseHandler;
    
    private void Start()
    {
        waitElementAdvance = new WaitForSeconds(elementDelay);
        typewriterEffect ??= GetComponent<TypewriterEffect>();
        responseHandler ??= GetComponent<ResponseHandler>();
        CloseDialogueBox();
    }
    
    public void SetAutoClose(bool state) => autoClose = state;
    
    public void ShowDialogue(DialogueData dialogueObj)
    {
        if (dialogueObj.locked) return;
        dialogueObj.Activated();
        
        if (IsOpen && _dialogueCoroutine != null)
        {
            if (dialogueData == dialogueObj) return;
            CloseDialogueBox(dialogueData);
        }
        
        dialogueBox.SetActive(true);
        
        _dialogueCoroutine ??= StartCoroutine(StepThroughDialogue(dialogueObj));
    }
    public void AddResponseEvents(ResponseEvent[] responseEvents) => responseHandler.AddResponseEvents(responseEvents);
    
    private IEnumerator StepThroughDialogue(DialogueData dialogueObj)
    {
        IsOpen = true;
        dialogueData = dialogueObj;
        
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
            yield break;
        }

        if (autoClose)
        {
            var time = Time.time;

            while (Time.time - time < autoCloseDelay)
            {
                if (inputAction.action.triggered)
                {
                    CloseDialogueBox(dialogueObj);
                    yield break;
                }
                yield return null;
            }
        }
        else
        {
            yield return new WaitUntil(() => inputAction.action.triggered);
        }
        
        CloseDialogueBox(dialogueObj);
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
        IsOpen = false;
        dialogueBox?.SetActive(false);
        textLabel.text = string.Empty;
        _dialogueCoroutine = null;
    }
    
    public void CloseDialogueBox(DialogueData dialogueObj)
    {
        CloseDialogueBox();
        dialogueObj?.LastDialogueEvent(action);
    }
    
    public void OnEnable() => inputAction?.action.Enable();
    public void OnDisable() => inputAction?.action.Disable();
}
