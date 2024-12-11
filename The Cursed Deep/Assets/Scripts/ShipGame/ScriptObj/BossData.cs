using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "BossData", menuName = "Data/ShipGame/BossData")]
    public class BossData : EnemyData
    {
        [System.Serializable]
        protected struct BossDataJson
        {
            public int elements;
            public float[] bossHealths;
            public float[] bossDamages;
            public float[] bossSpeeds;
            public int[] bossBounties;
            public int[] bossScores;
        }
        
        protected override string dataFilePath => Application.dataPath + "/Resources/GameData/BossDataJson.json";
        protected override string resourcePath => "GameData/BossDataJson";
        
        private BossDataJson _tempBossJsonData;

        protected override void ParseJsonFile(TextAsset jsonObject)
        {
            _tempBossJsonData = ParseJsonData<BossDataJson>(jsonObject.text);
        }
        
        protected override void InitializeData()
        {
            if (_enemyInstanceData == null || _enemyInstanceData.Length != _tempBossJsonData.elements)
            {
                _enemyInstanceData = new EnemyInstanceData[_tempBossJsonData.elements];
            }
            
            for (var i = 0; i < _tempBossJsonData.elements; i++)
            {
                _enemyInstanceData[i] = new EnemyInstanceData
                {
                    health = _tempBossJsonData.bossHealths[i],
                    damage = _tempBossJsonData.bossDamages[i],
                    speed = _tempBossJsonData.bossSpeeds[i],
                    bounty = _tempBossJsonData.bossBounties[i],
                    score = _tempBossJsonData.bossScores[i]
                };

                if (i != currentIndex) continue;
                SetHealth(_tempBossJsonData.bossHealths[i]);
                SetDamage(_tempBossJsonData.bossDamages[i]);
                SetSpeed(_tempBossJsonData.bossSpeeds[i]);
                SetBounty(_tempBossJsonData.bossBounties[i]);
                SetScore(_tempBossJsonData.bossScores[i]);
            }
        }
        protected override void LogCurrentData()
        {
#if UNITY_EDITOR
            if (_allowDebug)
                Debug.Log("------Enemy Data------\n" +
                          $"Current Boss Index: {currentIndex}\n" +
                          $"Current Boss Base Health: {selectionHealth}\n" +
                          $"Current Boss Total Health: {health}\n" +
                          $"Current Boss Base Damage: {selectionDamage}\n" +
                          $"Current Boss Total Damage: {damage}\n" +
                          $"Current Boss Base Speed: {selectionSpeed}\n" +
                          $"Current Boss Total Speed: {speed}\n" +
                          $"Current Boss Base Bounty: {selectionBounty}\n" +
                          $"Current Boss Total Bounty: {bounty}\n" +
                          $"Current Boss Base Score: {selectionScore}\n" +
                          $"Current Boss Total Score: {score}\n" +
                          "----------------------", this);
#endif
        }
    }
}