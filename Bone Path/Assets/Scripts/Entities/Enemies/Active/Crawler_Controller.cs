using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Author: David Gomez
Date: 20 - Nov - 2025*/

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
        rb_Crawler.useGravity = true;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHealth = health;
        isAlive = true;
    }

    //Checks distance to player and decides between chasing or patrolling
    private void FixedUpdate()
    {
        if (!isAlive) return;

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

    //Handles patrol movement between left and right points
    private void Patrol()
    {
        Vector3 target = movingRight ? rightPoint.position : leftPoint.position;
        Vector3 dir = (target - transform.position).normalized;

        dir.y = 0;

        rb_Crawler.MovePosition(transform.position + dir * patrolSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, target) < 0.2f)
            FlipDirection();
    }

    //Moves toward the player and performs jump attacks
    private void ChasePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        rb_Crawler.MovePosition(transform.position + dir * chaseSpeed * Time.fixedDeltaTime);

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            JumpAttack();
            lastAttackTime = Time.time;
        }
    }

    //Switches direction when reaching patrol boundaries
    private void FlipDirection()
    {
        movingRight = !movingRight;
    }

    //Performs a jump attack if grounded
    private void JumpAttack()
    {
        if (IsGrounded())
        {
            rb_Crawler.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    //Checks if enemy is on the ground using raycast
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.6f);
    }

    //Handles collision with player and attack cooldown
    private void OnCollisionStay(Collision collision)
    {
        if (!isAlive) return;

        if (collision.gameObject.CompareTag("Player") && Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
        }
    }

    //Reduces health and triggers death if needed
    public void TakeDamage(int amount)
    {
        if (!isAlive) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
            Die();
    }

    //Handles enemy death and destruction
    private void Die()
    {
        isAlive = false;
        Destroy(gameObject);
    }

    //Draws detection gizmo in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
