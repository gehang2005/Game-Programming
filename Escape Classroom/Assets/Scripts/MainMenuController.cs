using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the main menu: start game, show/hide how-to-play panel, quit.
/// Attach to MainMenuManager. Buttons are found by name at runtime — no Inspector wiring needed.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Name of the game scene to load when Start is clicked.")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Panels")]
    [SerializeField] private GameObject howToPlayPanel;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        // Find buttons by name and wire up clicks at runtime (reliable across Unity versions)
        BindButton("BtnStart", StartGame);
        BindButton("BtnHow",   ShowHowToPlay);
        BindButton("BtnQuit",  QuitGame);
        BindButton("CloseBtn", HideHowToPlay);
    }

    private void BindButton(string goName, UnityEngine.Events.UnityAction action)
    {
        // FindObjectsInactive.Include ensures buttons inside hidden panels are also found
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            if (btn.gameObject.name == goName)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
                return;
            }
        }
        Debug.LogWarning("[MainMenuController] Button not found: " + goName);
    }

    public void StartGame()    => SceneManager.LoadScene(gameSceneName);
    public void ShowHowToPlay() { if (howToPlayPanel != null) howToPlayPanel.SetActive(true); }
    public void HideHowToPlay() { if (howToPlayPanel != null) howToPlayPanel.SetActive(false); }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
