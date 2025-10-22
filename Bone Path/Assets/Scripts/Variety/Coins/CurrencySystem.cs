using UnityEngine;
using TMPro;

public class CurrencySystem : MonoBehaviour
{
    public int currentCoins = 0;
    public int maxCoins = 9999;
    public TMP_Text coinText;
    public string coinFormat = "{0}";
    public AudioClip coinCollectSound;
    public float animationDuration = 0.3f;
    public float animationScale = 1.2f;
    public bool enableScaleAnimation = true;

    private int lastDisplayedCoins = 0;
    private AudioSource audioSource;
    private Coroutine currentAnimation;
    private Vector3 originalScale;
    public static CurrencySystem Instance { get; private set; }
    [System.NonSerialized] public System.Action<int> OnCoinsChanged;
    [System.NonSerialized] public System.Action<int> OnCoinsAdded;
    [System.NonSerialized] public System.Action<int> OnCoinsSpent;

    //Initialize singleton
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    //Start initialization
    void Start()
    {
        InitializeCurrencySystem();
    }

    //Setup currency system
    void InitializeCurrencySystem()
    {
        FindCoinText();
        SetupAudioSource();

        if (coinText != null)
        {
            originalScale = coinText.transform.localScale;
        }

        LoadCoins();
        UpdateCoinUI();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //Setup audio source
    void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    //Scene loaded callback
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        StartCoroutine(DelayedUISetup());
    }

    //Delay UI setup for proper initialization
    System.Collections.IEnumerator DelayedUISetup()
    {
        yield return null;
        FindCoinText();

        if (coinText != null)
        {
            if (originalScale != Vector3.zero)
            {
                coinText.transform.localScale = originalScale;
            }
        }

        UpdateCoinUI();
    }

