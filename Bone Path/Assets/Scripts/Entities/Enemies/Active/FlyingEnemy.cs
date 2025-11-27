using System.Collections;
using UnityEngine;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float hoverSpeed = 1.5f;
    public float moveSpeed = 2f;
    public float retreatSpeed = 2f;
    public float hoverRange = 1f;

    [Header("Attack Settings")]
    public float attackSpeed = 20f;
    public float chargeDelay = 0.75f;
    public float cooldownTime = 5f;
    public float detectionRadius = 8f;
    public float retreatDistance = 0.75f;

    private Vector3 startPosition;
    private bool playerDetected = false;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;
    private Transform player;

    void Start()
    {
        startPosition = transform.position;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        playerDetected = distanceToPlayer <= detectionRadius;

        if (playerDetected && !isAttacking && cooldownTimer <= 0f)
        {
            StartCoroutine(AttackRoutine());
        }

        if (!isAttacking)
        {
            if (playerDetected)
            {
                MoveTowards(player.position, moveSpeed);
            }
            else
            {
                Hover();
            }
        }

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    void Hover()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverRange;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    //Handles attack routine including charge, attack and retreat
    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }

    //Retreat a bit in a natural direction
    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        Vector3 attackTarget = player.position;

        yield return new WaitForSeconds(chargeDelay);

        float distance = Vector3.Distance(transform.position, attackTarget);
        float elapsed = 0f;
        Vector3 startAttackPos = transform.position;

        while (elapsed < distance / attackSpeed)
        {
            transform.position = Vector3.Lerp(startAttackPos, attackTarget, elapsed * attackSpeed / distance);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = attackTarget;

        yield return new WaitForSeconds(0.1f);

        Vector3 retreatTarget = transform.position - (player.position - transform.position).normalized * retreatDistance;
        distance = Vector3.Distance(transform.position, retreatTarget);
        elapsed = 0f;
        startAttackPos = transform.position;

        while (elapsed < distance / retreatSpeed)
        {
            transform.position = Vector3.Lerp(startAttackPos, retreatTarget, elapsed * retreatSpeed / distance);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cooldownTimer = cooldownTime;
        isAttacking = false;
    }

    //Draws the detection radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
