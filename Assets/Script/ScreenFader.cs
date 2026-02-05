using System.Collections;
using UnityEngine;

// Attach this script to the FadeCanvas GameObject
// Controls the screen fade effect using the CanvasGroup alpha
public class ScreenFader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup fadeCanvasGroup; // The CanvasGroup on FadeCanvas

    [Header("Fade Settings")]
    [SerializeField] private float fadeToBlackDuration = 0.5f; // How long the fade to black takes
    [SerializeField] private float fadeFromBlackDuration = 0.5f; // How long the fade from black takes
    [SerializeField] private float blackScreenHoldTime = 0.2f; // How long to stay fully black

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;

    // Internal state
    private Coroutine currentFadeCoroutine = null;
    private bool isFading = false;

    void Start()
    {
        // Ensure screen starts fully transparent (not black)
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
        }
        else
        {
            Debug.LogError("ScreenFader: FadeCanvasGroup reference is missing! Assign it in the Inspector.");
        }
    }

    // Called by DeathManager to fade screen to black
    public void FadeToBlack()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("ScreenFader: Cannot fade to black - FadeCanvasGroup is null!");
            return;
        }

        // If already fading, stop the current fade first
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // Start fade to black coroutine
        currentFadeCoroutine = StartCoroutine(FadeToBlackCoroutine());
    }

    // Called by DeathManager to fade screen from black back to transparent
    public void FadeFromBlack()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("ScreenFader: Cannot fade from black - FadeCanvasGroup is null!");
            return;
        }

        // If already fading, stop the current fade first
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // Start fade from black coroutine
        currentFadeCoroutine = StartCoroutine(FadeFromBlackCoroutine());
    }

    // Coroutine that fades the screen to black over time
    private IEnumerator FadeToBlackCoroutine()
    {
        isFading = true;

        if (showDebugLogs == true)
        {
            Debug.Log("ScreenFader: Starting fade to black...");
        }

        float elapsedTime = 0f;
        float startAlpha = fadeCanvasGroup.alpha; // Start from current alpha (might not be 0)

        // Gradually increase alpha from current value to 1 (fully black)
        while (elapsedTime < fadeToBlackDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeToBlackDuration; // 0 to 1
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, normalizedTime);
            yield return null; // Wait for next frame
        }

        // Ensure we end at exactly alpha = 1 (fully black)
        fadeCanvasGroup.alpha = 1f;

        if (showDebugLogs == true)
        {
            Debug.Log("ScreenFader: Fade to black complete. Screen is now fully black.");
        }

        // Hold black screen for a moment before allowing fade back
        yield return new WaitForSeconds(blackScreenHoldTime);

        isFading = false;
        currentFadeCoroutine = null;
    }

    // Coroutine that fades the screen from black back to transparent
    private IEnumerator FadeFromBlackCoroutine()
    {
        isFading = true;

        if (showDebugLogs == true)
        {
            Debug.Log("ScreenFader: Starting fade from black...");
        }

        float elapsedTime = 0f;
        float startAlpha = fadeCanvasGroup.alpha; // Start from current alpha (might not be 1)

        // Gradually decrease alpha from current value to 0 (fully transparent)
        while (elapsedTime < fadeFromBlackDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeFromBlackDuration; // 0 to 1
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, normalizedTime);
            yield return null; // Wait for next frame
        }

        // Ensure we end at exactly alpha = 0 (fully transparent)
        fadeCanvasGroup.alpha = 0f;

        if (showDebugLogs == true)
        {
            Debug.Log("ScreenFader: Fade from black complete. Screen is now fully transparent.");
        }

        isFading = false;
        currentFadeCoroutine = null;
    }

    // Public property to check if a fade is currently happening
    public bool IsFading
    {
        get { return isFading; }
    }

    // Utility method to instantly set screen to black (skip animation)
    public void SetBlackInstant()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;

            if (showDebugLogs == true)
            {
                Debug.Log("ScreenFader: Screen set to black instantly (no fade).");
            }
        }
    }

    // Utility method to instantly set screen to transparent (skip animation)
    public void SetTransparentInstant()
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;

            if (showDebugLogs == true)
            {
                Debug.Log("ScreenFader: Screen set to transparent instantly (no fade).");
            }
        }
    }
}
