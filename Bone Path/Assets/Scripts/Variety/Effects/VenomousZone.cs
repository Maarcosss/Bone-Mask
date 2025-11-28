using UnityEngine;

public class VenomousZone : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damagePerTick = 1;
    public float tickRate = 2f;
    public string playerTag = "Player";

    private Player playerRef;
    private float tickTimer = 0f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            playerRef = collision.gameObject.GetComponent<Player>();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (playerRef == null) return;

        tickTimer += Time.deltaTime;

        if (tickTimer >= tickRate)
        {
            playerRef.health = Mathf.Max(playerRef.health - damagePerTick, 0);
            playerRef.UpdateHeartsUI();
            tickTimer = 0f;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            playerRef = null;
            tickTimer = 0f;
        }
    }
}
