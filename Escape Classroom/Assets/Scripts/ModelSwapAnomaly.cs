using UnityEngine;

/// <summary>
/// Anomaly effect for cxk.
/// Swaps between two child GameObjects: one normal FBX, one anomaly FBX.
/// 
/// Setup:
///   - Attach to the cxk parent GameObject.
///   - Add two child GameObjects: normal model and anomaly model.
///   - Assign normalModel and anomalyModel in Inspector.
/// </summary>
[DisallowMultipleComponent]
public class ModelSwapAnomaly : MonoBehaviour, IAnomalyEffect
{
    [Header("Models")]
    [Tooltip("Child GameObject with the normal FBX.")]
    public GameObject normalModel;

    [Tooltip("Child GameObject with the anomaly FBX. Assign once the model is ready.")]
    public GameObject anomalyModel;

    private void Awake()
    {
        // Start in normal state
        Deactivate();
    }

    public void Activate()
    {
        if (normalModel != null)  normalModel.SetActive(false);
        if (anomalyModel != null) anomalyModel.SetActive(true);
    }

    public void Deactivate()
    {
        if (normalModel != null)  normalModel.SetActive(true);
        if (anomalyModel != null) anomalyModel.SetActive(false);
    }
}