    //Find coin UI text in scene
    void FindCoinText()
    {
        if (coinText == null)
        {
            GameObject coinTextObject = GameObject.Find("CoinText");
            if (coinTextObject != null)
            {
                coinText = coinTextObject.GetComponent<TMP_Text>();
            }

            if (coinText != null)
            {
                coinText = coinText.GetComponent<TMP_Text>();
            }
            else
            {
                GameObject coinTxtObject = GameObject.Find("CoinTXT");
                if (coinTxtObject != null)
                {
                    coinText = coinTxtObject.GetComponent<TMP_Text>();
                }

                if (coinText != null)
                {
                    coinText = coinText.GetComponent<TMP_Text>();
                }
                else
                {
                    Canvas canvas = FindObjectOfType<Canvas>();
                    if (canvas != null)
                    {
                        TMP_Text[] allTexts = canvas.GetComponentsInChildren<TMP_Text>();
                        for (int i = 0; i < allTexts.Length; i++)
                        {
                            TMP_Text text = allTexts[i];
                            if (text.name.ToLower().Contains("coin"))
                            {
                                coinText = text;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    //Cleanup on destroy
    void OnDestroy()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //Add coins to current total
    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int previousCoins = currentCoins;
        currentCoins = Mathf.Min(currentCoins + amount, maxCoins);
        int actualAdded = currentCoins - previousCoins;

        if (actualAdded > 0)
        {
            UpdateCoinUI();
            PlayCoinSound();
            SaveCoins();
            OnCoinsChanged?.Invoke(currentCoins);
            OnCoinsAdded?.Invoke(actualAdded);
        }
    }

    //Spend coins if enough are available
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || currentCoins < amount)
        {
            return false;
        }

        currentCoins -= amount;
        UpdateCoinUI();
        SaveCoins();
        OnCoinsChanged?.Invoke(currentCoins);
        OnCoinsSpent?.Invoke(amount);

        return true;
    }

    //Check if player has enough coins
    public bool HasEnoughCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Get current coin count
    public int GetCurrentCoins()
    {
        return currentCoins;
    }

    //Set coin count directly
    public void SetCoins(int amount)
    {
        currentCoins = Mathf.Clamp(amount, 0, maxCoins);
        UpdateCoinUI();
        SaveCoins();
        OnCoinsChanged?.Invoke(currentCoins);
    }

    //Update coin UI
    void UpdateCoinUI()
    {
        if (coinText == null)
        {
            FindCoinText();
            if (coinText == null)
            {
                return;
            }
        }

        coinText.text = string.Format(coinFormat, currentCoins);

        if (enableScaleAnimation)
        {
            if (currentCoins != lastDisplayedCoins)
            {
                if (currentAnimation != null)
                {
                    StopCoroutine(currentAnimation);
                    coinText.transform.localScale = originalScale;
                }

                currentAnimation = StartCoroutine(AnimateCoinText());
                lastDisplayedCoins = currentCoins;
            }
        }
    }

    //Animate coin text scale
    System.Collections.IEnumerator AnimateCoinText()
    {
        if (coinText == null)
        {
            yield break;
        }

        if (originalScale == Vector3.zero)
        {
            yield break;
        }

        Vector3 targetScale = originalScale * animationScale;
        float elapsed = 0f;
        float halfDuration = animationDuration * 0.5f;

        while (elapsed < halfDuration)
        {
            if (coinText == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            coinText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        if (coinText != null)
        {
            coinText.transform.localScale = targetScale;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            if (coinText == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            t = Mathf.SmoothStep(0f, 1f, t);
            coinText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        if (coinText != null)
        {
            coinText.transform.localScale = originalScale;
        }

        currentAnimation = null;
    }

    //Play coin collect sound
    void PlayCoinSound()
    {
        if (audioSource != null)
        {
            if (coinCollectSound != null)
            {
                audioSource.PlayOneShot(coinCollectSound);
            }
        }
    }

    //Save coin count to playerprefs
    void SaveCoins()
    {
        PlayerPrefs.SetInt("PlayerCoins", currentCoins);
        PlayerPrefs.Save();
    }

    //Load coin count from playerprefs
    void LoadCoins()
    {
        currentCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
        currentCoins = Mathf.Clamp(currentCoins, 0, maxCoins);
    }

    //Reset text scale to original
    public void ResetTextScale()
    {
        if (coinText != null)
        {
            if (originalScale != Vector3.zero)
            {
                if (currentAnimation != null)
                {
                    StopCoroutine(currentAnimation);
                    currentAnimation = null;
                }

                coinText.transform.localScale = originalScale;
            }
        }
    }

    //Enable or disable scale animation
    public void SetAnimationEnabled(bool enabled)
    {
        enableScaleAnimation = enabled;

        if (!enabled)
        {
            if (currentAnimation != null)
            {
                StopCoroutine(currentAnimation);
                if (coinText != null)
                {
                    if (originalScale != Vector3.zero)
                    {
                        coinText.transform.localScale = originalScale;
                    }
                }
                currentAnimation = null;
            }
        }
    }

    //Reset coins to 0
    public void ResetCoins()
    {
        SetCoins(0);
    }

    //Force UI refresh
    public void ForceResetUI()
    {
        coinText = null;
        FindCoinText();

        if (coinText != null)
        {
            originalScale = coinText.transform.localScale;
            UpdateCoinUI();
        }
    }

    //Get system information as string
    public string GetSystemInfo()
    {
        string uiStatus = "No";
        if (coinText != null)
        {
            uiStatus = "Yes";
        }

        string audioStatus = "No";
        if (audioSource != null)
        {
            audioStatus = "Yes";
        }

        string animationStatus = "No";
        if (enableScaleAnimation)
        {
            animationStatus = "Yes";
        }

        return "Coins: " + currentCoins + "/" + maxCoins + " | UI: " + uiStatus + " | Audio: " + audioStatus + " | Animation: " + animationStatus + " | Scale: " + originalScale;
    }
}
