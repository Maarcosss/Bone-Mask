using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    private int currentHealth;
    public bool isDead = false;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;

    private Rigidbody rb;
    private Vector3 knockbackVelocity = Vector3.zero;
    private float knockbackTimer = 0f;
    private bool originalKinematic;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();

        // 🔧 GUARDAR ESTADO ORIGINAL Y OPTIMIZAR
        originalKinematic = rb.isKinematic;
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Suavizar movimiento
    }

    void FixedUpdate()
    {
        // ✅ SOLO PROCESAR KNOCKBACK SI ES NECESARIO
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;

            // Aplicar knockback suavemente
            Vector3 targetVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero,
                                                (knockbackDuration - knockbackTimer) / knockbackDuration);

            // Solo aplicar si no es kinematic
            if (!rb.isKinematic)
            {
                rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);
            }

            // Finalizar knockback
            if (knockbackTimer <= 0f)
            {
                knockbackVelocity = Vector3.zero;
                if (!isDead)
                {
                    rb.velocity = Vector3.zero;
                }
            }
        }
    }

    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isDead) return;

        currentHealth -= damage;

        // 🔧 KNOCKBACK MEJORADO
        if (!isDead)
        {
            // Temporalmente no kinematic para knockback
            rb.isKinematic = false;
            knockbackVelocity = hitDirection.normalized * knockbackForce;
            knockbackTimer = knockbackDuration;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        // 🔧 OPTIMIZAR COLLIDERS
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.isTrigger = true;
        }

        // Detener toda física
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        knockbackVelocity = Vector3.zero;
        knockbackTimer = 0f;
    }

    // Método para verificar si está en knockback
    public bool IsInKnockback()
    {
        return knockbackTimer > 0f;
    }
}
