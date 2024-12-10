namespace ZPTools.Interface
{
    public interface ISaveSystem
    {
        string filePath { get; }
        bool savePathExists { get; }
        
        void Save();
        void Load();
        void DeleteSavedData();
    }
}
