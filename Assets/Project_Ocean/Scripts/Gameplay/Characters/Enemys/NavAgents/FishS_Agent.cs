using Game.Testing;
using System.Collections;
using UnityEngine;

public class FishS_Agent : EnemyNavAgentBase
{
    [SerializeField] private FishSBrainSettings brainSettings;
    [SerializeField] private Transform playerTransform;

    public FishS_Agent leader;
    public int groupID;

    private FishSBrain fishSBrain;
    private bool isBouncing;

    public Vector2 MoveDirection => fishSBrain != null ? fishSBrain.CurrentDirection : Vector2.zero;

    private FishSBrainSettings BrainSettings
    {
        get
        {
            if (brainSettings == null)
                brainSettings = ScriptableObject.CreateInstance<FishSBrainSettings>();

            return brainSettings;
        }
    }

    protected override bool RequiresPlayer => false;

    protected override EnemyFacingController.FacingTurnMode DefaultFacingTurnMode =>
        EnemyFacingController.FacingTurnMode.TurnSpeed;

    protected override IEnemyBrain CreateBrain()
    {
        fishSBrain = new FishSBrain(BrainSettings);
        return fishSBrain;
    }

    protected override void OnBrainInitialized()
    {
        if (PlayerTransform != null)
            playerTransform = PlayerTransform;
        else if (GameManager.CurPlayer != null)
            playerTransform = GameManager.CurPlayer.transform;

        fishSBrain?.SetPlayerTransform(playerTransform);
    }

    protected override bool CanTick()
    {
        if (IsEnemyDead)
        {
            OnEnemyDead();
            return false;
        }

        if (isBouncing)
            return false;

        return Agent != null && Agent.isOnNavMesh;
    }

    public void FleeFrom(Vector2 targetPos)
    {
        fishSBrain?.FleeFrom(targetPos);
    }

    protected override void HandleBulletHit(GameObject bullet)
    {
        if (IsEnemyDead)
            return;

        if (bullet == null || !bullet.CompareTag(playerBulletTag))
            return;

        FleeFrom(bullet.transform.position);

        if (destroyBulletOnHit)
            Destroy(bullet);
    }

    protected override void HandlePlayerCollision()
    {
        if (playerTransform != null)
            FleeFrom(playerTransform.position);
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsEnemyDead || fishSBrain == null)
            return;

        if (((1 << collision.gameObject.layer) & BrainSettings.wallLayer) == 0)
            return;

        Vector2 normal = collision.contacts[0].normal;
        fishSBrain.ReflectDirection(normal, randomizePanic: true);

        StopAllCoroutines();
        StartCoroutine(WallBounceRoutine());
    }

    private IEnumerator WallBounceRoutine()
    {
        isBouncing = true;

        int attempts = 0;
        bool freeFromWall = false;

        while (attempts < 10 && !freeFromWall)
        {
            if (!fishSBrain.IsDirectionBlocked())
            {
                freeFromWall = true;
                break;
            }

            float angle = Random.Range(45f, 135f);
            fishSBrain.NudgeDirection(angle);
            attempts++;
            yield return null;
        }

        fishSBrain.ApplyBounceDestination();
        isBouncing = false;
    }
}
