namespace ZPTools.Interface
{
    public interface ISaveSystemComponent
    {
        string filePath { get; }
        
        void SaveGameData();
        void LoadGameData();
        void DeleteGameData();
    }
}
