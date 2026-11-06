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
    [SerializeField] private Vector3 flashlightMovingRotation = new Vector3(-90f, 0f, 90f);

    private static readonly int WalkHash = Animator.StringToHash("Walk");
    private static readonly int RunHash = Animator.StringToHash("Run");
    private static readonly int HasFlashLightHash = Animator.StringToHash("HasFlashLight");

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isRunning;

    private Quaternion originalFlashlightRotation;
    private bool flashlightRotationApplied;

    private bool HasFlashlight => flashlightItem != null && flashlightItem.IsHeld;
    private bool IsMoving => moveInput.sqrMagnitude > 0.01f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // El cuerpo no rota: solo gira el visualRoot hacia la dirección de movimiento.
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator != null)
            animator.applyRootMotion = false;

        if (flashlightTransform != null)
            originalFlashlightRotation = flashlightTransform.localRotation;
    }

    private void Update()
    {
        UpdateAnimator();
        UpdateFlashlightRotation();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
        RotateCharacter();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    private Vector3 GetMoveDirection()
    {
        if (cameraTarget == null)
            return Vector3.zero;

        Vector3 forward = cameraTarget.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cameraTarget.right;
        right.y = 0f;
        right.Normalize();

        return Vector3.ClampMagnitude(forward * moveInput.y + right * moveInput.x, 1f);
    }

    private void MoveCharacter()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 targetVelocity = GetMoveDirection() * speed;

        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        float rate = IsMoving ? acceleration : deceleration;

        Vector3 newHorizontal = Vector3.MoveTowards(
            currentHorizontal,
            targetVelocity,
            rate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(newHorizontal.x, currentVelocity.y, newHorizontal.z);
    }

    private void RotateCharacter()
    {
        if (visualRoot == null || !IsMoving)
            return;

        Vector3 direction = GetMoveDirection();

        if (direction.sqrMagnitude < 0.001f)
            return;

        visualRoot.rotation = Quaternion.RotateTowards(
            visualRoot.rotation,
            Quaternion.LookRotation(direction.normalized),
            rotationSpeed * Time.fixedDeltaTime);
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetBool(WalkHash, IsMoving && !isRunning);
        animator.SetBool(RunHash, IsMoving && isRunning);
        animator.SetBool(HasFlashLightHash, HasFlashlight);
    }

    private void UpdateFlashlightRotation()
    {
        if (flashlightTransform == null)
            return;

        if (HasFlashlight && IsMoving)
        {
            flashlightTransform.localRotation = Quaternion.Euler(flashlightMovingRotation);
            flashlightRotationApplied = true;
        }
        else if (flashlightRotationApplied)
        {
            flashlightTransform.localRotation = originalFlashlightRotation;
            flashlightRotationApplied = false;
        }
    }
}
