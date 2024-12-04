using UnityEngine;

namespace ShipGame.ScriptObj
{
    [CreateAssetMenu(fileName = "BossData", menuName = "Data/ShipGame/BossData")]
    public class BossData : EnemyData
    {
        [System.Serializable]
        protected new struct EnemyDataJson
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
        
        protected new EnemyDataJson TempJsonData;

        protected override void ParseJsonFile(TextAsset jsonObject)
        {
            TempJsonData = ParseJsonData<EnemyDataJson>(jsonObject.text);
        }
        
        protected override void InitializeData()
        {
            if (_enemyInstanceData == null || _enemyInstanceData.Length != TempJsonData.elements)
            {
                _enemyInstanceData = new EnemyInstanceData[TempJsonData.elements];
            }
            
            for (var i = 0; i < TempJsonData.elements; i++)
            {
                _enemyInstanceData[i] = new EnemyInstanceData
                {
                    health = TempJsonData.bossHealths[i],
                    damage = TempJsonData.bossDamages[i],
                    speed = TempJsonData.bossSpeeds[i],
                    bounty = TempJsonData.bossBounties[i],
                    score = TempJsonData.bossScores[i]
                };

                if (i != currentIndex) continue;
                SetHealth(TempJsonData.bossHealths[i]);
                SetDamage(TempJsonData.bossDamages[i]);
                SetSpeed(TempJsonData.bossSpeeds[i]);
                SetBounty(TempJsonData.bossBounties[i]);
                SetScore(TempJsonData.bossScores[i]);
            }
        }
    }
}