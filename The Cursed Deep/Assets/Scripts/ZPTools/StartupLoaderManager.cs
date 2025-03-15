using System;
using System.Collections;
using UnityEngine;
using ZPTools.Interface;
using static ZPTools.Utility.UtilityFunctions;

namespace ZPTools
{
    public class StartupLoaderManager : MonoBehaviour
    {
        private IEnumerator ExecuteLoadOnStartupCoroutine(ILoadOnStartup loader)
        {
            if (loader == null)
            {
                Debug.LogError("[ERROR] Loader is null", this);
                yield break;
            }

            yield return new WaitForEndOfFrame();

            try
            {
                loader.LoadOnStartup();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ERROR] Error loading '{loader}' on startup: {e}", this);
            }
        }

        private void Start()
        {
            PerformActionOnInterface((ILoadOnStartup objectToLoad) => StartCoroutine(ExecuteLoadOnStartupCoroutine(objectToLoad)));
        }

    }
}