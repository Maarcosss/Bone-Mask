using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler_Controller : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int health = 3;
    private int currentHealth;

    [Header("Behaviour")]
    private bool isChasing;
    private bool isAlive;
    private bool movingRight = true;
    private float lastAttackTime = 0f;
    private float attackCooldown = 1f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float loseInterestRange = 12f;
    public Transform leftPoint;
    public Transform rightPoint;

    private Rigidbody rb_Crawler;
    private Transform player;

    private void Start()
    {
        rb_Crawler = GetComponent<Rigidbody>();
        rb_Crawler.useGravity = true;          // Activar gravedad
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = health;
        isAlive = true;
    }

    private void FixedUpdate()
    {
        if (!isAlive) return;

        // Comprobar distancia al jugador
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
            isChasing = true;
        else if (distanceToPlayer > loseInterestRange)
            isChasing = false;

        if (isChasing)
            ChasePlayer();
        else
            Patrol();
    }

    private void Patrol()
    {
        Vector3 target = movingRight ? rightPoint.position : leftPoint.position;
        Vector3 dir = (target - transform.position).normalized;

        // Solo moverse horizontalmente
        dir.y = 0;

        rb_Crawler.MovePosition(transform.position + dir * patrolSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, target) < 0.2f)
            FlipDirection();
    }

    private void ChasePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        rb_Crawler.MovePosition(transform.position + dir * chaseSpeed * Time.fixedDeltaTime);

        // Salto al atacar
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            JumpAttack();
            lastAttackTime = Time.time;
        }
    }

    private void FlipDirection()
    {
        movingRight = !movingRight;
    }

    private void JumpAttack()
    {
        // Solo aplicar si está tocando el suelo
        if (IsGrounded())
        {
            rb_Crawler.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        // Raycast hacia abajo para detectar suelo
        return Physics.Raycast(transform.position, Vector3.down, 0.6f);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!isAlive) return;

        if (collision.gameObject.CompareTag("Player") && Time.time - lastAttackTime >= attackCooldown)
        {
            // collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(contactDamage);
            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(int amount)
    {
        if (!isAlive) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isAlive = false;
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
