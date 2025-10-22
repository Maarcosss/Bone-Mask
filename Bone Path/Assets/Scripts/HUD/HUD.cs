using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [Header("References")]
    public TMP_Text coinText;

    [Header("Coin Settings")]
    public string coinFormat = "{0}";
    public bool animateCoins = true;
    public float coinAnimationDuration = 0.3f;
    public float coinAnimationScale = 1.2f;

    private int lastCoinValue = -1;
    private string cachedCoinString = "";
    private bool coinAnimationInProgress = false;

    void Start()
    {
        UpdateCoinDisplay();
    }

    void Update()
    {
        UpdateCoinDisplay();
    }

    void UpdateCoinDisplay()
    {
        if (coinText == null || CurrencySystem.Instance == null)
        {
            return;
        }

        int currentCoins = CurrencySystem.Instance.GetCurrentCoins();

        if (currentCoins == lastCoinValue)
        {
            return;
        }

        lastCoinValue = currentCoins;
        cachedCoinString = string.Format(coinFormat, currentCoins);
        coinText.text = cachedCoinString;

        if (animateCoins && !coinAnimationInProgress)
        {
            StartCoroutine(AnimateCoinText());
        }
    }

    System.Collections.IEnumerator AnimateCoinText()
    {
        if (coinText == null)
        {
            yield break;
        }

        coinAnimationInProgress = true;
        Vector3 originalScale = coinText.transform.localScale;
        Vector3 targetScale = originalScale * coinAnimationScale;

        float elapsed = 0f;
        float halfDuration = coinAnimationDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            coinText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            coinText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        coinText.transform.localScale = originalScale;
        coinAnimationInProgress = false;
    }
}
