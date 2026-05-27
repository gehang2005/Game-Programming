using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class TeleportTrigger : MonoBehaviour
{
    [Header("Link")]
    [Tooltip("玩家进入当前触发器后，会被传送到这个目标触发器。")]
    public TeleportTrigger destination;

    [Header("Filtering")]
    [Tooltip("只响应这个 Tag。留空则不筛选。")]
    public string requiredTag = "Player";

    [Header("Teleport Behaviour")]
    [Tooltip("传送后保持玩家当前世界朝向不变，保证从任何角度进入都以相同方向出来。")]
    public bool preserveWorldRotation = true;
    [Tooltip("传送后沿玩家朝向轻推一点距离，减少卡在触发器边缘和瞬移感。")]
    [Min(0f)] public float exitPushDistance = 0.35f;
    [Tooltip("传送冷却，防止刚出去又立刻触发另一端。")]
    [Min(0.01f)] public float teleportCooldown = 0.2f;

    [Header("Anomaly")]
    [Tooltip("每次成功传送后触发一次随机异常显示。")]
    public ClassroomAnomalyRandomizer anomalyRandomizer;

    private static readonly Dictionary<int, float> PlayerCooldownUntil = new Dictionary<int, float>();

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        EnsureTriggerCollider();
    }

    private void OnValidate()
    {
        EnsureTriggerCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destination == null)
            return;

        Transform playerRoot = ResolvePlayerRoot(other);
        if (playerRoot == null)
            return;

        if (!string.IsNullOrEmpty(requiredTag) && !playerRoot.CompareTag(requiredTag))
            return;

        int playerId = playerRoot.GetInstanceID();
        if (PlayerCooldownUntil.TryGetValue(playerId, out float cooldownUntil) && cooldownUntil > Time.time)
            return;

        TeleportPlayer(playerRoot);
        float nextAvailableTime = Time.time + teleportCooldown;
        PlayerCooldownUntil[playerId] = nextAvailableTime;

        if (anomalyRandomizer != null)
            anomalyRandomizer.ActivateRandomAnomaly();
    }

    private void TeleportPlayer(Transform playerRoot)
    {
        CharacterController characterController = playerRoot.GetComponent<CharacterController>();
        if (characterController != null)
            characterController.enabled = false;

        Vector3 localOffset = transform.InverseTransformPoint(playerRoot.position);
        Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * playerRoot.rotation;

        Vector3 targetPosition = destination.transform.TransformPoint(localOffset);
        Quaternion targetRotation = preserveWorldRotation
            ? playerRoot.rotation
            : destination.transform.rotation * relativeRotation;

        Vector3 pushDirection = Vector3.ProjectOnPlane(targetRotation * Vector3.forward, destination.transform.up).normalized;
        if (pushDirection.sqrMagnitude < 0.0001f)
            pushDirection = Vector3.ProjectOnPlane(destination.transform.forward, destination.transform.up).normalized;

        targetPosition += pushDirection * exitPushDistance;

        playerRoot.SetPositionAndRotation(targetPosition, targetRotation);
        Physics.SyncTransforms();

        if (characterController != null)
            characterController.enabled = true;
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
        triggerCollider.isTrigger = true;
    }
}
