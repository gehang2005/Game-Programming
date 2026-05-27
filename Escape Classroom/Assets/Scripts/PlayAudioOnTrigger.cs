using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class PlayAudioOnTrigger : MonoBehaviour
{
    [Header("Filter")]
    [Tooltip("只响应这个 Tag。留空则不筛选。")]
    public string requiredTag = "Player";

    [Header("Audio")]
    [Tooltip("用于播放音频的 AudioSource。留空时会尝试从当前物体或父物体自动获取。")]
    public AudioSource targetAudioSource;
    [Tooltip("可选：覆盖 AudioSource 上的默认音频。")]
    public AudioClip clipOverride;

    [Header("Trigger Rule")]
    [Tooltip("是否只触发一次。")]
    public bool triggerOnce = true;
    [Tooltip("重复触发的最小间隔（秒）。")]
    [Min(0f)] public float cooldown = 0f;

    private bool hasTriggered;
    private float nextAllowedTime;

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        EnsureTriggerCollider();

        if (targetAudioSource == null)
        {
            targetAudioSource = GetComponent<AudioSource>();
            if (targetAudioSource == null)
                targetAudioSource = GetComponentInParent<AudioSource>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
            return;

        if (Time.time < nextAllowedTime)
            return;

        Transform playerRoot = ResolvePlayerRoot(other);
        if (playerRoot == null)
            return;

        if (!string.IsNullOrEmpty(requiredTag) && !playerRoot.CompareTag(requiredTag))
            return;

        if (targetAudioSource == null)
        {
            Debug.LogWarning("PlayAudioOnTrigger: targetAudioSource 未设置。", this);
            return;
        }

        if (clipOverride != null)
            targetAudioSource.clip = clipOverride;

        targetAudioSource.Play();

        hasTriggered = true;
        if (cooldown > 0f)
            nextAllowedTime = Time.time + cooldown;
    }

    [ContextMenu("Reset Trigger State")]
    public void ResetTriggerState()
    {
        hasTriggered = false;
        nextAllowedTime = 0f;
    }

    private Transform ResolvePlayerRoot(Collider other)
    {
        CharacterController characterController = other.GetComponentInParent<CharacterController>();
        if (characterController != null)
            return characterController.transform;

        if (other.attachedRigidbody != null)
            return other.attachedRigidbody.transform;

        return other.transform.root;
    }

    private void EnsureTriggerCollider()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
            triggerCollider.isTrigger = true;
    }
}