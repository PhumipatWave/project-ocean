using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Aggressive enemy: chases, winds up and attacks the player.</summary>
    [CreateAssetMenu(fileName = "AggressiveEnemyProfile", menuName = "Enemy/Aggressive Enemy Profile")]
    public class AggressiveEnemyProfile : EnemyBaseProfile
    {
        [Header("Behavior")]
        [Tooltip("If enabled, enemy continues searching the last known player position after losing sight.")]
        public bool searchAfterLost = true;

        [Tooltip("If enabled, enemy enters a prepare phase before attacking.")]
        public bool prepareBeforeAttack;

        [Tooltip("If enabled, this enemy can attack the player.")]
        public bool canAttack;

        [Tooltip("If enabled, detecting the player alerts nearby allied enemies.")]
        public bool alertNearbyAllies;

        [Header("Search")]
        [Tooltip("How long the enemy searches the last known position before giving up.")]
        public float searchDuration = 4f;

        [Tooltip("Radius used when picking search waypoints around the last known position.")]
        public float searchRadius = 2.5f;

        [Header("Prepare")]
        [Tooltip("Duration of the prepare phase before launching an attack.")]
        public float prepareTime = 1f;

        [Header("Attack")]
        [Tooltip("Speed boost applied during the attack dash.")]
        public float boostSpeed = 7f;

        [Tooltip("Acceleration during the attack dash. Lower = slower wind-up to boostSpeed.")]
        public float dashAcceleration = 200f;

        [Tooltip("If enabled, the dash locks its direction at start and runs the full Dash Duration straight through the player. If disabled, the enemy stops at Attack Range and bites.")]
        public bool dashThrough;

        [Tooltip("Dash length in seconds (Dash Through only). Dash distance is about Boost Speed x Dash Duration.")]
        public float dashDuration = 1.5f;

        [Tooltip("Distance within which the enemy can land an attack.")]
        public float attackRange = 1.5f;

        [Tooltip("Minimum time between consecutive attacks.")]
        public float attackCooldown = 2f;

        [Tooltip("Damage dealt per attack hit.")]
        public float attackDamage = 10f;

        [Tooltip("Seconds a new attack is blocked after an attack timed out without connecting (prevents Prepare/Attack loops).")]
        public float attackRetryDelay = 1f;

        [Header("Reposition")]
        [Tooltip("After an attack, the enemy swims to a random spot at least this far from the player before attacking again.")]
        public float retreatMinDistance = 3f;

        [Tooltip("Maximum distance from the player for the post-attack reposition spot.")]
        public float retreatMaxDistance = 5f;

        [Header("Lurk (player out of reach)")]
        [Tooltip("If enabled, a fish that cannot reach the player lurks nearby for a while, then gives up and wanders.")]
        public bool enableLurk = true;

        [Tooltip("How long the player must stay unreachable before the fish starts to lurk.")]
        public float unreachableBeforeLurk = 1f;

        [Tooltip("How long the fish lurks before it gives up.")]
        public float lurkDuration = 6f;

        [Tooltip("Radius of the ring the fish swims around while lurking.")]
        public float lurkOrbitRadius = 4f;

        [Tooltip("Lurk orbit speed in degrees per second.")]
        public float lurkOrbitSpeed = 40f;

        [Tooltip("After giving up, the fish ignores a player it cannot reach for this many seconds. A reachable player is chased immediately.")]
        public float chaseGiveUpCooldown = 8f;

        [Header("Group Alert")]
        [Tooltip("Radius within which nearby allies are alerted when this enemy detects the player.")]
        public float groupAlertRadius = 3f;

        
        public float detectDistance => viewDistance;
    }
}
