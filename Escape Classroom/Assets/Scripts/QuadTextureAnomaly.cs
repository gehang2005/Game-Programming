using UnityEngine;

/// <summary>
/// Anomaly effect for Quad objects (Hachimi, Chihaya).
/// Swaps the texture on the Quad's MeshRenderer between a normal and anomaly texture.
/// Attach to the Quad GameObject. Assign normalTexture and anomalyTexture in Inspector.
/// </summary>
[DisallowMultipleComponent]
public class QuadTextureAnomaly : MonoBehaviour, IAnomalyEffect
{
    [Header("Textures")]
    public Texture normalTexture;
    public Texture anomalyTexture;

    [Tooltip("Name of the shader property to swap. Usually _MainTex.")]
    public string textureProperty = "_MainTex";

    private MeshRenderer meshRenderer;
    private Material runtimeMaterial;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogWarning($"[QuadTextureAnomaly] No MeshRenderer found on {name}.", this);
            return;
        }
        // Use an instance material so we don't modify the shared asset
        runtimeMaterial = meshRenderer.material;
    }

    public void Activate()
    {
        if (runtimeMaterial != null && anomalyTexture != null)
            runtimeMaterial.SetTexture(textureProperty, anomalyTexture);
    }

    public void Deactivate()
    {
        if (runtimeMaterial != null && normalTexture != null)
            runtimeMaterial.SetTexture(textureProperty, normalTexture);
    }
}
