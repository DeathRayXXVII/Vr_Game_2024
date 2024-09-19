using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

public class AutoFindInteractbleAffordance : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<XRInteractableAffordanceStateProvider>().interactableSource = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
    }
}
