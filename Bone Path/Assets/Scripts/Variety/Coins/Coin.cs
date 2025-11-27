using UnityEngine;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class Coin : MonoBehaviour
{
    private CoinDropper dropper;
    private float lifetime;

    //Initialize coin with dropper reference and set self-destruction timer
    public void Setup(CoinDropper dropper, float lifetime)
    {
        this.dropper = dropper;
        this.lifetime = lifetime;
        Destroy(gameObject, lifetime);
    }

}
