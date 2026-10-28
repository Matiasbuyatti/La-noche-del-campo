
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Cinemachine.IInputAxisOwner.AxisDescriptor;

public class PlayerFlashlightController : MonoBehaviour
{
    [Header("Linterna")]
    [SerializeField] private FlashlightItem flashlight;

    [Header("Puntos del personaje")]
    [SerializeField] private Transform handAnchor;
    [SerializeField] private Transform stowAnchor;
    [SerializeField] private Transform interactionOrigin;

    [Header("Interacción")]
    [SerializeField] private float interactionRadius = 2.5f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    // Estos métodos se conectan desde PlayerInput > Events.

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // Si ya tenemos una linterna en la mano o guardada,
        // no intentamos recoger otra.
        if (flashlight != null && !flashlight.IsOnGround)
            return;

        FlashlightItem nearby = FindNearbyFlashlight();

        if (nearby == null)
        {
            Debug.Log("No hay ninguna linterna recogible cerca.", this);
            return;
        }

        if (handAnchor == null)
        {
            Debug.LogError("Falta asignar Hand Anchor.", this);
            return;
        }

        flashlight = nearby;
        flashlight.PickUp(handAnchor);

        Debug.Log("Linterna recogida.", this);
    }

    public void OnToggleFlashlight(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (flashlight == null)
        {
            Debug.Log("Todavía no tenés una linterna.", this);
            return;
        }

        flashlight.ToggleLight();
    }

    public void OnStowFlashlight(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (flashlight == null)
            return;

        if (flashlight.IsHeld)
        {
            if (stowAnchor == null)
            {
                Debug.LogError("Falta asignar Stow Anchor.", this);
                return;
            }

            flashlight.Stow(stowAnchor);
        }
        else if (flashlight.IsStowed)
        {
            if (handAnchor == null)
            {
                Debug.LogError("Falta asignar Hand Anchor.", this);
                return;
            }

            flashlight.Draw(handAnchor);
        }
    }

    private FlashlightItem FindNearbyFlashlight()
    {
        Transform origin = interactionOrigin != null
            ? interactionOrigin
            : transform;
        Collider[] hits = Physics.OverlapSphere(
            origin.position,
            10f,
            ~0,
            QueryTriggerInteraction.Collide
        );

        Debug.Log(
            $"Origen: {origin.position} | Colliders encontrados: {hits.Length}",
            this
        );
        Debug.Log($"Colliders detectados cerca: {hits.Length}", this);

        FlashlightItem closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            FlashlightItem candidate =
                hit.GetComponentInParent<FlashlightItem>();
            Debug.Log($"Collider detectado: {hit.name}", hit);

            if (candidate == null || !candidate.IsOnGround)
                continue;

            float distance = Vector3.Distance(
                origin.position,
                candidate.transform.position
            );

            if (distance < closestDistance)
            {
                closest = candidate;
                closestDistance = distance;
            }
        }

        return closest;
    }

    public bool HasFlashlightHeld()
    {
        return flashlight != null && flashlight.IsHeld;
    }

    public void AddBatteryToFlashlight(float amount)
    {
        if (flashlight != null)
            flashlight.AddBattery(amount);
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = interactionOrigin != null
            ? interactionOrigin
            : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin.position, interactionRadius);
    }
}
