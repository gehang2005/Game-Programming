using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Fade-in end screen shown when the player reaches the bed after completing all 5 classes.
/// Attach to a UI Canvas GameObject. Call Show() to trigger the fade-in sequence.
/// </summary>
public class GameClearScreen : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public TMP_Text messageText;
    public TMP_Text subMessageText;
    public Button playAgainButton;
    public Button quitButton;

    [Header("Settings")]
    [Tooltip("Duration of the black fade-in before the message appears.")]
    public float blackFadeDuration = 1.5f;
    [Tooltip("Duration of the message text fade-in.")]
    public float textFadeDuration = 1.8f;
    [Tooltip("Delay between black fade and message appearing.")]
    public float messageDelay = 0.6f;
    [Tooltip("Name of the scene to reload when Play Again is clicked.")]
    public string gameSceneName = "Standard";

    private CanvasGroup messageGroup;
    private CanvasGroup buttonGroup;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Grab sub-groups if present, otherwise fall back to the root group
        if (messageText != null)
            messageGroup = messageText.transform.parent.GetComponent<CanvasGroup>();
        if (playAgainButton != null)
            buttonGroup = playAgainButton.transform.parent.GetComponent<CanvasGroup>();

        // Start fully hidden
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (messageGroup != null) messageGroup.alpha = 0f;
        if (buttonGroup  != null) buttonGroup.alpha  = 0f;

        gameObject.SetActive(false);
    }

    /// <summary>Trigger the full fade-in sequence.</summary>
    public void Show()
    {
        gameObject.SetActive(true);

        // Unlock cursor so buttons can be clicked
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // Disable player movement scripts so they don't fight for cursor
        var playerMove = FindAnyObjectByType<SimplePlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = false;
            // Stop footstep audio immediately
            var audio = playerMove.GetComponent<AudioSource>();
            if (audio != null) audio.Stop();
        }

        var camFollow = FindAnyObjectByType<CameraFollow>();
        if (camFollow != null) camFollow.enabled = false;

        Time.timeScale = 1f;
        StartCoroutine(FadeInSequence());
    }

    private void Update()
    {
        // Keep cursor unlocked while end screen is visible
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    private IEnumerator FadeInSequence()
    {
        // Phase 1: Fade black overlay in
        yield return StartCoroutine(FadeGroup(canvasGroup, 0f, 1f, blackFadeDuration));

        // Phase 2: Short pause
        yield return new WaitForSeconds(messageDelay);

        // Phase 3: Fade main message in
        if (messageGroup != null)
            yield return StartCoroutine(FadeGroup(messageGroup, 0f, 1f, textFadeDuration));

        // Phase 4: Short pause then fade buttons in
        yield return new WaitForSeconds(0.5f);
        if (buttonGroup != null)
            yield return StartCoroutine(FadeGroup(buttonGroup, 0f, 1f, 1f));

        canvasGroup.interactable   = true;
        canvasGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        group.alpha = to;
    }

    public void OnPlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
