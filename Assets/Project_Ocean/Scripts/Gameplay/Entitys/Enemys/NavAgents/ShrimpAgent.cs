using System.Collections;
using UnityEngine;

namespace Game.Testing
{
    [RequireComponent(typeof(Enemy))]
    public class ShrimpAgent : EnemyAgent
    {
        protected override bool requiresNavMesh => true;

        private ShrimpBrain shrimpBrain;
        private bool        corpseRoutineStarted;
        private bool        playerNearCorpse;
        private Coroutine   corpseCoroutine;

        // ------------------------------------------------------------------ //
        //  Brain factory                                                       //
        // ------------------------------------------------------------------ //
        protected override IEnemyBrain CreateBrain()
        {
            ShrimpProfile profile = BaseProfile as ShrimpProfile;

            if (profile == null)
            {
                Debug.LogError($"[ShrimpAgent] brainProfile must be a ShrimpProfile on {name}.", this);
                profile = ScriptableObject.CreateInstance<ShrimpProfile>();
            }

            shrimpBrain = new ShrimpBrain(profile);
            return shrimpBrain;
        }

        // ------------------------------------------------------------------ //
        //  Reset state every time brain is (re-)initialized — covers pooling  //
        // ------------------------------------------------------------------ //
        protected override void OnBrainInitialized()
        {
            corpseRoutineStarted = false;
            playerNearCorpse     = false;

            if (corpseCoroutine != null)
            {
                StopCoroutine(corpseCoroutine);
                corpseCoroutine = null;
            }
        }

        // ------------------------------------------------------------------ //
        //  Per-frame tick (facing)                                            //
        // ------------------------------------------------------------------ //
        protected override void OnTick()
        {
            TickFacingFromAgentVelocity();
        }

        // ------------------------------------------------------------------ //
        //  Contact damage                                                      //
        // ------------------------------------------------------------------ //
        protected override void HandlePlayerCollision()
        {
            if (isEnemyDead) return;
            if (player == null) return;

            shrimpBrain?.OnPlayerContact(player);
        }

        // ------------------------------------------------------------------ //
        //  Death — start corpse/pickup coroutine                              //
        // ------------------------------------------------------------------ //
        protected override void OnEnemyDead()
        {
            base.OnEnemyDead();

            if (corpseRoutineStarted) return;
            corpseRoutineStarted = true;

            ShrimpProfile profile = BaseProfile as ShrimpProfile;
            if (profile == null) return;

            corpseCoroutine = StartCoroutine(CorpseRoutine(profile));
        }

        // ------------------------------------------------------------------ //
        //  Corpse / Pickup coroutine                                          //
        // ------------------------------------------------------------------ //
        private IEnumerator CorpseRoutine(ShrimpProfile profile)
        {
            float elapsed = 0f;

            // Phase 1: manual pickup window — press F to collect
            while (elapsed < profile.corpseInteractTime)
            {
                elapsed += Time.deltaTime;

                if (playerNearCorpse && Input.GetKeyDown(KeyCode.F))
                {
                    CollectCorpse();
                    yield break;
                }

                yield return null;
            }

            // Phase 2: auto-collect
            CollectCorpse();

            // Phase 3: linger before pool return
            float extra = Mathf.Max(0f, profile.corpseLingerTime - profile.corpseInteractTime);
            yield return new WaitForSeconds(extra);
        }

        private void CollectCorpse()
        {
            Debug.Log($"[ShrimpAgent] '{name}' corpse collected.");
            // TODO: InventoryManager.Instance?.AddItem("shrimp");
        }

        // ------------------------------------------------------------------ //
        //  Interact range detection                                            //
        // ------------------------------------------------------------------ //
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isEnemyDead) return;
            if (other.CompareTag("Player")) playerNearCorpse = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) playerNearCorpse = false;
        }
    }
}