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
    }
}