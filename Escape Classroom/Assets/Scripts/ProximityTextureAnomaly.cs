using UnityEngine;

/// <summary>
/// Anomaly effect for nailongLaugh.
/// When inactive: shows normal texture, trigger collider disabled.
/// When activated: enables trigger collider. Player walking in swaps to anomaly texture + plays audio.
/// On deactivate: reverts texture, disables trigger.
/// 
/// Setup:
///   - Attach this script to the nailongLaugh parent GameObject (with a Quad child).
///   - Add a child GameObject with a Collider (isTrigger = true) as the proximity trigger.
///   - Assign all fields in Inspector.
/// </summary>
[DisallowMultipleComponent]
public class ProximityTextureAnomaly : MonoBehaviour, IAnomalyEffect
{
    [Header("Quad Reference")]
    [Tooltip("The Quad whose texture will be swapped.")]
    public MeshRenderer quadRenderer;
    public Texture normalTexture;
    public Texture anomalyTexture;
    [Tooltip("Shader property name, usually _MainTex.")]
    public string textureProperty = "_MainTex";

    [Header("Proximity Trigger")]
    [Tooltip("Child collider that acts as the proximity trigger. Disabled when not anomaly round.")]
    public Collider proximityTrigger;
    [Tooltip("Tag required to activate. Should be Player.")]
    public string playerTag = "Player";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip laughClip;

    private Material runtimeMaterial;
    private bool isActive;

    private void Awake()
    {
        if (quadRenderer != null)
            runtimeMaterial = quadRenderer.material; // instance material

        // Always start with trigger disabled
        if (proximityTrigger != null)
            proximityTrigger.enabled = false;
    }

    public void Activate()
    {
        isActive = true;
        // Enable the proximity trigger; texture swap happens when player walks in
        if (proximityTrigger != null)
            proximityTrigger.enabled = true;
    }

    public void Deactivate()
    {
        isActive = false;

        // Disable trigger
        if (proximityTrigger != null)
            proximityTrigger.enabled = false;

        // Revert texture
        if (runtimeMaterial != null && normalTexture != null)
            runtimeMaterial.SetTexture(textureProperty, normalTexture);

        // Stop audio
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        OnProximityEnter(other);
    }

    public void OnProximityEnter(Collider other)
    {
        if (!isActive) return;
        if (!other.CompareTag(playerTag)) return;

        // Swap texture
        if (runtimeMaterial != null && anomalyTexture != null)
            runtimeMaterial.SetTexture(textureProperty, anomalyTexture);

        // Play laugh audio
        if (audioSource != null && laughClip != null)
        {
            audioSource.clip = laughClip;
            audioSource.Play();
        }

        // Disable trigger so it only fires once per round
        if (proximityTrigger != null)
            proximityTrigger.enabled = false;
    }
}
