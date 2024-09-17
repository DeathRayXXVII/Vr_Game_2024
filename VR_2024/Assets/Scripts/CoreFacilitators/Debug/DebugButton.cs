using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DebugButton : MonoBehaviour, INeedButton
{
    public UnityEvent onButtonPress;

#if UNITY_EDITOR
    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)>
        {
            (() => onButtonPress.Invoke(), "OnButtonPress")
        };
    }
#endif
}
