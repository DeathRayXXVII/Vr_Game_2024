using UnityEngine;

[CreateAssetMenu(fileName = "Debugger", menuName = "Debug/Debugger")]
public class Debugger : ScriptableObject
{
    private void HandleDebug<T>(T obj)
    {
        Debug.Log(obj, this);
    }
    
    public void OnDebug(string obj)
    {
        HandleDebug(obj);
    }
       
    public void OnDebug(float obj)
    {
        HandleDebug(obj);
    }
       
    public void OnDebug(bool obj)
    {
        HandleDebug(obj);
    }
       
    public void OnDebug(object obj)
    {
        HandleDebug(obj);
    }
       
    public void OnDebug(int obj)
    {
        HandleDebug(obj);
    }
}