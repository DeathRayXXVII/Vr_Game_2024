using UnityEngine;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

namespace ZPTools
{
    public class StartupLoaderManager : MonoBehaviour
    {
        private void ExecuteLoadOnStartup(ILoadOnStartup loader)
        {
            if (loader == null)
            {
                Debug.LogError("[ERROR] Loader is null", this);
                return;
            }
            loader.LoadOnStartup();
        }
        private void Start() => PerformActionOnInterface((ILoadOnStartup objectToLoad) => ExecuteLoadOnStartup(objectToLoad));
    }
}