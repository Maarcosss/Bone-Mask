using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialogueMarker;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;

    private float typingDelay = 0.05f;

    public InputActionReference interactAction;
    private bool isPlayerInRange;
    private bool dialogueStarted;
    private int currentLineIndex;

    private void Update()
    {
        if (isPlayerInRange && interactAction.action.WasPerformedThisFrame())
        {
            if (!dialogueStarted)
            {
                StartDialogue();
            }
            else if (dialogueText.text == dialogueLines[currentLineIndex])
            {
                ShowNextLine();
            }
            else
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[currentLineIndex];
            }
        }
    }

    private void OnEnable()
    {
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.Disable();
    }

    //Start a dialogue
    private void StartDialogue()
    {
        dialogueStarted = true;
        dialoguePanel.SetActive(true);
        dialogueMarker.SetActive(false);
        currentLineIndex = 0;
        Time.timeScale = 0f;
        StartCoroutine(TypeLine());
    }

    //Advance to the next line of dialogue
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

    //Display a dialog box letter by letter
    private IEnumerator TypeLine()
    {
        dialogueText.text = string.Empty;
        foreach (char ch in dialogueLines[currentLineIndex])
        {
            dialogueText.text += ch;
            yield return new WaitForSecondsRealtime(typingDelay);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isPlayerInRange = true;
        dialogueMarker.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isPlayerInRange = false;
            dialogueMarker.SetActive(false);
        }
    }
}
