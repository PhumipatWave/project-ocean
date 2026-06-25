using UnityEngine;

namespace Game.Testing
{
    public enum PatrolAxis 
    { 
        Horizontal, 
        Vertical
    }
    public enum PatrolMovementMode {
        FixedLength,
        UntilWall 
    }

    [CreateAssetMenu(menuName = "Game/Enemy/LinearPatrolProfile", fileName = "LinearPatrolProfile")]
    public class LinearPatrolProfile : EnemyBaseProfile
    {
        [Header("Linear Patrol")]
        [Tooltip("Axis the enemy patrols along.")]
        public PatrolAxis axis = PatrolAxis.Horizontal;

        [Tooltip("FixedLength: travel a set distance each way.\nUntilWall: travel until a wall is hit.")]
        public PatrolMovementMode movementMode = PatrolMovementMode.FixedLength;

        [Tooltip("Distance to travel each way before reversing (FixedLength mode only).")]
        public float moveLength = 5f;

        [Tooltip("Seconds paused before reversing direction.")]
        public float pauseDuration = 0.4f;

        [Header("Edge / Ground Check (UntilWall mode)")]
        [Tooltip("Horizontal offset from center in the move direction where the downward ground ray originates. Set to roughly half the enemy collider width.")]
        public float groundCheckOffset = 0.4f;

        [Tooltip("How far downward to raycast for ground. If nothing hit, direction reverses.")]
        public float groundCheckDistance = 0.6f;

        [Tooltip("Layer(s) that count as solid ground for edge detection. Usually 'Default' (ocean floor tilemap). If empty, falls back to wallLayer.")]
        public LayerMask groundLayer;

        [Header("Attack")]
        [Tooltip("Damage dealt to the player when the attack hitbox connects. Requires an EnemyAttackHitbox child object on the prefab.")]
        public float attackDamage = 10f;

        public override bool RequiresNavMesh => false;
        public override IEnemyBrain CreateBrain() => new LinearPatrolBrain(this);
    }
}
