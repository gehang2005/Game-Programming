using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("实际旋转的门物体。若留空则默认旋转当前物体。")]
    public Transform doorPivot;
    [Tooltip("玩家对象（建议拖入刻晴根对象）。留空时会自动查找 Player Tag。")]
    public Transform player;

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.F;
    [Min(0.1f)] public float interactDistance = 2f;

    [Header("Door Motion")]
    [Tooltip("开门角度。正负可控制开门方向。")]
    public float openAngle = 90f;
    [Min(0.1f)] public float openCloseSpeed = 6f;

    private bool isOpen;
    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private void Awake()
    {
        if (doorPivot == null)
            doorPivot = transform;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }

        closedRotation = doorPivot.localRotation;
        openedRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
    }

    private void Update()
    {
        bool canInteract = player != null &&
                           Vector3.Distance(player.position, doorPivot.position) <= interactDistance;

        if (canInteract && Input.GetKeyDown(interactKey))
            isOpen = !isOpen;

        Quaternion target = isOpen ? openedRotation : closedRotation;
        doorPivot.localRotation = Quaternion.RotateTowards(
            doorPivot.localRotation,
            target,
            openCloseSpeed * 360f * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        Transform pivot = doorPivot != null ? doorPivot : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pivot.position, interactDistance);
    }
}
