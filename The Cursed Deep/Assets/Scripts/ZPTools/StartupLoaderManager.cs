using UnityEngine;
using System.Linq;
using ZPTools.Interface;

namespace ZPTools
{
    public class StartupLoaderManager : MonoBehaviour
    {
        public ScriptableObject[] scriptableObjectLoaders;

        private void Start()
        {
            foreach (var loader in scriptableObjectLoaders.OfType<IStartupLoader>())
            {
                loader.LoadOnStartup();
            }
        }
    }
}