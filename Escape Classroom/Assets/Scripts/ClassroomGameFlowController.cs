using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ClassroomGameFlowController : MonoBehaviour
{
    [Header("References")]
    public ClassroomAnomalyRandomizer anomalyManager;
    public TeleportTrigger teleportFront;
    public TeleportTrigger teleportBack;
    public TMP_Text periodText;

    [Header("Progress")]
    [Min(1)] public int targetPeriod = 5;
    [Tooltip("当前进度（第几节课）。")]
    public int currentPeriod = 1;

    private bool gameCleared;

    private void Start()
    {
        currentPeriod = 1;
        gameCleared = false;
        UpdatePeriodText();
        InitializeRound();
    }

    public bool TryHandleExitDecision(TeleportTrigger trigger, Transform playerRoot)
    {
        if (gameCleared || trigger == null || anomalyManager == null)
            return false;

        bool expectsBackExit = anomalyManager.HasAnomalyThisRound;
        bool playerUsedBackExit = trigger.exitDirection == ExitDirection.Back;
        bool isCorrect = expectsBackExit == playerUsedBackExit;

        if (!isCorrect)
        {
            HandleWrongDecision();
            return true;
        }

        currentPeriod += 1;
        UpdatePeriodText();

        if (currentPeriod > targetPeriod)
        {
            HandleGameClear();
            return false;
        }

        return true;
    }

    public void OnTeleportCompleted(TeleportTrigger trigger, Transform playerRoot)
    {
        if (gameCleared)
            return;

        InitializeRound();
    }

    private void InitializeRound()
    {
        if (anomalyManager == null || gameCleared)
            return;

        anomalyManager.HideAllAnomalies();
        anomalyManager.GenerateRoundAnomaly();
        UpdatePeriodText();
    }

    private void HandleWrongDecision()
    {
        currentPeriod = 1;
        UpdatePeriodText();

        if (anomalyManager != null)
        {
            anomalyManager.HideAllAnomalies();
            anomalyManager.GenerateRoundAnomaly();
        }
    }

    private void HandleGameClear()
    {
        gameCleared = true;

        if (anomalyManager != null)
            anomalyManager.HideAllAnomalies();

        SetTeleportEnabled(teleportFront, false);
        SetTeleportEnabled(teleportBack, false);
    }

    private void SetTeleportEnabled(TeleportTrigger trigger, bool enabled)
    {
        if (trigger == null)
            return;

        trigger.enabled = enabled;
        Collider triggerCollider = trigger.GetComponent<Collider>();
        if (triggerCollider != null)
            triggerCollider.enabled = enabled;
    }

    private void UpdatePeriodText()
    {
        if (periodText != null)
            periodText.text = "Class " + currentPeriod;
    }
}
