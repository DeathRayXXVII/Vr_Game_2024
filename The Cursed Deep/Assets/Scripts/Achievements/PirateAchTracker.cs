using UnityEngine;
using UnityEngine.Events;
// - - - Used only for The Cursed Deep VR Game - - -
namespace Achievements
{
    public class PirateAchTracker : MonoBehaviour
    {
        private bool _hpWin;
        [SerializeField] private FloatData currentHealth;
        [SerializeField] private GameAction hpAchAction;
        
        private bool _firstLoss;
        [SerializeField] private IntData currentLevel;
        [SerializeField] private GameAction lossAchAction;
        
        [SerializeField] private BoolData boss;
        [SerializeField] private GameAction bossAchAction;
        
        [SerializeField] private UnityEvent bossFight, enemyFight;
    
        public void CheckFirstLoss()
        {
            if (_firstLoss) return;
            if (currentLevel.value is > 1 or < 1) return;
            _firstLoss = true;
            lossAchAction.RaiseAction();
        }
    
        public void CheckHpWin()
        {
            if (_hpWin) return;
            if (currentHealth.value is > 1 or < 1) return;
            _hpWin = true;
            hpAchAction.RaiseAction();
        }

        public void CheckEnemy()
        {
            if (boss)
            {
                bossFight.Invoke();
            }
            else
            {
                enemyFight.Invoke();
            }
        }
    
        public void CheckBoosWin()
        {
            if (!boss) return;
            bossAchAction.RaiseAction();
        }
    }
}
