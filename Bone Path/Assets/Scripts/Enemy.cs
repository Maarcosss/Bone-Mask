using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    private int currentHealth;
    public bool isDead = false;

    public float knockbackForce = 5f;
    private Rigidbody rb;
    private Vector3 knockbackVelocity = Vector3.zero;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // no afecta la física normal
    }

    void Update()
    {
        if (knockbackVelocity.magnitude > 0.1f)
        {
            rb.position += knockbackVelocity * Time.deltaTime;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, 5f * Time.deltaTime);
        }
    }

    // Se llama desde PlayerAttack
    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Knockback
        knockbackVelocity = hitDirection.normalized * knockbackForce;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.isTrigger = true;
        }

        rb.isKinematic = true;
    }
}
