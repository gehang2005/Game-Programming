using System.Collections;
using UnityEngine;
using UnityEngine.Video;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class BinJumpScareController : MonoBehaviour
{
    [Header("Player Filter")]
    [Tooltip("只响应该 Tag。")]
    public string playerTag = "Player";

    [Header("Scare References")]
    [Tooltip("用于显示惊吓视频的 Quad（或任意物体）。")]
    public Transform scareQuad;
    [Tooltip("惊吓视频播放器。留空会自动从 scareQuad 获取。")]
    public VideoPlayer videoPlayer;
    [Tooltip("惊吓音频源。留空会自动从 scareQuad 获取。")]
    public AudioSource audioSource;

    [Header("Anchors")]
    [Tooltip("待机时（隐藏在教室下方）的位置锚点。")]
    public Transform hiddenAnchor;
    [Tooltip("跳出时（垃圾桶上方）的位置锚点。")]
    public Transform showAnchor;
    [Tooltip("结束后下移隐藏的位置锚点。留空则使用 hiddenAnchor。")]
    public Transform endHiddenAnchor;

    [Header("Timing")]
    [Tooltip("触发后延迟多久开始惊吓。")] 
    [Min(0f)] public float triggerDelay = 0.15f;
    [Tooltip("从隐藏点冲到显示点所需时间。")] 
    [Min(0.01f)] public float riseDuration = 0.08f;
    [Tooltip("播放结束后下移隐藏所需时间。")] 
    [Min(0.01f)] public float hideDuration = 0.15f;
    [Tooltip("视频长度无法读取时的兜底显示时长。")] 
    [Min(0.05f)] public float fallbackScareDuration = 1.0f;
    [Tooltip("音频比视频延后播放的时间（秒），用于手动对齐音画同步。0为同时播放。")]
    [Min(0f)] public float audioDelay = 0f;

    [Header("Behaviour")]
    [Tooltip("是否仅触发一次。")]
    public bool triggerOnce = true;
    [Tooltip("开始时隐藏 Quad。")]
    public bool hideQuadOnStart = true;

    private bool hasTriggered;
    private bool isRunning;

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        EnsureTriggerCollider();

        if (scareQuad != null)
        {
            if (videoPlayer == null)
                videoPlayer = scareQuad.GetComponent<VideoPlayer>();
            if (audioSource == null)
                audioSource = scareQuad.GetComponent<AudioSource>();
        }

        Transform startAnchor = hiddenAnchor != null ? hiddenAnchor : endHiddenAnchor;
        if (scareQuad != null && startAnchor != null)
            scareQuad.position = startAnchor.position;

        StopMedia();

        if (scareQuad != null && hideQuadOnStart)
            scareQuad.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isRunning)
            return;
        if (triggerOnce && hasTriggered)
            return;

        Transform playerRoot = ResolvePlayerRoot(other);
        if (playerRoot == null)
            return;

        if (!string.IsNullOrEmpty(playerTag) && !playerRoot.CompareTag(playerTag))
            return;

        if (!ValidateReferences())
            return;

        StartCoroutine(RunJumpScare());
    }

    private IEnumerator RunJumpScare()
    {
        isRunning = true;
        hasTriggered = true;

        if (triggerDelay > 0f)
            yield return new WaitForSeconds(triggerDelay);

        scareQuad.gameObject.SetActive(true);

        if (hiddenAnchor != null)
            scareQuad.position = hiddenAnchor.position;

        if (showAnchor != null)
            yield return MoveOverTime(scareQuad, scareQuad.position, showAnchor.position, riseDuration);

        // 视频和音频分开控制
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
            videoPlayer.Play();
        }

        if (audioSource != null && audioDelay > 0f)
        {
            audioSource.Stop();
            audioSource.time = 0f;
            yield return new WaitForSeconds(audioDelay);
            audioSource.Play();
        }
        else if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
            audioSource.Play();
        }

        double scareDuration = fallbackScareDuration;
        if (videoPlayer != null && videoPlayer.clip != null && videoPlayer.clip.length > 0.01)
            scareDuration = videoPlayer.clip.length;

        yield return new WaitForSeconds((float)scareDuration);

        StopMedia();

        Transform targetHideAnchor = endHiddenAnchor != null ? endHiddenAnchor : hiddenAnchor;
        if (targetHideAnchor != null)
            yield return MoveOverTime(scareQuad, scareQuad.position, targetHideAnchor.position, hideDuration);

        scareQuad.gameObject.SetActive(false);
        isRunning = false;
    }

    private IEnumerator MoveOverTime(Transform target, Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0.01f)
        {
            target.position = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.position = Vector3.LerpUnclamped(from, to, t);
            yield return null;
        }

        target.position = to;
    }

    private void PlayMediaFromStart()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
            videoPlayer.Play();
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
            audioSource.Play();
        }
    }

    private void StopMedia()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.time = 0;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.time = 0f;
        }
    }

    private bool ValidateReferences()
    {
        if (scareQuad == null)
        {
            Debug.LogWarning("BinJumpScareController: scareQuad 未设置。", this);
            return false;
        }
        if (hiddenAnchor == null)
        {
            Debug.LogWarning("BinJumpScareController: hiddenAnchor 未设置。", this);
            return false;
        }
        if (showAnchor == null)
        {
            Debug.LogWarning("BinJumpScareController: showAnchor 未设置。", this);
            return false;
        }

        return true;
    }

    private Transform ResolvePlayerRoot(Collider other)
    {
        CharacterController cc = other.GetComponentInParent<CharacterController>();
        if (cc != null)
            return cc.transform;

        if (other.attachedRigidbody != null)
            return other.attachedRigidbody.transform;

        return other.transform.root;
    }

    private void EnsureTriggerCollider()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
            trigger.isTrigger = true;
    }
}