using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Image heart1;
    public Image heart2;
    public Image heart3;

    public Sprite fullHeart;
    public Sprite emptyHeart;

    int currentHealth = 3;
    int maxHealth = 3;

    bool isHealing = false;
    public float healTime = 1f; // tiempo que tarda en curar
    public float healTimer = 0f;

    void Start()
    {
        UpdateHeartsUI();
    }

    void Update()
    {
        // Si mantiene pulsada la E y aún no está a tope de vida
        if (Input.GetKey(KeyCode.E) && currentHealth < maxHealth)
        {
            if (!isHealing)
            {
                isHealing = true;
                healTimer = healTime;
                Debug.Log("Manteniendo E para curarse...");
            }

            // va descontando tiempo
            healTimer -= Time.deltaTime;

            if (healTimer <= 0f)
            {
                HealOneHeart();
                isHealing = false; // reinicia para que vuelva a requerir mantener la tecla
            }
        }
        else
        {
            // si suelta la tecla, se cancela la curación
            if (isHealing)
            {
                Debug.Log("Se interrumpió la curación.");
            }
            isHealing = false;
        }

        // TEST: perder vida con H
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1);
        }
    }

    void HealOneHeart()
    {
        if (currentHealth < maxHealth)
        {
            currentHealth++;
            UpdateHeartsUI();
            Debug.Log("Jugador se curó 1 corazón!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isHealing) return; // Opcional: interrumpir curación con daño

        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHeartsUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHeartsUI()
    {
        if (currentHealth >= 1) heart1.sprite = fullHeart; else heart1.sprite = emptyHeart;
        if (currentHealth >= 2) heart2.sprite = fullHeart; else heart2.sprite = emptyHeart;
        if (currentHealth >= 3) heart3.sprite = fullHeart; else heart3.sprite = emptyHeart;
    }

    void Die()
    {
        Debug.Log("Jugador ha muerto");
        // reiniciar nivel, mostrar pantalla de muerte, etc.
    }
}
