using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Enemy FSM states; each brain registers only the ones it uses.</summary>
    public enum EnemyStateTag
    {
        Wander,
        Follow,
        Search,
        Prepare,
        Attack,
        Panic,
        Flee,
        Flock,
        Lurk,
        Skill,
        ReturnHome,
    }
}
