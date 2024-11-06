using UnityEngine;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

namespace ZPTools
{
    public class StartupLoaderManager : MonoBehaviour
    {
        private static void ExecuteLoadOnStartup(ILoadOnStartup loader) => loader.LoadOnStartup();
        private void Start() => PerformActionOnInterface((ILoadOnStartup objectToLoad) => ExecuteLoadOnStartup(objectToLoad));
    }
}