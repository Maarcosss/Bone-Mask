using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessManager : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private Volume globalVolume = null;

    [Header("Brightness Settings")]
    public float minBrightness = -1f;
    public float maxBrightness = 0f;

    public static BrightnessManager Instance { get; private set; }

    private ColorAdjustments colorAdjustments;
    private bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeBrightnessSystem();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    //Prepare the brightness system to function correctly when the game starts.
    void InitializeBrightnessSystem()
    {
        if (globalVolume == null)
        {
            globalVolume = FindObjectOfType<Volume>();
            if (globalVolume == null)
            {
                return;
            }
        }

        if (globalVolume.profile == null)
        {
            return;
        }

        if (!globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            return;
        }

        if (!colorAdjustments.active)
        {
            colorAdjustments.active = true;
        }

        isInitialized = true;

        float savedBrightness = PlayerPrefs.GetFloat("masterBrightness", 1f);
        SetBrightness(savedBrightness);
    }

    //Adjust the brightness of the game
    public void SetBrightness(float value)
    {
        if (!isInitialized)
        {
            return;
        }

        if (colorAdjustments == null)
        {
            return;
        }

        if (value < 0f)
        {
            value = 0f;
        }
        else if (value > 1f)
        {
            value = 1f;
        }

        float mappedValue = minBrightness + (maxBrightness - minBrightness) * value;
        colorAdjustments.postExposure.value = mappedValue;
        PlayerPrefs.SetFloat("masterBrightness", value);
    }

    //Current brightness value of the game
    public float GetBrightness()
    {
        if (!isInitialized || colorAdjustments == null)
        {
            return 1f;
        }

        float currentExposure = colorAdjustments.postExposure.value;

        float normalizedValue = (currentExposure - minBrightness) / (maxBrightness - minBrightness);

        if (normalizedValue < 0f)
        {
            normalizedValue = 0f;
        }
        else if (normalizedValue > 1f)
        {
            normalizedValue = 1f;
        }

        return normalizedValue;
    }

    //Resets the game brightness to the default value
    public void ResetBrightness()
    {
        SetBrightness(1f);
    }

    void OnValidate()
    {
        if (minBrightness > maxBrightness)
        {
            maxBrightness = minBrightness;
        }
    }
}
