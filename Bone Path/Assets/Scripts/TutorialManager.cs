using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Panels")]
    public GameObject movePanel;
    public GameObject jumpPanel;

    [Header("Tutorial Settings")]
    [Tooltip("Tiempo que se muestra cada panel de tutorial")]
    public float panelDisplayTime = 3f;
    [Tooltip("Mostrar logs de debug del tutorial")]
    public bool showDebugLogs = true;

    [Header("Player References")]
    public PlayerMovement playerMovement;

    // Estados del tutorial
    private bool moveTutorialShown = false;
    private bool jumpTutorialShown = false;
    private bool tutorialCompleted = false;

    // Singleton para acceso global
    public static TutorialManager Instance { get; private set; }

    private void Awake()
    {
        // Implementar Singleton
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

    private void Start()
    {
        // Buscar referencias si no están asignadas
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();

        // Buscar paneles si no están asignados
        if (movePanel == null)
            movePanel = GameObject.Find("MovePanel");
        if (jumpPanel == null)
            jumpPanel = GameObject.Find("JumpPanel");

        // Ocultar paneles al inicio
        if (movePanel != null) movePanel.SetActive(false);
        if (jumpPanel != null) jumpPanel.SetActive(false);

        // Iniciar tutorial de movimiento después de un breve delay
        StartCoroutine(StartTutorialSequence());

        if (showDebugLogs)
            Debug.Log("🎓 TutorialManager iniciado");
    }

    private IEnumerator StartTutorialSequence()
    {
        // Esperar un momento para que todo se inicialice
        yield return new WaitForSeconds(1f);

        // Mostrar tutorial de movimiento
        ShowMoveTutorial();
    }

    public void ShowMoveTutorial()
    {
        if (moveTutorialShown || tutorialCompleted) return;

        if (movePanel != null)
        {
            movePanel.SetActive(true);
            moveTutorialShown = true;

            if (showDebugLogs)
                Debug.Log("🎓 Mostrando tutorial de movimiento");

            StartCoroutine(HidePanelAfterTime(movePanel, panelDisplayTime));
        }
    }

    public void ShowJumpTutorial()
    {
        if (jumpTutorialShown || tutorialCompleted) return;

        if (jumpPanel != null)
        {
            jumpPanel.SetActive(true);
            jumpTutorialShown = true;

            if (showDebugLogs)
                Debug.Log("🎓 Mostrando tutorial de salto");

            StartCoroutine(HidePanelAfterTime(jumpPanel, panelDisplayTime));
        }
    }

    private IEnumerator HidePanelAfterTime(GameObject panel, float time)
    {
        yield return new WaitForSeconds(time);

        if (panel != null)
        {
            panel.SetActive(false);
        }

        // Verificar si el tutorial está completo
        CheckTutorialCompletion();
    }

    private void CheckTutorialCompletion()
    {
        if (moveTutorialShown && jumpTutorialShown && !tutorialCompleted)
        {
            CompleteTutorial();
        }
    }

    private void CompleteTutorial()
    {
        tutorialCompleted = true;

        if (showDebugLogs)
            Debug.Log("🎉 Tutorial completado");

        // Opcional: Guardar que el tutorial fue completado
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
    }

    // Métodos públicos para control manual
    public void ForceShowMovePanel()
    {
        if (movePanel != null)
        {
            movePanel.SetActive(true);
        }
    }

    public void ForceShowJumpPanel()
    {
        if (jumpPanel != null)
        {
            jumpPanel.SetActive(true);
        }
    }

    public void ForceHideAllPanels()
    {
        if (movePanel != null) movePanel.SetActive(false);
        if (jumpPanel != null) jumpPanel.SetActive(false);
    }

    public bool IsTutorialCompleted()
    {
        return tutorialCompleted;
    }

    public void ResetTutorial()
    {
        moveTutorialShown = false;
        jumpTutorialShown = false;
        tutorialCompleted = false;

        PlayerPrefs.DeleteKey("TutorialCompleted");

        if (showDebugLogs)
            Debug.Log("🔄 Tutorial reiniciado");
    }
}
