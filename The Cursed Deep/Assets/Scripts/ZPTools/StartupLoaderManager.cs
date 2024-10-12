using System;
using UnityEngine;
using System.Linq;
using ZPTools.Interface;

namespace ZPTools
{
    public class StartupLoaderManager : MonoBehaviour
    {
        public ScriptableObject[] scriptableObjectLoaders;

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var loader in scriptableObjectLoaders)
            {
                if (loader is IStartupLoader || !loader) continue;
                Debug.LogError($"{loader.name} does not implement IStartupLoader", this);
            }
        }
#endif

        private void Start()
        {
            foreach (var loader in scriptableObjectLoaders.OfType<IStartupLoader>())
            {
                try
                {
                    loader.LoadOnStartup();
                }
                catch (Exception e)
                {
                    Debug.LogError(e, this);
                }
            }
        }
    }
}