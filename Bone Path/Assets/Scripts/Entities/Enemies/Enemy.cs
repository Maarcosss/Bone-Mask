using UnityEngine;

[System.Serializable]
public class CoinDropSettings
{
    [Header("Coin Drop Settings")]
    public bool dropCoins = true;
    public int minCoins = 1;
    public int maxCoins = 3;
    [Range(0f, 100f)]
    public float dropChance = 80f;
    public GameObject coinPrefab;
    public float dropForce = 5f;
    public float dropHeight = 1f;
    public float scatterRadius = 1.5f;
}

public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    protected int currentHealth;
    public bool isDead = false;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.3f;
    public float knockbackDrag = 8f;

    [Header("Currency System")]
    public CoinDropSettings coinDropSettings = new CoinDropSettings();

    protected Rigidbody rb;
    protected Transform playerTransform;
    protected Vector3 knockbackVelocity = Vector3.zero;
    protected float knockbackTimer = 0f;
    protected bool originalKinematic;

    //Initialize enemy
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            originalKinematic = rb.isKinematic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    //Handle knockback physics
    protected virtual void FixedUpdate()
    {
        if (knockbackTimer > 0f && rb != null)
        {
            knockbackTimer -= Time.fixedDeltaTime;

            if (!rb.isKinematic)
            {
                Vector3 currentVel = rb.velocity;
                currentVel.x = Mathf.Lerp(currentVel.x, 0f, knockbackDrag * Time.fixedDeltaTime);
                currentVel.z = Mathf.Lerp(currentVel.z, 0f, knockbackDrag * Time.fixedDeltaTime);
                rb.velocity = currentVel;
            }

            if (knockbackTimer <= 0f)
            {
                knockbackVelocity = Vector3.zero;
                if (rb != null && !isDead)
                {
                    Vector3 vel = rb.velocity;
                    vel.x = 0f;
                    vel.z = 0f;
                    rb.velocity = vel;
                }
            }
        }
    }

    //Apply damage to enemy
    public virtual void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (!isDead && rb != null)
        {
            rb.isKinematic = false;
            Vector3 knockback = hitDirection.normalized * knockbackForce;
            knockback.y = 0f;
            rb.velocity = new Vector3(knockback.x, rb.velocity.y, knockback.z);
            knockbackTimer = knockbackDuration;
        }

        if (currentHealth <= 0) Die();
    }

    //Handle enemy death
    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;

        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders) col.isTrigger = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        knockbackVelocity = Vector3.zero;
        knockbackTimer = 0f;

        DropCoins();
        Destroy(gameObject, 2f);
    }

    //Handle coin drop
    protected virtual void DropCoins()
    {
        if (!coinDropSettings.dropCoins) return;
        if (Random.Range(0f, 100f) > coinDropSettings.dropChance) return;

        float playerZ = playerTransform != null ? playerTransform.position.z : transform.position.z;

        if (CurrencySystem.Instance == null) return;

        int coinsToDrop = Random.Range(coinDropSettings.minCoins, coinDropSettings.maxCoins + 1);

        for (int i = 0; i < coinsToDrop; i++)
        {
            if (coinDropSettings.coinPrefab == null)
            {
                CurrencySystem.Instance.AddCoins(1);
            }
            else
            {
                Vector3 dropPos = new Vector3(
                    transform.position.x,
                    transform.position.y + coinDropSettings.dropHeight,
                    playerZ
                );

                Vector3 randomOffset = new Vector3(
                    Random.Range(-coinDropSettings.scatterRadius, coinDropSettings.scatterRadius),
                    Random.Range(0f, coinDropSettings.dropHeight * 0.3f),
                    0f
                );
                dropPos += randomOffset;
                dropPos.z = playerZ;

                GameObject droppedCoin = Instantiate(coinDropSettings.coinPrefab, dropPos, Quaternion.identity);

                CoinPickup coinPickup = droppedCoin.GetComponent<CoinPickup>();
                if (coinPickup != null) coinPickup.SetupPhysics(true, playerZ);

                Rigidbody coinRb = droppedCoin.GetComponent<Rigidbody>();
                if (coinRb != null)
                {
                    coinRb.constraints = RigidbodyConstraints.FreezePositionZ |
                                         RigidbodyConstraints.FreezeRotationX |
                                         RigidbodyConstraints.FreezeRotationZ;

                    Vector3 forceDir;
                    int dir = Random.Range(0, 3);
                    switch (dir)
                    {
                        case 0: forceDir = new Vector3(-1f, 0.5f, 0f); break;
                        case 1: forceDir = new Vector3(1f, 0.5f, 0f); break;
                        default: forceDir = new Vector3(Random.Range(-0.3f, 0.3f), 1f, 0f); break;
                    }

                    coinRb.AddForce(forceDir.normalized * coinDropSettings.dropForce, ForceMode.Impulse);
                }
            }
        }
    }

    //Check if enemy is in knockback
    public bool IsInKnockback() => knockbackTimer > 0f;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
}
