using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Events;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private InputActionReference inputAction;
    [SerializeField] private float autoAdvancedDelay = 5f;
    [SerializeField] private bool autoAdvance = false;
    [SerializeField] private UnityEvent onDialogueEnd, onDialogueStart;
    
    public bool IsOpen { get; private set;}
    
    public TypewriterEffect typewriterEffect;
    private ResponseHandler responseHandler;
    
    private void Start()
    {
        typewriterEffect = GetComponent<TypewriterEffect>();
        responseHandler = GetComponent<ResponseHandler>();
        CloseDialogueBox();
    }
    
    public void ShowDialogue(DialogueData dialogueObj)
    {
        IsOpen = true;
        dialogueBox.SetActive(true);
        StartCoroutine(StepThroughDialogue(dialogueObj));
    }
    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        responseHandler.AddResponseEvents(responseEvents);
    }
    private IEnumerator StepThroughDialogue(DialogueData dialogueObj)
    {
        onDialogueStart.Invoke();
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
            CloseDialogueBox();
        }
        onDialogueEnd.Invoke();
    }
    
    private IEnumerator RunTypingEffect(string dialogue)
    {
        typewriterEffect.Run(dialogue, textLabel);
        while (typewriterEffect.IsRunning)
        {
            yield return null;
            //Add input for skipping dialogue at some point
            if (inputAction.action.triggered)
            {
                typewriterEffect.Stop();
            }
        }
    }
    
    public void CloseDialogueBox()
    {
        IsOpen = false;
        dialogueBox.SetActive(false);
        textLabel.text = string.Empty;
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
