
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidades")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6f;

    [Header("Respuesta del movimiento")]
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;
    [SerializeField] private float rotationSpeed = 540f;

    [Header("Referencias")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;

    [Header("Linterna")]
    [SerializeField] private FlashlightItem flashlightItem;
    [SerializeField] private Transform flashlightTransform;

    [Header("Rotación de la linterna al moverse")]
    [SerializeField]
    private Vector3 flashlightMovingRotation =
        new Vector3(-90f, 0f, 90f);

    private static readonly int WalkHash =
        Animator.StringToHash("Walk");

    private static readonly int RunHash =
        Animator.StringToHash("Run");

    private static readonly int HasFlashLightHash =
        Animator.StringToHash("HasFlashLight");

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isRunning;

    private Quaternion originalFlashlightRotation;
    private bool flashlightRotationApplied;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Permite girar sobre Y, pero evita que el cuerpo se vuelque.
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        // Guarda la orientación original de la linterna.
        if (flashlightTransform != null)
        {
            originalFlashlightRotation =
                flashlightTransform.localRotation;
        }
    }

    private void FixedUpdate()
    {
        MoveCharacter();
        RotateCharacter();
    }

    private void Update()
    {
        UpdateAnimator();
        UpdateFlashlightRotation();
    }

    private void MoveCharacter()
    {
        Vector3 moveDirection = Vector3.zero;

        if (cameraTarget != null)
        {
            // Dirección horizontal de la cámara.
            Vector3 forward = cameraTarget.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraTarget.right;
            right.y = 0f;
            right.Normalize();

            moveDirection =
                forward * moveInput.y +
                right * moveInput.x;
        }

        // Evita que la diagonal sea más rápida.
        moveDirection = Vector3.ClampMagnitude(
            moveDirection, 1f);

        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 targetVelocity = moveDirection * speed;

        Vector3 currentVelocity = rb.linearVelocity;

        Vector3 currentHorizontal = new Vector3(
            currentVelocity.x, 0f, currentVelocity.z);

        float rate = moveInput.sqrMagnitude > 0.01f
            ? acceleration
            : deceleration;

        Vector3 newHorizontal = Vector3.MoveTowards(
            currentHorizontal,
            targetVelocity,
            rate * Time.fixedDeltaTime);

        // Conserva la velocidad vertical.
        rb.linearVelocity = new Vector3(
            newHorizontal.x,
            currentVelocity.y,
            newHorizontal.z);
    }

    private void RotateCharacter()
    {
        if (cameraTarget == null || visualRoot == null)
            return;

        if (moveInput.sqrMagnitude < 0.01f)
            return;

        Vector3 direction =
            cameraTarget.forward * moveInput.y +
            cameraTarget.right * moveInput.x;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        visualRoot.rotation = Quaternion.RotateTowards(
            visualRoot.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = Vector2.ClampMagnitude(
            context.ReadValue<Vector2>(), 1f);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        bool moving = moveInput.sqrMagnitude > 0.01f;

        animator.SetBool(WalkHash, moving && !isRunning);
        animator.SetBool(RunHash, moving && isRunning);

        bool hasFlashlight =
            flashlightItem != null && flashlightItem.IsHeld;

        animator.SetBool(HasFlashLightHash, hasFlashlight);
    }

    private void UpdateFlashlightRotation()
    {
        if (flashlightTransform == null)
            return;

        bool hasFlashlight =
            flashlightItem != null && flashlightItem.IsHeld;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;

        bool shouldApplyRotation = hasFlashlight && isMoving;

        if (shouldApplyRotation)
        {
            flashlightTransform.localRotation =
                Quaternion.Euler(flashlightMovingRotation);

            flashlightRotationApplied = true;
        }
        else if (flashlightRotationApplied)
        {
            flashlightTransform.localRotation =
                originalFlashlightRotation;

            flashlightRotationApplied = false;
        }
    }
}
