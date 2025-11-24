using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialogueMarker;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Dialogue Lines")]
    [SerializeField] private LocalizedString[] dialogueLines;

    private float typingDelay = 0.05f;
    public InputActionReference interactAction;

    private bool isPlayerInRange;
    private bool dialogueStarted;
    private int currentLineIndex;

    private string currentLine;
    private bool isTyping;

    private void Update()
    {
        if (isPlayerInRange && interactAction.action.WasPerformedThisFrame())
        {
            if (!dialogueStarted)
            {
                StartDialogue();
            }
            else if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = currentLine;
                isTyping = false;
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    private void OnEnable() => interactAction.action.Enable();
    private void OnDisable() => interactAction.action.Disable();

    //Starts the dialogue, shows panel, stops time, and begins typing first line
    private void StartDialogue()
    {
        dialogueStarted = true;
        dialoguePanel.SetActive(true);
        dialogueMarker.SetActive(false);
        currentLineIndex = 0;
        Time.timeScale = 0f;
        StartCoroutine(TypeLine());
    }

    //Shows the next dialogue line or ends dialogue if finished
    private void ShowNextLine()
    {
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            dialogueStarted = false;
            dialoguePanel.SetActive(false);
            dialogueMarker.SetActive(true);
            Time.timeScale = 1f;
        }
    }

    //Coroutine to display the current dialogue line letter by letter
    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = string.Empty;

        var handle = dialogueLines[currentLineIndex].GetLocalizedStringAsync();
        yield return new WaitUntil(() => handle.IsDone);

        currentLine = handle.Result;

        foreach (char ch in currentLine)
        {
            dialogueText.text += ch;
            yield return new WaitForSecondsRealtime(typingDelay);
        }

        isTyping = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            dialogueMarker.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            dialogueMarker.SetActive(false);
        }
    }
}
