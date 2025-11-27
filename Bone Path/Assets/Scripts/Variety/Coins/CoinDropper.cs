using UnityEngine;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class CoinDropper : MonoBehaviour
{
    [Header("Coin Drop Settings")]
    public GameObject coinPrefab;
    public int coinsToDrop = 3;
    public float spreadRadius = 1.5f;
    public float coinLifetime = 10f;

    //Instantiate multiple coins with random offsets and setup their lifetime
    public void DropCoins()
    {
        if (coinPrefab == null || coinsToDrop <= 0)
        {
            return;
        }

        for (int i = 0; i < coinsToDrop; i++)
        {
            Vector3 randomOffset = new Vector3(Random.Range(-spreadRadius, spreadRadius), 0.5f, 0f);

            GameObject coin = Instantiate(coinPrefab, transform.position + randomOffset, Quaternion.identity);

            Rigidbody rb = coin.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            }

            Coin coinScript = coin.AddComponent<Coin>();
            coinScript.Setup(this, coinLifetime);
        }
    }
}