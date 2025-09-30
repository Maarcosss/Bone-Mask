using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    [Header("Input System")]
    public InputActionAsset inputActions;

    [Header("Save Settings")]
    [Tooltip("Mostrar mensajes de debug del sistema de guardado")]
    public bool showDebugMessages = true;

    [Tooltip("Guardar monedas automáticamente")]
    public bool saveCurrencyAutomatically = true;

    // Sistema de guardado
    private string saveFile;
    private bool nearCheckpoint = false;
    private Transform player;

    // Input System
    private InputAction saveAction;

    [System.Serializable]
    public class SaveData  // ← CAMBIADO DE 'class' A 'public class'
    {
        public float x, y, z;
        public int health;
        public float soul;
        public string scene;
        public int coins;
    }

    void Start()
    {
        saveFile = Application.persistentDataPath + "/save.json";

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("❌ No se encontró objeto con tag 'Player'");
        }

        SetupInputActions();
        LoadGame();

        // Suscribirse a eventos del sistema de monedas si está habilitado
        if (saveCurrencyAutomatically)
        {
            SubscribeToCurrencyEvents();
        }
    }

    void SubscribeToCurrencyEvents()
    {
        CurrencySystem.OnCoinsChanged += OnCoinsChanged;
    }

    void OnDestroy()
    {
        saveAction?.Disable();

        // Desuscribirse de eventos
        if (saveCurrencyAutomatically)
        {
            CurrencySystem.OnCoinsChanged -= OnCoinsChanged;
        }
    }

    void OnCoinsChanged(int newCoinAmount)
    {
        if (saveCurrencyAutomatically)
        {
            SaveCurrencyOnly();
        }
    }

    void SetupInputActions()
    {
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }

        if (inputActions != null)
        {
            var playerActionMap = inputActions.FindActionMap("Player");
            if (playerActionMap != null)
            {
                saveAction = playerActionMap.FindAction("Save");
            }
        }

        // Crear acción manualmente si no se encuentra
        if (saveAction == null)
        {
            saveAction = new InputAction("Save", InputActionType.Button);
            saveAction.AddBinding("<Keyboard>/f");
            saveAction.AddBinding("<Gamepad>/buttonNorth"); // X/Square
        }

        // Configurar callback
        saveAction.started += OnSave;

        // Habilitar acción
        saveAction?.Enable();
    }

    void OnSave(InputAction.CallbackContext context)
    {
        if (nearCheckpoint)
        {
            SaveGame();
        }
    }

    void SaveGame()
    {
        if (player == null)
        {
            Debug.LogError("❌ No se puede guardar: referencia al jugador es null");
            return;
        }

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null)
        {
            Debug.LogError("❌ No se puede guardar: PlayerHealth no encontrado");
            return;
        }

        SaveData data = new SaveData();
        data.x = player.position.x;
        data.y = player.position.y;
        data.z = player.position.z;
        data.health = ph.GetCurrentHealth();
        data.soul = ph.GetCurrentSoul();
        data.scene = SceneManager.GetActiveScene().name;

        // Guardar monedas
        if (CurrencySystem.Instance != null)
        {
            data.coins = CurrencySystem.Instance.GetCurrentCoins();
        }
        else
        {
            data.coins = 0;
            if (showDebugMessages)
                Debug.LogWarning("⚠️ CurrencySystem no encontrado - guardando 0 monedas");
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(saveFile, json);

            if (showDebugMessages)
                Debug.Log($"✅ Juego guardado | HP: {data.health} | Alma: {data.soul} | Monedas: {data.coins}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al guardar: {e.Message}");
        }
    }

    void SaveCurrencyOnly()
    {
        if (CurrencySystem.Instance == null) return;

        // Solo guardar las monedas en PlayerPrefs para actualizaciones rápidas
        PlayerPrefs.SetInt("PlayerCoins", CurrencySystem.Instance.GetCurrentCoins());
        PlayerPrefs.Save();

        if (showDebugMessages)
            Debug.Log($"💰 Monedas guardadas automáticamente: {CurrencySystem.Instance.GetCurrentCoins()}");
    }

    void LoadGame()
    {
        if (!File.Exists(saveFile))
        {
            if (showDebugMessages)
                Debug.Log("📁 No hay partida guardada");

            // Cargar monedas desde PlayerPrefs como respaldo
            LoadCurrencyFromPrefs();
            return;
        }

        if (player == null)
        {
            Debug.LogError("❌ No se puede cargar: referencia al jugador es null");
            return;
        }

        try
        {
            string json = File.ReadAllText(saveFile);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (SceneManager.GetActiveScene().name == data.scene)
            {
                player.position = new Vector3(data.x, data.y, data.z);

                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.SetCurrentHealth(data.health);
                    ph.SetCurrentSoul(data.soul);
                }

                // Cargar monedas
                if (CurrencySystem.Instance != null)
                {
                    CurrencySystem.Instance.SetCoins(data.coins);
                }
                else
                {
                    // Guardar en PlayerPrefs para cargar más tarde
                    PlayerPrefs.SetInt("PlayerCoins", data.coins);
                    if (showDebugMessages)
                        Debug.Log($"💰 Monedas guardadas en PlayerPrefs para cargar más tarde: {data.coins}");
                }

                if (showDebugMessages)
                    Debug.Log($"📂 Partida cargada | HP: {data.health} | Alma: {data.soul} | Monedas: {data.coins}");
            }
            else
            {
                if (showDebugMessages)
                    Debug.Log($"📁 El guardado es de otra escena: {data.scene}");

                // Aún así cargar las monedas
                if (CurrencySystem.Instance != null)
                {
                    CurrencySystem.Instance.SetCoins(data.coins);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al cargar partida: {e.Message}");

            // Cargar monedas desde PlayerPrefs como respaldo
            LoadCurrencyFromPrefs();
        }
    }

    void LoadCurrencyFromPrefs()
    {
        if (CurrencySystem.Instance != null)
        {
            int savedCoins = PlayerPrefs.GetInt("PlayerCoins", 0);
            CurrencySystem.Instance.SetCoins(savedCoins);

            if (showDebugMessages)
                Debug.Log($"💰 Monedas cargadas desde PlayerPrefs: {savedCoins}");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            nearCheckpoint = true;
            if (showDebugMessages)
                Debug.Log("💾 Cerca de un checkpoint: pulsa F para guardar");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            nearCheckpoint = false;
            if (showDebugMessages)
                Debug.Log("📤 Saliste del checkpoint");
        }
    }

    // Métodos públicos para guardado manual
    public void ForceSave()
    {
        SaveGame();
    }

    public void ForceLoad()
    {
        LoadGame();
    }

    public void ForceSaveCurrency()
    {
        SaveCurrencyOnly();
    }

    public bool HasSaveFile()
    {
        return File.Exists(saveFile);
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
            if (showDebugMessages)
                Debug.Log("🗑️ Partida eliminada");
        }

        // También limpiar PlayerPrefs de monedas
        PlayerPrefs.DeleteKey("PlayerCoins");
        PlayerPrefs.Save();
    }

    // Información del sistema para debugging
    public string GetSaveSystemInfo()
    {
        string fileExists = HasSaveFile() ? "✅" : "❌";
        string nearCheck = nearCheckpoint ? "✅" : "❌";
        string currencySystem = CurrencySystem.Instance != null ? "✅" : "❌";

        return $"SaveSystem | Archivo: {fileExists} | Checkpoint: {nearCheck} | Currency: {currencySystem} | Auto-save coins: {saveCurrencyAutomatically}";
    }

    // Método para obtener información del save actual
    public SaveData GetCurrentSaveData()
    {
        if (!File.Exists(saveFile)) return null;

        try
        {
            string json = File.ReadAllText(saveFile);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error al leer save data: {e.Message}");
            return null;
        }
    }
}
