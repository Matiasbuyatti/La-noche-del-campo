
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

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Transform origin = GetInteractionOrigin();

        Collider[] hits = Physics.OverlapSphere(
            origin.position,
            interactionRadius,
            interactionLayers,
            QueryTriggerInteraction.Collide
        );

        FlashlightItem closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            // Si hay un objeto coleccionable más cercano,
            // dejamos que PlayerInteractor se encargue.
            CollectibleItem collectible =
                hit.GetComponentInParent<CollectibleItem>();

            FlashlightItem candidate =
                hit.GetComponentInParent<FlashlightItem>();

            if (candidate == null || !candidate.IsOnGround)
                continue;

            // No considerar la linterna si el mismo collider
            // pertenece también a un objeto coleccionable.
            if (collectible != null)
                continue;

            float distance = Vector3.Distance(
                origin.position,
                candidate.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = candidate;
            }
        }

        // No recoger otra linterna si ya tenemos una.
        if (flashlight != null && !flashlight.IsOnGround)
            return;

        if (closest == null)
        {
            Debug.Log("No hay ninguna linterna recogible cerca.", this);
            return;
        }

        if (handAnchor == null)
        {
            Debug.LogError("Falta asignar Hand Anchor.", this);
            return;
        }

        flashlight = closest;
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

    private Transform GetInteractionOrigin()
    {
        return interactionOrigin != null
            ? interactionOrigin
            : transform;
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
        Transform origin = GetInteractionOrigin();

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin.position, interactionRadius);
    }
}
