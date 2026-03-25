using UnityEngine;
using UnityEngine.InputSystem; // Added to use the new Input System

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonMovement : MonoBehaviour
{
    public float speed = 6f;
    public float rotationSpeed = 10f;

    [Header("Jump & Gravity")]
    public float gravity = -9.81f;
    public float jumpHeight = 2f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Input Actions")]
    [Tooltip("Reference to the Move action (Vector2)")]
    public InputActionReference moveAction;
    [Tooltip("Reference to the Jump action (Button)")]
    public InputActionReference jumpAction;

    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    // It is best practice to enable and disable your actions when the script is toggled
    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset downward velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // small downward force to keep grounded
        }

        // --- NEW INPUT SYSTEM: Movement ---
        // We read the Vector2 value directly from the action instead of using Input.GetAxis
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        float horizontal = input.x;
        float vertical = input.y;

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = camForward * vertical + camRight * horizontal;

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        // --- NEW INPUT SYSTEM: Jump ---
        // WasPressedThisFrame() replaces Input.GetButtonDown()
        if (jumpAction.action.WasPressedThisFrame() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply stronger downward acceleration for tighter, less floaty jumps.
        bool jumpHeld = jumpAction != null && jumpAction.action.IsPressed();
        float gravityScale = 1f;

        if (velocity.y < 0f)
        {
            gravityScale = fallMultiplier;
        }
        else if (velocity.y > 0f && !jumpHeld)
        {
            gravityScale = lowJumpMultiplier;
        }

        velocity.y += gravity * gravityScale * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}