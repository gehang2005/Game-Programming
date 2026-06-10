using UnityEngine;

/// <summary>
/// Anomaly effect for MeshRenderer objects (frame).
/// Swaps the entire Material between normal and anomaly versions.
/// Attach to the mesh GameObject. Assign normalMaterial and anomalyMaterial in Inspector.
/// </summary>
[DisallowMultipleComponent]
public class MaterialSwapAnomaly : MonoBehaviour, IAnomalyEffect
{
    [Header("Materials")]
    public Material normalMaterial;
    public Material anomalyMaterial;

    [Tooltip("Index of the material slot to swap (0 for the first/only material).")]
    public int materialIndex = 0;

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            Debug.LogWarning($"[MaterialSwapAnomaly] No MeshRenderer found on {name}.", this);
    }

    public void Activate()
    {
        if (meshRenderer == null || anomalyMaterial == null) return;
        var mats = meshRenderer.materials;
        if (materialIndex < mats.Length)
        {
            mats[materialIndex] = anomalyMaterial;
            meshRenderer.materials = mats;
        }
    }

    public void Deactivate()
    {
        if (meshRenderer == null || normalMaterial == null) return;
        var mats = meshRenderer.materials;
        if (materialIndex < mats.Length)
        {
            mats[materialIndex] = normalMaterial;
            meshRenderer.materials = mats;
        }
    }
}
