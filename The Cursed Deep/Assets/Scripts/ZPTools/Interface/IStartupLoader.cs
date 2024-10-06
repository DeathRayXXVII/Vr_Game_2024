namespace ZPTools.Interface
{
    public interface IStartupLoader
    {
        bool isLoaded { get; }
        void LoadOnStartup();
    }
}