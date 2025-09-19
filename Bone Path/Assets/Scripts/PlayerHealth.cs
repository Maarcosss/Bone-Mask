using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Image heart1;
    public Image heart2;
    public Image heart3;

    public Sprite fullHeart;
    public Sprite emptyHeart;

    private int currentHealth = 3;
    private int maxHealth = 3;

    private bool isHealing = false;
    private float healTime = 2f; // tiempo que tarda en curar
    private float healTimer = 0f;

    void Start()
    {
        UpdateHeartsUI();
    }

    void Update()
    {
        // Iniciar curación si pulsa LeftShift
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isHealing && currentHealth < maxHealth)
        {
            StartHealing();
        }

        // Mientras cura
        if (isHealing)
        {
            healTimer -= Time.deltaTime;

            if (healTimer <= 0f)
            {
                FinishHealing();
            }
        }

        // TEST: perder vida con H
        if (Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(1);
        }
    }

    void StartHealing()
    {
        isHealing = true;
        healTimer = healTime;

        // Aquí puedes bloquear movimiento/ataque del jugador
        Debug.Log("Jugador empieza a curarse...");
    }

    void FinishHealing()
    {
        isHealing = false;

        if (currentHealth < maxHealth)
        {
            currentHealth++;
            UpdateHeartsUI();
            Debug.Log("Jugador se curó 1 corazón!");
        }

        // Aquí desbloqueas movimiento/ataque
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
