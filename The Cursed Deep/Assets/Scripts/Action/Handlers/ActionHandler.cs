using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GameActionEvent
{
    public GameAction actionObj;
    public UnityEvent onRaiseEvent;
}

[DisallowMultipleComponent]
public class ActionHandler: MonoBehaviour
{
    public List<GameActionEvent> gameActions;

    private void OnEnable()
    {
        // Subscribe to all the events in the list.
        foreach (var gameAction in gameActions.Where(gameAction => gameAction.actionObj != null))
        {
            gameAction.actionObj.RaiseEvent += RaiseEvent;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from all the events in the list.
        foreach (var gameAction in gameActions.Where(gameAction => gameAction.actionObj != null))
        {
            gameAction.actionObj.RaiseEvent -= RaiseEvent;
        }
    }

    private void RaiseEvent(GameAction callingObj)
    {
        // Find the first matching GameAction
        var gameAction = gameActions.FirstOrDefault(action => action.actionObj == callingObj);

        // If found, invoke its onRaiseEvent
        gameAction?.onRaiseEvent.Invoke();
    }
}