#if UNITY_EDITOR
public interface INeedButton
{
    System.Collections.Generic.List<(System.Action, string)> GetButtonActions();
}
#endif