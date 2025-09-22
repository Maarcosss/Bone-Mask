using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    public PlayerHealth playerHealthRef; // Arrastrar el Player en el inspector
    public TMP_Text soulText;         // Arrastrar el texto de la UI

    void Update()
    {
        if (playerHealthRef != null && soulText != null)
        {
            int soulValue = (int)playerHealthRef.GetCurrentSoul(); // Convertir a entero
            soulText.text = playerHealthRef.GetCurrentSoul().ToString();
        }
    }
}
