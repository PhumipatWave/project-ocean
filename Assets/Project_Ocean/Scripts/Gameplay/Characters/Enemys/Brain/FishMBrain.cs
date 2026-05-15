using UnityEngine;

public class FishMBrain : EnemyBrainBase
{
    private readonly FishMBrainSettings settings;

    private float attackTimer;
    private float wanderTimer;
    private float hitChaseTimer;
    private Vector2 currentDirection;
    private bool isFollowing;

    public FishMBrain(FishMBrainSettings settings)
    {
        this.settings = settings;
    }

    public override void Initialize(EnemyBrainContext context)
    {
        base.Initialize(context);
        Ctx.Agent.speed = settings.speed;
        wanderTimer = settings.wanderChangeInterval;
    }

    public override void Tick(float deltaTime)
    {
        if (Ctx.PlayerTransform == null || Ctx.Agent == null)
            return;

        if (hitChaseTimer > 0f)
        {
            hitChaseTimer -= deltaTime;
            isFollowing = true;
        }
        else
        {
            isFollowing = CanSeePlayer();
        }

        if (attackTimer > 0f)
            attackTimer -= deltaTime;

        if (isFollowing)
            TickFollowAndBite();
        else
            TickWander(deltaTime);

        TickWallAvoidance();
        TickFacing();
    }

    public void OnBulletHit()
    {
        if (settings.chasePlayerWhenHit)
            hitChaseTimer = settings.hitChaseDuration;
    }

    public void OnPlayerCollision()
    {
        if (settings.chasePlayerWhenHit)
            hitChaseTimer = settings.hitChaseDuration;
    }

    private bool CanSeePlayer()
    {
        return EnemyVisionHelper.HasLineOfSightRay(
            Ctx.Transform.position,
            Ctx.PlayerTransform.position,
            currentDirection,
            settings.viewDistance,
            settings.viewAngle,
            settings.obstacleLayer
        );
    }

    private void TickFollowAndBite()
    {
        Ctx.Agent.SetDestination(Ctx.PlayerTransform.position);

        if (Ctx.Agent.velocity.magnitude > 0.1f)
            currentDirection = Ctx.Agent.velocity.normalized;

        float dist = Vector2.Distance(Ctx.Transform.position, Ctx.PlayerTransform.position);

        if (dist <= settings.attackRange && attackTimer <= 0f)
        {
            Ctx.Player.TakeDamage(settings.attackDamage);
            attackTimer = settings.attackCooldown;
        }
    }

    private void TickWander(float deltaTime)
    {
        wanderTimer -= deltaTime;

        if (wanderTimer <= 0f || Ctx.Agent.remainingDistance < 0.2f)
        {
            Vector2 randomPos = Random.insideUnitCircle * settings.wanderRadius;
            Vector3 target = Ctx.Transform.position + new Vector3(randomPos.x, randomPos.y, 0f);
            Ctx.Agent.SetDestination(target);
            wanderTimer = settings.wanderChangeInterval;
        }

        if (Ctx.Agent.velocity.magnitude > 0.1f)
            currentDirection = Ctx.Agent.velocity.normalized;
    }

    private void TickWallAvoidance()
    {
        if (currentDirection == Vector2.zero)
            return;

        RaycastHit2D hit = Physics2D.Raycast(
            Ctx.Transform.position,
            currentDirection,
            settings.wallDetectDistance,
            settings.wallLayer
        );

        if (hit.collider == null)
            return;

        Vector2 avoidDir = Vector2.Perpendicular(currentDirection) * (Random.value > 0.5f ? 1f : -1f);
        Vector3 newTarget = Ctx.Transform.position + new Vector3(avoidDir.x, avoidDir.y, 0f);
        Ctx.Agent.SetDestination(newTarget);
    }

    private void TickFacing()
    {
        if (currentDirection.sqrMagnitude < 0.01f)
            return;

        Ctx.Facing.RotateTowards(currentDirection.normalized);
    }
}
