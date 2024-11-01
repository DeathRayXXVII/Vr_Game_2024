using UnityEngine;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

namespace ZPTools
{
    public class StartupLoaderManager : MonoBehaviour
    {
        private static void ExecuteLoadOnStartup(IStartupLoader loader) => loader.LoadOnStartup();
        private void Start() => PerformActionOnInterface((IStartupLoader objectToLoad) => ExecuteLoadOnStartup(objectToLoad));
    }
}