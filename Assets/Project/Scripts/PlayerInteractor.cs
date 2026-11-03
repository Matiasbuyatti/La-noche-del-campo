
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

    private void Update()
    {
        DetectCollectibles();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        DetectCollectibles();

        if (currentItem == null)
            return;

        currentItem.Collect();
        currentItem = null;
    }

    private void DetectCollectibles()
    {
        currentItem = null;

        Transform origin = interactionOrigin != null
            ? interactionOrigin
            : transform;

        Collider[] hits = Physics.OverlapSphere(
            origin.position,
            interactionRadius,
            interactionLayers,
            QueryTriggerInteraction.Collide
        );

        float closestDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            CollectibleItem candidate =
                hit.GetComponentInParent<CollectibleItem>();

            if (candidate == null || candidate.Collected)
                continue;

            float distance = Vector3.Distance(
                origin.position,
                candidate.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentItem = candidate;
            }
        }
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
