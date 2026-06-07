using UnityEngine;

/// <summary>
/// Place this trigger collider near the bunk bed at the end of the corridor.
/// When the player walks into it AFTER all classes are cleared, it activates the end screen.
/// </summary>
[RequireComponent(typeof(Collider))]
public class GameEndTrigger : MonoBehaviour
{
    [Tooltip("Reference to the ClassroomGameFlowController to check if all classes are cleared.")]
    public ClassroomGameFlowController gameFlowController;

    [Tooltip("The GameClearScreen UI to activate when the player arrives.")]
    public GameClearScreen gameClearScreen;

    [Tooltip("Tag required to activate this trigger.")]
    public string requiredTag = "Player";

    private bool triggered = false;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        // Only fire if the game has been cleared
        if (gameFlowController != null && !gameFlowController.IsGameCleared) return;

        triggered = true;

        if (gameClearScreen != null)
            gameClearScreen.Show();
    }
}
