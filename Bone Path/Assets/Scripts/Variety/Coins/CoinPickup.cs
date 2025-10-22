using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int coinValue = 1;
    public bool autoDestroy = true;
    public float minimumCollisionForce = 0.1f;
    public AudioClip customPickupSound;

    private bool isCollected = false;
    private AudioSource audioSource;

    //Setup audio if a custom clip is assigned
    void Start()
    {
        if (customPickupSound != null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.7f;
            audioSource.playOnAwake = false;
        }
    }

    //Detect collision with player
    void OnCollisionEnter(Collision collision)
    {
        if (isCollected)
        {
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.relativeVelocity.magnitude >= minimumCollisionForce)
            {
                CollectCoin();
            }
        }
    }

    //Collect coin and apply effects
    void CollectCoin()
    {
        if (isCollected)
        {
            return;
        }

        isCollected = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (CurrencySystem.Instance != null)
        {
            CurrencySystem.Instance.AddCoins(coinValue);
        }

        if (customPickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(customPickupSound);
        }

        if (autoDestroy)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            float delay = customPickupSound != null ? Mathf.Min(customPickupSound.length, 0.5f) : 0.1f;
            Destroy(gameObject, delay);
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }
    }

    //Empty method for compatibility with Enemy.cs
    public void SetupPhysics(bool restrictZ = true, float zPos = 0f)
    {
    }

    //Set coin value dynamically
    public void SetCoinValue(int newValue)
    {
        coinValue = Mathf.Max(1, newValue);
    }

    //Reset coin for reuse
    public void ResetCoin()
    {
        isCollected = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}
