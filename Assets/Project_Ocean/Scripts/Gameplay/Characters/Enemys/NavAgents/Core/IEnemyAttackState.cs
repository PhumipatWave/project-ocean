/// <summary>
/// Optional behaviour on fish agents that report attack animation state to Enemy.
/// </summary>
public interface IEnemyAttackState
{
    bool IsAttacking { get; }
}
