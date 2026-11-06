using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Origen de interacción")]
    [SerializeField] private Transform interactionOrigin;

    [Header("Configuración")]
    [SerializeField] private float interactionRadius = 2.5f;
    [SerializeField] private LayerMask interactionLayers = ~0;

    private CollectibleItem currentItem;
    private WellInteraction currentWell;
    private Door currentDoor;

    private Transform Origin => interactionOrigin != null ? interactionOrigin : transform;

    private void Update()
    {
        DetectInteractables();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        DetectInteractables();

        // Los objetos recogibles tienen prioridad; entre pozo y puerta gana el más cercano.
        if (currentItem != null)
        {
            currentItem.Collect();
            currentItem = null;
        }
        else if (currentWell != null)
        {
            currentWell.Interact();
        }
        else if (currentDoor != null)
        {
            currentDoor.Interact();
        }
    }

    private void DetectInteractables()
    {
       
        currentItem = null;
        currentWell = null;
        currentDoor = null;

        Vector3 origin = Origin.position;

        Collider[] hits = Physics.OverlapSphere(
            origin,
            interactionRadius,
            interactionLayers,
            QueryTriggerInteraction.Collide);

        float closestItemDistance = float.MaxValue;
        float closestWellDistance = float.MaxValue;
        float closestDoorDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            CollectibleItem item = hit.GetComponentInParent<CollectibleItem>();

            if (item != null && !item.Collected)
            {
                float distance = Vector3.Distance(origin, item.transform.position);

                if (distance < closestItemDistance)
                {
                    closestItemDistance = distance;
                    currentItem = item;
                }

                continue;
            }

            WellInteraction well = hit.GetComponentInParent<WellInteraction>();

            if (well != null)
            {
                float distance = Vector3.Distance(origin, well.transform.position);

                if (distance < closestWellDistance)
                {
                    closestWellDistance = distance;
                    currentWell = well;
                }
            }

            Door door = hit.GetComponentInParent<Door>();

            if (door != null)
            {
                float distance = Vector3.Distance(origin, door.transform.position);

                if (distance < closestDoorDistance)
                {
                    closestDoorDistance = distance;
                    currentDoor = door;
                }
            }
        }

        if (currentItem != null)
        {
            currentWell = null;
            currentDoor = null;
        }
        else if (currentWell != null && currentDoor != null)
        {
            if (closestWellDistance <= closestDoorDistance)
                currentDoor = null;
            else
                currentWell = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Origin.position, interactionRadius);
    }
}