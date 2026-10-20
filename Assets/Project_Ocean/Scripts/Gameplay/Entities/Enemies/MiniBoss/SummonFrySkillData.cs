using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Game.Core;

namespace Game.Gameplay
{
    [CreateAssetMenu(fileName = "SummonFrySkill", menuName = "Enemy/Boss Skills/Summon Fry")]
    public class SummonFrySkillData : BossSkillData
    {
        [Header("Summon Fry")]
        [Tooltip("Any swimming enemy prefab. Do not use a gravity-driven one such as the crab.")]
        public GameObject fryPrefab;

        [Min(1)] public int spawnCount = 3;

        [Tooltip("Fish appear within this distance of the boss.")]
        [Min(0.5f)] public float spawnRadius = 3f;

        [Header("Fry Death")]
        [Tooltip("A dead fish stays this long, then is gone. Includes the fade time.")]
        [Min(0f)] public float corpseTime = 2f;

        [Tooltip("The last part of the corpse time in which the fish fades out.")]
        [Min(0f)] public float fadeTime = 0.5f;

        [Tooltip("Optional effect (for example a particle burst) created where the fish dissolves.")]
        public GameObject dissolveVfxPrefab;

        [Tooltip("The effect object is destroyed after this many seconds.")]
        [Min(0.1f)] public float dissolveVfxLifetime = 2f;

        private void Reset()
        {
            telegraphTime = 1f;
            recoverTime = 0.6f;
            cooldown = 20f;
            maxRange = 25f;
        }

        public override BossSkill CreateSkill(float unlockBelowHealth) => new SummonFrySkill(this, unlockBelowHealth);
    }

    /// <summary>
    /// Calls a few small fish around the boss. The fish chase the player for as long as they live, however far away
    /// the player is, and fade away a moment after they die. The skill is not used again until every fish it summoned is dead.
    /// Summoned fish are ordinary enemies: they have no spawn id (so the kill tracker ignores them)
    /// and stay alive after the boss dies.
    /// </summary>
    public class SummonFrySkill : BossSkill
    {
        private const int PointAttempts = 8;
        private const float NavSampleDistance = 1.5f;

        // How often a summoned fish is told to chase again. Must be shorter than the profile's Hit Chase Duration.
        private const float ChaseRefreshInterval = 0.5f;

        private readonly SummonFrySkillData summon;
        private readonly List<GameObject> fry = new List<GameObject>();

        public SummonFrySkill(SummonFrySkillData data, float unlockBelowHealth) : base(data, unlockBelowHealth)
        {
            summon = data;
        }

        public override bool CanUse(BossContext context) =>
            context.DistanceToPlayer() <= summon.maxRange && AliveCount() == 0;

        protected override void OnExecute()
        {
            if (ObjectPoolManager.Instance == null || summon.fryPrefab == null)
                return;

            for (int i = 0; i < summon.spawnCount; i++)
            {
                if (!TryFindPoint(out Vector3 point))
                    continue;

                GameObject go = ObjectPoolManager.Instance.Get(summon.fryPrefab, point, Quaternion.identity);
                if (go == null)
                    continue;

                fry.Add(go);

                if (go.TryGetComponent(out Enemy fish))
                    fish.StartCoroutine(LiveAndDissolve(fish));
            }
        }

        // Runs on the fish itself, so it stops by itself if the fish goes back to the pool early.
        // While alive: re-applies the "was hit, chase the player" reaction, so the fish follows the player's
        // real position every frame, whether or not it can see the player.
        // After death: leaves the corpse for a while, fades it out, plays the optional effect, returns it to the pool.
        private IEnumerator LiveAndDissolve(Enemy fish)
        {
            var wait = new WaitForSeconds(ChaseRefreshInterval);

            // One frame first: a fish created just now sets up its brain on its first frame.
            yield return null;

            while (fish != null && !fish.IsDead)
            {
                if (fish.Player != null)
                    fish.Brain?.OnHit(fish.Player.transform);

                yield return wait;
            }

            if (fish == null)
                yield break;

            float fade = Mathf.Min(summon.fadeTime, summon.corpseTime);
            yield return new WaitForSeconds(summon.corpseTime - fade);

            SpriteRenderer[] sprites = fish.GetComponentsInChildren<SpriteRenderer>(true);
            Color[] original = new Color[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
                original[i] = sprites[i].color;

            if (summon.dissolveVfxPrefab != null)
            {
                GameObject vfx = Object.Instantiate(summon.dissolveVfxPrefab, fish.transform.position, Quaternion.identity);
                Object.Destroy(vfx, summon.dissolveVfxLifetime);
            }

            for (float t = 0f; t < fade; t += Time.deltaTime)
            {
                float alpha = 1f - t / fade;
                for (int i = 0; i < sprites.Length; i++)
                {
                    Color c = original[i];
                    c.a = original[i].a * alpha;
                    sprites[i].color = c;
                }

                yield return null;
            }

            // Put the colours back so the next fish made from this pooled object is not invisible.
            for (int i = 0; i < sprites.Length; i++)
                sprites[i].color = original[i];

            ObjectPoolManager.Instance?.Return(fish.gameObject);
        }

        // The fish act on their own; the skill only waits for its recover time.
        protected override bool TickExecute(float deltaTime) => true;

        private int AliveCount()
        {
            for (int i = fry.Count - 1; i >= 0; i--)
            {
                GameObject go = fry[i];
                bool gone = go == null || !go.activeInHierarchy;
                bool dead = !gone && go.TryGetComponent(out Enemy enemy) && enemy.IsDead;

                if (gone || dead)
                    fry.RemoveAt(i);
            }

            return fry.Count;
        }

        // A point on the NavMesh near the boss, clear of walls and not behind one.
        private bool TryFindPoint(out Vector3 point)
        {
            Vector3 center = ctx.Self.position;

            for (int attempt = 0; attempt < PointAttempts; attempt++)
            {
                Vector2 offset = Random.insideUnitCircle * summon.spawnRadius;
                Vector3 candidate = center + new Vector3(offset.x, offset.y, 0f);

                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, NavSampleDistance, NavMesh.AllAreas))
                    continue;

                if (!ctx.Pathfinder.CanFishFitAt(hit.position))
                    continue;

                if (Physics2D.Linecast(center, hit.position, ctx.Profile.wallLayer).collider != null)
                    continue;

                point = hit.position;
                return true;
            }

            point = center;
            return false;
        }
    }
}
