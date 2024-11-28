using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using ZPTools.Interface;

[ExecuteInEditMode]
public class DebugButton : MonoBehaviour, INeedButton
{
    public UnityEvent onButtonPress;
    
    public void PressButton() => onButtonPress?.Invoke();
    
    public List<(System.Action, string)> GetButtonActions()
    {
        return new List<(System.Action, string)>
        {
            (() => onButtonPress?.Invoke(), "OnButtonPress")
        };
    }

}
