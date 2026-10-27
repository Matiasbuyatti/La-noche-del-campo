using UnityEngine;

public class FlashlightItem : MonoBehaviour
{
    public enum FlashlightState
    {
        OnGround,
        Held,
        Stowed
    }

    [Header("Referencias")]
    [SerializeField] private ElectricTorchOnOff torchController;
    [SerializeField] private Collider pickupCollider;
    [SerializeField] private Rigidbody flashlightRigidbody;

    [Header("Estado inicial")]
    [SerializeField] private bool startsOnGround = true;

    [Header("Escala de la linterna")]
    [SerializeField] private Vector3 groundScale = Vector3.one;
    [SerializeField] private Vector3 handScale = Vector3.one;
    [SerializeField] private Vector3 stowScale = Vector3.one;

    [Header("Ajustes de la mano")]
    [SerializeField] private Vector3 handLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 handLocalRotation = Vector3.zero;

    [Header("Ajustes de la cadera")]
    [SerializeField] private Vector3 stowLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 stowLocalRotation = Vector3.zero;

    public FlashlightState State { get; private set; }

    public bool IsOn =>
        torchController != null && torchController.IsLightOn;

    public bool IsHeld => State == FlashlightState.Held;
    public bool IsStowed => State == FlashlightState.Stowed;
    public bool IsOnGround => State == FlashlightState.OnGround;

    private void Awake()
    {
        if (torchController == null)
            torchController =
                GetComponentInChildren<ElectricTorchOnOff>(true);

        if (pickupCollider == null)
            pickupCollider = GetComponentInChildren<Collider>(true);

        if (flashlightRigidbody == null)
            flashlightRigidbody =
                GetComponentInChildren<Rigidbody>(true);

        State = startsOnGround
            ? FlashlightState.OnGround
            : FlashlightState.Stowed;

        if (torchController != null)
            torchController.SetLightState(false);

        if (State == FlashlightState.OnGround)
            transform.localScale = groundScale;

        UpdatePhysics();
    }

    public void PickUp(Transform handAnchor)
    {
        if (handAnchor == null)
        {
            Debug.LogWarning(
                "FlashlightItem: falta asignar Hand Anchor.", this);
            return;
        }

        SetLight(false);

        State = FlashlightState.Held;

        transform.SetParent(handAnchor, false);

        transform.localPosition = handLocalPosition;
        transform.localRotation =
            Quaternion.Euler(handLocalRotation);

        transform.localScale = handScale;

        UpdatePhysics();
    }

    public void Stow(Transform stowAnchor)
    {
        if (stowAnchor == null)
        {
            Debug.LogWarning(
                "FlashlightItem: falta asignar Stow Anchor.", this);
            return;
        }

        SetLight(false);

        State = FlashlightState.Stowed;

        transform.SetParent(stowAnchor, false);

        transform.localPosition = stowLocalPosition;
        transform.localRotation =
            Quaternion.Euler(stowLocalRotation);

        transform.localScale = stowScale;

        UpdatePhysics();
    }

    public void Draw(Transform handAnchor)
    {
        if (!IsStowed)
            return;

        PickUp(handAnchor);
    }

    public void ToggleLight()
    {
        if (!IsHeld || torchController == null)
            return;

        torchController.ToggleLightState();
    }

    public void SetLight(bool isOn)
    {
        if (torchController != null)
            torchController.SetLightState(isOn);
    }

    public void AddBattery(float amount)
    {
        if (torchController != null)
            torchController.AddBattery(amount);
    }

    private void UpdatePhysics()
    {
        bool onGround = State == FlashlightState.OnGround;

        if (pickupCollider != null)
            pickupCollider.enabled = onGround;

        if (flashlightRigidbody != null)
        {
            flashlightRigidbody.isKinematic = !onGround;

            if (!onGround)
            {
                flashlightRigidbody.linearVelocity = Vector3.zero;
                flashlightRigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

    private void OnValidate()
    {
        groundScale.x = Mathf.Max(0.001f, groundScale.x);
        groundScale.y = Mathf.Max(0.001f, groundScale.y);
        groundScale.z = Mathf.Max(0.001f, groundScale.z);

        handScale.x = Mathf.Max(0.001f, handScale.x);
        handScale.y = Mathf.Max(0.001f, handScale.y);
        handScale.z = Mathf.Max(0.001f, handScale.z);

        stowScale.x = Mathf.Max(0.001f, stowScale.x);
        stowScale.y = Mathf.Max(0.001f, stowScale.y);
        stowScale.z = Mathf.Max(0.001f, stowScale.z);
    }
}