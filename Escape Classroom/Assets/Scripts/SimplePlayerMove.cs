using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class SimplePlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 10f;
    public float rotationSpeed = 10f;
    [Tooltip("动画步伐对应的设计速度，用于同步动画播放速率与实际移动速度")]
    public float walkAnimSpeed = 5f;
    public AudioClip footstepClip;
    public AudioClip runStepClip;
    [Range(0.5f, 1f)]
    public float runStepPitch = 0.8f;

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
            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            float currentSpeed = isRunning ? runSpeed : moveSpeed;

            Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight   = Vector3.Scale(cameraTransform.right,   new Vector3(1, 0, 1)).normalized;
            Vector3 moveDir    = (camForward * z + camRight * x).normalized;

            Vector3 horizontalMove = moveDir * currentSpeed * Time.deltaTime;
            ApplyGravityAndMove(horizontalMove);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(moveDir),
                rotationSpeed * Time.deltaTime
            );

            // 用实际速度比例驱动动画播放速率，避免步伐与位移不同步
            float speedRatio = currentSpeed / walkAnimSpeed;
            animator.speed = speedRatio;

            // 0 = Idle, 0.5 = Walk, 1 = Run
            animator.SetFloat("Speed", isRunning ? 1f : 0.5f, 0.05f, Time.deltaTime);
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
            bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);
            AudioClip targetClip = (isRunning && runStepClip != null) ? runStepClip : footstepClip;

            if (isMoving)
            {
                if (audioSource.clip != targetClip)
                {
                    audioSource.clip = targetClip;
                    audioSource.Play();
                }
                else if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            else
            {
                audioSource.Stop();
            }

            audioSource.pitch = isRunning ? runStepPitch : 1f;
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