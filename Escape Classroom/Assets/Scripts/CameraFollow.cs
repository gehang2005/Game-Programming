using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 5f;
    public float minDistance = 2f;
    public float maxDistance = 6f;
    public float scrollSpeed = 2f;
    public float height = 1.6f;
    public float mouseRotationSpeed = 3f;
    public float minPitch = -20f;
    public float maxPitch = 60f;

    private float yaw;
    private float pitch = 15f;
    private SimplePlayerMove playerMove;

    void Start()
    {
        yaw = transform.eulerAngles.y;
        if (target != null)
            playerMove = target.GetComponent<SimplePlayerMove>();
        if (playerMove != null)
            playerMove.cameraTransform = transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 鼠标滚轮缩放距离
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance = Mathf.Clamp(distance - scroll * scrollSpeed, minDistance, maxDistance);

        // 鼠标左右旋转（水平），上下旋转（俯仰）
        yaw   += Input.GetAxis("Mouse X") * mouseRotationSpeed;
        pitch -= Input.GetAxis("Mouse Y") * mouseRotationSpeed;
        pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -distance);

        transform.position = target.position + Vector3.up * height + offset;
        transform.rotation = rotation;
    }
}
