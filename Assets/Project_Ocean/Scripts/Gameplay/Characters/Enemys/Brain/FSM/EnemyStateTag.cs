/// <summary>
/// Identifies each state in the enemy FSM.
/// Each brain type only registers the tags it actually uses.
/// </summary>
public enum EnemyStateTag
{
    Wander,
    Follow,
    Search,
    Prepare,
    Attack,
    Panic,
    Flock,
}
