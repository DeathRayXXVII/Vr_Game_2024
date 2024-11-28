using UnityEngine;

[CreateAssetMenu]
public class GameAction : ScriptableObject
{
    public delegate void GameActionEvent(GameAction action);
    
    public event GameActionEvent RaiseEvent;

    public void RaiseAction() => RaiseEvent?.Invoke(this);
}