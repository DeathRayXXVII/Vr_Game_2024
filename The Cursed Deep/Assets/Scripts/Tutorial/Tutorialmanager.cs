using System;
using System.Collections;
using System.Collections.Generic;
using UI.DialogueSystem;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;


public class Tutorialmanager : MonoBehaviour
{
    public DialogueData dialogOne;
    public DialogueData dialogTwo;
    public UnityEvent activateEvent;
    public BoxCollider firstStepsCollider;
    public Collider player;

    public void OnCollisionEnter(Collision other)
    {
        if (dialogTwo.hasPlayed)
        {
            Debug.Log("AHHH");
            activateEvent.Invoke();
        }
    }
}
