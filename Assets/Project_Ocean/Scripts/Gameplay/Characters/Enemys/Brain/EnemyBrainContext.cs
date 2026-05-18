using Game.Testing;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Shared references passed from the nav host into every enemy brain.
/// </summary>
public class EnemyBrainContext
{
    public Enemy Owner;
    public EnemyConfig Config;
    public Transform Transform;
    public NavMeshAgent Agent;
    public EnemyFacingController Facing;
    public FishAlertUI AlertUI;
    public MonoBehaviour Host;

    public Player Player => Owner != null ? Owner.Player : null;

    public Transform PlayerTransform => Player != null ? Player.transform : null;

    public bool IsDead => Owner != null && Owner.IsDeath;
}
