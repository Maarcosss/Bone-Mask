using UnityEngine;

public class PassiveEnemyAI : Enemy
{
    public float normalMoveSpeed = 2f;
    public float fleeSpeed = 5f;
    public float fleeDuration = 4f;
    public float wanderRadius = 8f;
    public float wanderInterval = 3f;
    public float playerDetectionRadius = 6f;
    public float fleeTimeout = 5f;
    public MovementPattern movementPattern = MovementPattern.Random;

    public enum MovementPattern { Random, Circle, BackAndForth }

    private Vector3 initialPosition;
    private Vector3 currentTarget;
    private float fleeTimer = 0f;
    private float wanderTimer = 0f;
    private bool isFleeing = false;
    private int backAndForthDirection = 1;
    private int wanderDirection = 1;
    private float lastDamageTime = 0f;
    private bool isFleeingFromPlayer = false;
    private bool wasAttacked = false;
    private bool isMoving = false;

    //Initialize enemy
    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position;
        InitializeMovement();
    }

    //Initialize movement target and direction
    void InitializeMovement()
    {
        wanderDirection = Random.Range(0, 2) == 0 ? -1 : 1;
        wanderTimer = Random.Range(0.5f, 1.5f);
        SetNewWanderTarget();
        isMoving = true;
    }

    //Update physics each frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (isDead || rb == null) return;
        if (IsInKnockback()) return;
        UpdateAI();
    }

    //Update AI logic
    void UpdateAI()
    {
        bool playerInRange = IsPlayerInRange();

        if (isFleeing)
        {
            fleeTimer -= Time.fixedDeltaTime;
            bool timeoutReached = (Time.time - lastDamageTime) >= fleeTimeout;
            bool playerTooFar = !playerInRange && !wasAttacked;

            if (fleeTimer <= 0f || timeoutReached || playerTooFar)
            {
                StopFleeing();
            }
            else
            {
                FleeFromPlayer();
                return;
            }
        }

        if (!isFleeing && playerInRange && !wasAttacked)
        {
            StartFleeingFromPlayer();
            return;
        }

        UpdateNormalMovement();
    }

    //Update normal wandering movement
    void UpdateNormalMovement()
    {
        wanderTimer -= Time.fixedDeltaTime;
        bool shouldChangeTarget = wanderTimer <= 0f;
        bool reachedTarget = Vector3.Distance(transform.position, currentTarget) < 1f;

        if (shouldChangeTarget || reachedTarget)
        {
            SetNewWanderTarget();
            wanderTimer = wanderInterval + Random.Range(-0.5f, 0.5f);
        }

        MoveToTarget();
        isMoving = true;
    }

    //Check if player is in detection range
    bool IsPlayerInRange()
    {
        if (playerTransform == null) return false;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        return distanceToPlayer <= playerDetectionRadius;
    }

    //Start fleeing from player
    void StartFleeingFromPlayer()
    {
        isFleeing = true;
        isFleeingFromPlayer = true;
        fleeTimer = fleeDuration;
        isMoving = true;

        if (playerTransform != null)
        {
            float fleeDirectionX = Mathf.Sign(transform.position.x - playerTransform.position.x);
            if (Mathf.Abs(fleeDirectionX) < 0.1f)
            {
                fleeDirectionX = Random.Range(0, 2) == 0 ? -1f : 1f;
            }

            wanderDirection = (int)fleeDirectionX;
        }
    }

    //Set a new wandering target based on pattern
    void SetNewWanderTarget()
    {
        Vector3 newTarget;

        switch (movementPattern)
        {
            case MovementPattern.Random:
                if (Random.Range(0f, 1f) < 0.3f)
                {
                    wanderDirection *= -1;
                }
                float randomDistance = Random.Range(3f, wanderRadius);
                newTarget = transform.position + new Vector3(wanderDirection * randomDistance, 0f, 0f);

                float distanceFromStart = Vector3.Distance(newTarget, initialPosition);
                if (distanceFromStart > wanderRadius)
                {
                    wanderDirection = (int)Mathf.Sign(initialPosition.x - transform.position.x);
                    newTarget = transform.position + new Vector3(wanderDirection * randomDistance * 0.5f, 0f, 0f);
                }
                break;

            case MovementPattern.Circle:
                float angle = Time.time * 0.5f;
                newTarget = initialPosition + new Vector3(
                    Mathf.Cos(angle) * wanderRadius * 0.5f,
                    0f,
                    Mathf.Sin(angle) * wanderRadius * 0.5f
                );
                break;

            case MovementPattern.BackAndForth:
                newTarget = initialPosition + new Vector3(backAndForthDirection * wanderRadius * 0.5f, 0f, 0f);
                if (Vector3.Distance(transform.position, newTarget) < 1f)
                {
                    backAndForthDirection *= -1;
                    newTarget = initialPosition + new Vector3(backAndForthDirection * wanderRadius * 0.5f, 0f, 0f);
                }
                break;

            default:
                newTarget = initialPosition;
                break;
        }

        currentTarget = newTarget;
    }

    //Move towards current target
    void MoveToTarget()
    {
        if (rb.isKinematic) return;
        float directionX = 0f;
        Vector3 targetVelocity = Vector3.zero;

        if (movementPattern == MovementPattern.Random || movementPattern == MovementPattern.BackAndForth)
        {
            directionX = Mathf.Sign(currentTarget.x - transform.position.x);
            if (Mathf.Abs(directionX) < 0.1f)
            {
                directionX = wanderDirection;
            }
            targetVelocity = new Vector3(directionX * normalMoveSpeed, rb.velocity.y, 0f);
        }
        else
        {
            Vector3 direction = (currentTarget - transform.position).normalized;
            direction.y = 0f;
            targetVelocity = direction * normalMoveSpeed;
            targetVelocity.y = rb.velocity.y;
        }

        rb.velocity = targetVelocity;
    }

    //Start fleeing due to attack
    void StartFleeing(Vector3 hitDirection)
    {
        isFleeing = true;
        isFleeingFromPlayer = false;
        wasAttacked = true;
        fleeTimer = fleeDuration;
        lastDamageTime = Time.time;
        isMoving = true;

        float fleeDirectionX;
        if (playerTransform != null)
        {
            fleeDirectionX = Mathf.Sign(transform.position.x - playerTransform.position.x);
        }
        else
        {
            fleeDirectionX = -Mathf.Sign(hitDirection.x);
        }

        if (Mathf.Abs(fleeDirectionX) < 0.1f)
        {
            fleeDirectionX = Random.Range(0, 2) == 0 ? -1f : 1f;
        }

        wanderDirection = (int)fleeDirectionX;
    }

    //Continue fleeing from player
    void FleeFromPlayer()
    {
        if (rb.isKinematic) return;
        if (isFleeingFromPlayer && playerTransform != null)
        {
            float currentFleeDirection = Mathf.Sign(transform.position.x - playerTransform.position.x);
            if (Mathf.Abs(currentFleeDirection - wanderDirection) > 0.5f)
            {
                wanderDirection = (int)currentFleeDirection;
            }
        }

        Vector3 targetVelocity = new Vector3(wanderDirection * fleeSpeed, rb.velocity.y, 0f);
        rb.velocity = targetVelocity;
        isMoving = true;
    }

    //Stop fleeing and resume normal movement
    void StopFleeing()
    {
        isFleeing = false;
        isFleeingFromPlayer = false;
        fleeTimer = 0f;

        if (wasAttacked && (Time.time - lastDamageTime) >= fleeTimeout)
        {
            wasAttacked = false;
        }

        SetNewWanderTarget();
        wanderTimer = Random.Range(0.5f, 1.5f);
        isMoving = true;
    }

    //Handle damage received
    public override void TakeDamage(int damage, Vector3 hitDirection)
    {
        base.TakeDamage(damage, hitDirection);
        lastDamageTime = Time.time;

        if (!isDead)
        {
            StartFleeing(hitDirection);
        }
    }

    //Check if enemy is moving
    public bool IsMoving()
    {
        return isMoving && rb != null && rb.velocity.magnitude > 0.1f;
    }

    //Draw gizmos for debugging
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? initialPosition : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);

        Gizmos.color = Color.blue;
        if (movementPattern == MovementPattern.Random || movementPattern == MovementPattern.BackAndForth)
        {
            Gizmos.DrawWireCube(center, new Vector3(wanderRadius * 2f, 1f, 1f));
        }
        else
        {
            Gizmos.DrawWireSphere(center, wanderRadius);
        }

        if (Application.isPlaying && currentTarget != Vector3.zero)
        {
            Gizmos.color = isFleeing ? Color.red : Color.green;
            Gizmos.DrawWireSphere(currentTarget, 0.5f);
            Gizmos.color = isMoving ? Color.green : Color.gray;
            Gizmos.DrawLine(transform.position, currentTarget);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = isFleeing ? Color.red : Color.cyan;
            Gizmos.DrawRay(transform.position, new Vector3(wanderDirection, 0f, 0f) * 2f);
        }

        if (Application.isPlaying && playerTransform != null && IsPlayerInRange())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}
