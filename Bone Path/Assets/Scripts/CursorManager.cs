using UnityEngine;
using System.Collections;

public static class CursorManager
{
    private static MonoBehaviour coroutineRunner;
    private static bool isVisible = false;

    public static void Initialize(MonoBehaviour runner)
    {
        coroutineRunner = runner;
        HideCursor();
    }

    public static void ShowCursor()
    {
        Debug.Log("CursorManager: Showing cursor");
        isVisible = true;
        ApplyCursorState();
    }

    public static void HideCursor()
    {
        Debug.Log("CursorManager: Hiding cursor");
        isVisible = false;
        if (coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(ForceHideCursorCoroutine());
        }
        else
        {
            ApplyCursorState();
        }
    }

    private static void ApplyCursorState()
    {
        if (isVisible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        Debug.Log($"Applied cursor state - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }

    private static IEnumerator ForceHideCursorCoroutine()
    {
        // Force hide across multiple frames to override any interference
        for (int i = 0; i < 5; i++)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Debug.Log($"Force hide attempt {i + 1}");
            yield return null; // Wait one frame
        }

        // Final check and force one more time
        yield return new WaitForEndOfFrame();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log($"Final force hide - Visible: {Cursor.visible}, LockState: {Cursor.lockState}");
    }

    public static bool IsVisible()
    {
        return isVisible;
    }
}
