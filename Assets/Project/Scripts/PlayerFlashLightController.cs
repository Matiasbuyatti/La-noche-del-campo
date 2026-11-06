using UnityEngine;
using UnityEngine.InputSystem;

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

    private Transform Origin => interactionOrigin != null ? interactionOrigin : transform;

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (flashlight != null && !flashlight.IsOnGround)
            return;

        if (handAnchor == null)
            return;

        FlashlightItem closest = FindClosestFlashlightOnGround();

        if (closest == null)
            return;

        flashlight = closest;
        flashlight.PickUp(handAnchor);
    }

    public void OnToggleFlashlight(InputAction.CallbackContext context)
    {
        if (context.performed && flashlight != null)
            flashlight.ToggleLight();
    }

    public void OnStowFlashlight(InputAction.CallbackContext context)
    {
        if (!context.performed || flashlight == null)
            return;

        if (flashlight.IsHeld && stowAnchor != null)
            flashlight.Stow(stowAnchor);
        else if (flashlight.IsStowed && handAnchor != null)
            flashlight.Draw(handAnchor);
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

    public bool TryAddBatteryToFlashlight(float amount)
    {
        return flashlight != null && flashlight.TryAddBattery(amount);
    }

    private FlashlightItem FindClosestFlashlightOnGround()
    {
        Vector3 origin = Origin.position;

        Collider[] hits = Physics.OverlapSphere(
            origin,
            interactionRadius,
            interactionLayers,
            QueryTriggerInteraction.Collide);

        FlashlightItem closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            FlashlightItem candidate = hit.GetComponentInParent<FlashlightItem>();

            if (candidate == null || !candidate.IsOnGround)
                continue;

            // Si el collider también es un coleccionable, lo maneja PlayerInteractor.
            if (hit.GetComponentInParent<CollectibleItem>() != null)
                continue;

            float distance = Vector3.Distance(origin, candidate.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = candidate;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Origin.position, interactionRadius);
    }
}
