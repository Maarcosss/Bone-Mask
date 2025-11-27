using UnityEngine;

/*Author: Alejandro Cruz
Date: 20 - Nov - 2025*/

public class MudSlow : MonoBehaviour
{
    [SerializeField] private float slowFactor = 0.5f; 
    private Player player;
    private float originalSpeed;
    private bool isPlayerInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isPlayerInside && other.CompareTag("Player"))
        {
            player = other.GetComponent<Player>();
            if (player != null)
            {
                originalSpeed = player.GetSpeed(); 
                player.SetSpeed(originalSpeed * slowFactor);
                isPlayerInside = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPlayerInside && other.CompareTag("Player") && player != null)
        {
            player.SetSpeed(originalSpeed); 
            isPlayerInside = false;
        }
    }
}
