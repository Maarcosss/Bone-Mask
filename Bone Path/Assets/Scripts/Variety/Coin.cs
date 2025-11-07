using UnityEngine;

public class Coin : MonoBehaviour
{
    private CoinDropper dropper;
    private float lifetime;

    public void Setup(CoinDropper dropper, float lifetime)
    {
        this.dropper = dropper;
        this.lifetime = lifetime;
        Destroy(gameObject, lifetime);
    }

}
