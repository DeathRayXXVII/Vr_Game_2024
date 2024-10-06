using UnityEngine;
using System.Linq;
using ZPTools.Interface;

namespace ZPTools
{
    public class StartupLoaderManager : MonoBehaviour
    {
        public ScriptableObject[] scriptableObjectLoaders;

        private void Awake()
        {
            foreach (var loader in scriptableObjectLoaders.OfType<IStartupLoader>())
            {
                loader.LoadOnStartup();
            }
        }
    }
}