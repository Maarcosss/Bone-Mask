using UnityEngine;
using UnityEngine.InputSystem;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class AttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    public int damageAmount = 1;
    public float soulPerHit = 10f;
    public float attackCooldown = 0.5f;
    public Vector3 hitboxSize = new Vector3(1f, 1f, 1f);
    public Vector3 hitboxOffset = new Vector3(1f, 0f, 0f);

    [Header("References")]
    public Player playerRef;
    public InputActionReference attackAction;

    private float attackTimer = 0f;

    void Start()
    {
        if (playerRef == null)
        {
            playerRef = FindObjectOfType<Player>();
        }
    }

    void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.action.Disable();
        }
    }

    void Update()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }

        if (attackAction != null && attackAction.action.ReadValue<float>() > 0.1f && attackTimer <= 0f)
        {
            PerformAttack();
            attackTimer = attackCooldown;
        }
    }

    //Performs attack, applies damage to enemies and breakable objects, and adds soul to player
    private void PerformAttack()
    {
        Vector3 attackPosition = transform.position + transform.TransformDirection(hitboxOffset);
        Collider[] hits = Physics.OverlapBox(attackPosition, hitboxSize * 0.5f, transform.rotation);

        foreach (Collider col in hits)
        {
            if (col.CompareTag("Enemy"))
            {
                PassiveEnemyAI passiveEnemy = col.GetComponent<PassiveEnemyAI>();
                if (passiveEnemy != null)
                {
                    passiveEnemy.TakeDamage(damageAmount);
                    if (playerRef != null)
                    {
                        playerRef.soul += soulPerHit;
                        if (playerRef.soul > playerRef.maxSoul)
                        {
                            playerRef.soul = playerRef.maxSoul;
                        }
                        playerRef.UpdateSoulUI();
                    }
                }

                ActiveEnemyAI activeEnemy = col.GetComponent<ActiveEnemyAI>();
                if (activeEnemy != null)
                {
                    activeEnemy.TakeDamage(damageAmount);
                    if (playerRef != null)
                    {
                        playerRef.soul += soulPerHit;
                        if (playerRef.soul > playerRef.maxSoul)
                        {
                            playerRef.soul = playerRef.maxSoul;
                        }
                        playerRef.UpdateSoulUI();
                    }
                }
            }
            BreakableObject breakable = col.GetComponent<BreakableObject>();
            if (breakable != null)
            {
                breakable.TakeDamage(damageAmount);
            }
        }
    }

    //Draws the attack hitbox in the editor for visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 attackPosition = transform.position + transform.TransformDirection(hitboxOffset);
        Gizmos.matrix = Matrix4x4.TRS(attackPosition, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
    }
}
