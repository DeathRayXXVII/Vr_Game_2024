using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text textLabel;
    [SerializeField] private InputActionReference inputAction;
    
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
        for (int i = 0; i < dialogueObj.Dialogue.Length; i++)
        {
            string dialogue = dialogueObj.Dialogue[i];
            yield return RunTypingEffect(dialogue);
            textLabel.text = dialogue;
            yield return new WaitUntil(() => inputAction.action.triggered);
            if (i == dialogueObj.Dialogue.Length - 1) break;
            //if (i == dialogueObj.Dialogue.Length - 1 && dialogueObj.hasResponses) break;
            // {
            //     responseHandler.ShowResponses(dialogueObj.Responses);
            // }
            
            //yield return null;
            //yield return new WaitUntil(() => inputAction.action.triggered);
        }

        if (dialogueObj.hasResponses && dialogueObj.Responses.Length > 0)
        {
            
            responseHandler.ShowResponses(dialogueObj.Responses);
        }
        else
        {
            //yield return new WaitUntil(() => inputAction.action.triggered);
            CloseDialogueBox();
        }
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
