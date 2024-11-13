using UnityEngine;

[CreateAssetMenu]
public class GameAction : ScriptableObject
{
    public delegate void GameActionEvent(GameAction action);
    
    public event GameActionEvent Raise;

    public void RaiseAction()
    {
        Raise?.Invoke(this);
    }
}