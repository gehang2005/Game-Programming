using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class SimplePlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public AudioClip footstepClip;

    [HideInInspector] public Transform cameraTransform;

    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;
    private float verticalVelocity = 0f;
    private const float gravity = -20f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = footstepClip;
        audioSource.loop = true;
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // 禁用重力影响：检查自身及所有子对象上的 Rigidbody
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(x, 0f, z);
        bool isMoving = inputDir.sqrMagnitude > 0.01f;

        if (isMoving && cameraTransform != null)
        {
            Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight   = Vector3.Scale(cameraTransform.right,   new Vector3(1, 0, 1)).normalized;
            Vector3 moveDir    = (camForward * z + camRight * x).normalized;

            Vector3 horizontalMove = moveDir * moveSpeed * Time.deltaTime;
            ApplyGravityAndMove(horizontalMove);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDir),
                rotationSpeed * Time.deltaTime
            );

            // 0 = Idle, 0.5 = Walk
            animator.speed = 1f;
            animator.SetFloat("Speed", 0.5f, 0.05f, Time.deltaTime);
        }
        else
        {
            animator.speed = 1f;
            animator.SetFloat("Speed", 0f, 0.05f, Time.deltaTime);
            ApplyGravityAndMove(Vector3.zero);
        }

        // 行走时播放脚步声，静止时停止
        if (footstepClip != null)
        {
            if (isMoving)
            {
                if (audioSource.clip != footstepClip)
                {
                    audioSource.clip = footstepClip;
                    audioSource.Play();
                }
                else if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }

            audioSource.pitch = 1f;
        }
    }

    void ApplyGravityAndMove(Vector3 horizontalMove)
    {
        if (controller.isGrounded)
            verticalVelocity = -2f; // 保持贴地
        else
            verticalVelocity += gravity * Time.deltaTime;

        horizontalMove.y = verticalVelocity * Time.deltaTime;
        controller.Move(horizontalMove);
    }
}