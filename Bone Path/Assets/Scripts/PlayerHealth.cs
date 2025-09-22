using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    // UI corazones (asignar en Inspector)
    public Image heart1;
    public Image heart2;
    public Image heart3;

    public Sprite fullHeart;
    public Sprite emptyHeart;

    // Vida
    int currentHealth = 3;
    int maxHealth = 3;

    // Alma (recurso para curarse)
    public float maxSoul = 100f;      // alma máxima
    public float currentSoul = 0f;    // alma actual
    [Range(0.01f, 1f)]
    public float healCostPercent = 0.5f; // porcentaje del maxSoul que cuesta 1 curación (0.5 = 50%)

    // Curación por mantener tecla
    public float healTime = 1.0f; // tiempo que hay que mantener E para curar 1 corazón
    private float healTimer = 0f;
    private bool isHealing = false;

    // Invencibilidad/interrupción
    private bool isInvincible = false;
    private float invincibleTime = 1.0f;
    private float invincibleTimer = 0f;
    public int GetCurrentHealth() => currentHealth;
    public float GetCurrentSoul() => currentSoul;

    
    void Start()
    {
        // ajustar por si currentSoul > maxSoul
        if (currentSoul > maxSoul) currentSoul = maxSoul;
        UpdateHeartsUI();
        UpdateSoulUI(); // opcional (implementa esta función para mostrar barra de alma)
    }

    void Update()
    {
        // temporizador de invencibilidad
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f)
            {
                isInvincible = false;
            }
        }

        // Comenzar o mantener curación con E
        if (Input.GetKey(KeyCode.E))
        {
            // Solo si no estás full health y tienes alma suficiente para una curación
            if (currentHealth < maxHealth && HasEnoughSoulForOneHeal())
            {
                if (!isHealing)
                {
                    // iniciar conteo
                    isHealing = true;
                    healTimer = healTime;
                    // aquí podrías bloquear movimiento/ataque si quieres
                    Debug.Log("Iniciando curación... mantén E");
                }

                // descontar tiempo manteniéndola
                if (isHealing)
                {
                    healTimer -= Time.deltaTime;

                    // Si completó el hold-time
                    if (healTimer <= 0f)
                    {
                        DoHealOneHeart();

                        // Si todavía tienes alma y sigues manteniendo, reinicia para intentar otra curación
                        if (currentHealth < maxHealth && HasEnoughSoulForOneHeal())
                        {
                            healTimer = healTime; // volver a empezar para curar siguiente corazón
                            // isHealing queda true
                        }
                        else
                        {
                            isHealing = false; // no más curación
                        }
                    }
                }
            }
            else
            {
                // No se puede iniciar curación (salud completa o alma insuficiente)
                if (isHealing)
                {
                    isHealing = false;
                }
            }
        }
        else
        {
            // Si suelta la tecla, cancelar la curación en progreso (sin consumir alma)
            if (isHealing)
            {
                isHealing = false;
                // opcional: resetear healTimer = healTime;
                Debug.Log("Curación interrumpida (soltaste E)");
            }
        }

        // TEST rápido: perder vida con H (para debug)
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1);
        }
    }

    public void SetCurrentHealth(int value)
    {
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        UpdateHeartsUI();
    }

    public void SetCurrentSoul(float value)
    {
        currentSoul = Mathf.Clamp(value, 0f, maxSoul);
        UpdateSoulUI();
    }

    // Realiza la curación: consume alma y suma 1 vida (si no está a tope)
    void DoHealOneHeart()
    {
        float cost = maxSoul * healCostPercent;

        if (currentSoul >= cost && currentHealth < maxHealth)
        {
            currentSoul -= cost;
            if (currentSoul < 0f) currentSoul = 0f;

            currentHealth++;
            UpdateHeartsUI();
            UpdateSoulUI();

            Debug.Log("Curó 1 corazón. Alma restante: " + currentSoul);
        }
        else
        {
            Debug.Log("No hay alma suficiente para curar.");
            // si por alguna razón no hay alma suficiente (ej. se gastó en otra parte), cancelar
            isHealing = false;
        }
    }

    // Comprueba si tienes suficiente alma para curar 1 corazón
    bool HasEnoughSoulForOneHeal()
    {
        float cost = maxSoul * healCostPercent;
        if (currentSoul >= cost) return true;
        return false;
    }

    // Añadir alma (llámalo desde el sistema de combate o al golpear enemigos)
    public void AddSoul(float amount)
    {
        currentSoul += amount;
        if (currentSoul > maxSoul) currentSoul = maxSoul;
        UpdateSoulUI();
    }

    // Cuando el jugador recibe daño
    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        // Si estaba curando, interrumpe sin consumir alma
        if (isHealing)
        {
            isHealing = false;
            Debug.Log("Curación interrumpida por daño.");
        }

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHeartsUI();

        // activar invencibilidad breve para evitar perder múltiples corazones de golpe
        isInvincible = true;
        invincibleTimer = invincibleTime;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Actualizar UI de corazones (sin arrays ni ternarios)
    void UpdateHeartsUI()
    {
        if (currentHealth >= 1)
        {
            heart1.sprite = fullHeart;
        }
        else
        {
            heart1.sprite = emptyHeart;
        }

        if (currentHealth >= 2)
        {
            heart2.sprite = fullHeart;
        }
        else
        {
            heart2.sprite = emptyHeart;
        }

        if (currentHealth >= 3)
        {
            heart3.sprite = fullHeart;
        }
        else
        {
            heart3.sprite = emptyHeart;
        }
    }

    // Placeholder para actualizar UI de alma; implementa tu barra aquí o conecta la UI que quieras
    void UpdateSoulUI()
    {
        // Ejemplo de debug, puedes reemplazar con actualización de Image.fillAmount, texto, etc.
        // Debug.Log("Alma: " + currentSoul + " / " + maxSoul);
    }

    void Die()
    {
        Debug.Log("Jugador ha muerto");
        // reiniciar nivel, mostrar pantalla de muerte, etc.
    }
}
