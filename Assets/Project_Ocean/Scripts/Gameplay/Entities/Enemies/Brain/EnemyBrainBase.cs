using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public abstract class EnemyBrainBase : IEnemyBrain
    {
        protected EnemyBrainContext enemyBrainContext;
        protected EnemyFSM enemyFSM { get; private set; }

        public virtual bool IsAttacking => false;

        public virtual void Initialize(EnemyBrainContext context)
        {
            enemyBrainContext = context;
            enemyFSM = new EnemyFSM();
            enemyFSM.Initialize(context);
        }

        public virtual void Tick(float deltaTime)
        {
            enemyFSM.Tick(deltaTime);
        }

        public virtual void OnHit(Transform attacker) { }

        public virtual void OnDeath()
        {
            HideAttackWarning();
            SetSeeingPlayer(false);
        }

        protected void ShowAttackWarning()
        {
            if (enemyBrainContext?.AlertUI != null)
                enemyBrainContext.AlertUI.ShowAttackWarning();
        }

        protected void HideAttackWarning()
        {
            if (enemyBrainContext?.AlertUI != null)
                enemyBrainContext.AlertUI.HideAttackWarning();
        }

        protected void SetSeeingPlayer(bool canSee)
        {
            if (enemyBrainContext?.AlertUI != null)
                enemyBrainContext.AlertUI.SetSeeingPlayer(canSee);
        }

        protected void HideAllAlerts()
        {
            if (enemyBrainContext?.AlertUI != null)
                enemyBrainContext.AlertUI.HideAll();
        }
    }
}
