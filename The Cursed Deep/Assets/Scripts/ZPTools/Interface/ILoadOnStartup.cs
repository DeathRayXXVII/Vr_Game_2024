namespace ZPTools.Interface
{
    public interface ILoadOnStartup
    {
        bool isLoaded { get; }
        void LoadOnStartup();
    }
}