using System.Collections;
using ShipGame.ScriptObj;
using UnityEngine;

namespace ShipGame.Manager
{
    public class ShopGameManager : GameManager
    {
        [SerializeField] private CoreData coreData;
        
        [System.Serializable]
        private struct ShopData
        {
            public DialoguePurchaseHandler purchaseHandler;
            public UpgradeData upgradeData;
        }
        
        [SerializeField] private ShopData[] _shopData;
        
        private UpgradeData[] upgradeables
        {
            get
            {
                if (_shopData == null || _shopData.Length == 0)
                {
                    return null;
                }
                
                var upgradeArray = new UpgradeData[_shopData.Length];
                for (var i = 0; i < _shopData.Length; i++)
                {
                    upgradeArray[i] = _shopData[i].upgradeData;
                }

                return upgradeArray;
            }
        }
        
        private IEnumerator InitializeShop()
        {
            yield return StartCoroutine(InitializeUpgradeables());
            
            yield return StartCoroutine(UpdateStock());
        }
        
        private IEnumerator InitializeUpgradeables()
        {
            var dataToProcess = upgradeables;
            if (dataToProcess == null)
                yield break;
            
            foreach (var upgrade in dataToProcess)
            {
                if (upgrade == null)
                {
                    continue;
                }

                WaitUntil waitUntil;
                try
                {
                    upgrade.LoadOnStartup();
                    waitUntil = new WaitUntil(() => upgrade.isInitialized);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ERROR] Error initializing upgradeable: {upgrade.name}\n{e}", this);
                    waitUntil = new WaitUntil(() => true);
                }
                
                yield return waitUntil;
            }
            
            yield return null;
        }
        
        private IEnumerator UpdateStock()
        {
            if (_shopData == null || _shopData.Length == 0)
            {
                yield break;
            }
            
            foreach (var shop in _shopData)
            {
                if (shop.purchaseHandler == null)
                {
                    continue;
                }
                
                shop.purchaseHandler.CheckStock();
                yield return null;
            }
        }
        
        protected override IEnumerator Initialize()
        {
            coreData.Setup();
            yield return new WaitUntil(() => coreData.setupComplete);
            
            yield return StartCoroutine(InitializeShop());
            yield return null;
            
            yield return StartCoroutine(base.Initialize());
        }
    }
}