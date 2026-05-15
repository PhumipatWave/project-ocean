using UnityEngine;

/// <summary>
/// Handles 2D sprite flip (on visual child) and body rotation for nav-mesh enemies.
/// </summary>
public class EnemyFacingController
{
    private readonly Transform body;
    private Transform visualRoot;

    private readonly float turnSmoothTime;
    private readonly float maxTiltAngle;
    private readonly float flipDeadZone;
    private readonly float flipDelay;
    private readonly float flipCooldown;
    private readonly float spriteAngleOffset;
    private readonly float attackAngleThreshold;
    private readonly FacingTurnMode turnMode;
    private readonly float turnSpeed;

    private bool facingRight = true;
    private float turnVelocity;
    private float flipTimer;
    private float flipCooldownTimer;
    private bool suppressRotation;

    public enum FacingTurnMode
    {
        SmoothTime,
        TurnSpeed
    }

    public bool FacingRight => facingRight;

    public EnemyFacingController(
        Transform body,
        Transform visualRoot,
        float turnSmoothTime,
        float maxTiltAngle,
        float flipDeadZone,
        float flipDelay,
        float flipCooldown,
        float spriteAngleOffset,
        float attackAngleThreshold,
        FacingTurnMode turnMode = FacingTurnMode.SmoothTime,
        float turnSpeed = 5f)
    {
        this.body = body;
        this.visualRoot = visualRoot;
        this.turnSmoothTime = turnSmoothTime;
        this.maxTiltAngle = maxTiltAngle;
        this.flipDeadZone = flipDeadZone;
        this.flipDelay = flipDelay;
        this.flipCooldown = flipCooldown;
        this.spriteAngleOffset = spriteAngleOffset;
        this.attackAngleThreshold = attackAngleThreshold;
        this.turnMode = turnMode;
        this.turnSpeed = turnSpeed;
    }

    public void Initialize()
    {
        visualRoot = EnemyFacing2D.ResolveVisualRoot(body, visualRoot);
        facingRight = EnemyFacing2D.ReadFacingRight(body, visualRoot);
    }

    public void SetSuppressRotation(bool suppress)
    {
        suppressRotation = suppress;
    }

    public Vector2 GetFacingDirection()
    {
        Vector2 dir = facingRight ? body.right : -body.right;
        return dir.normalized;
    }

    public void RotateFromVelocity(Vector2 velocity, float minSpeed = 0.01f)
    {
        if (suppressRotation || velocity.sqrMagnitude < minSpeed * minSpeed)
            return;

        RotateTowards(velocity.normalized);
    }

    public bool RotateTowards(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f)
            return true;

        if (flipCooldownTimer > 0f)
            flipCooldownTimer -= Time.deltaTime;

        bool wantsFaceRight = facingRight;

        if (dir.x > flipDeadZone)
            wantsFaceRight = true;
        else if (dir.x < -flipDeadZone)
            wantsFaceRight = false;

        if (wantsFaceRight != facingRight)
        {
            flipTimer += Time.deltaTime;

            if (flipTimer >= flipDelay && flipCooldownTimer <= 0f)
            {
                facingRight = wantsFaceRight;
                flipTimer = 0f;
                flipCooldownTimer = flipCooldown;
                turnVelocity = 0f;
            }
        }
        else
        {
            flipTimer = 0f;
        }

        EnemyFacing2D.ApplyHorizontalFlip(body, visualRoot, facingRight);

        float targetAngle = facingRight
            ? Mathf.Atan2(dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg
            : Mathf.Atan2(-dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg;

        targetAngle = Mathf.Clamp(targetAngle, -maxTiltAngle, maxTiltAngle);
        targetAngle += spriteAngleOffset;

        float smoothDuration = turnMode == FacingTurnMode.TurnSpeed
            ? 1f / Mathf.Max(turnSpeed, 0.01f)
            : turnSmoothTime;

        float smoothAngle = Mathf.SmoothDampAngle(
            body.eulerAngles.z,
            targetAngle,
            ref turnVelocity,
            smoothDuration
        );

        body.rotation = Quaternion.Euler(0f, 0f, smoothAngle);

        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(body.eulerAngles.z, targetAngle));
        return angleDifference <= attackAngleThreshold;
    }
}
