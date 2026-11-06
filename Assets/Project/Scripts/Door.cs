using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Llave")]
    [SerializeField] private string requiredKeyId = "llave_galpon";

    [Header("Puerta")]
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 openOffset = new Vector3(2f, 0f, 0f);
    [SerializeField] private float openDuration = 2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;

    [Header("Objetivos")]
    [SerializeField] private ObjectiveManager objectiveManager;

    private Vector3 closedPosition;
    private bool isOpening;
    private bool isOpen;

    private void Awake()
    {
        if (doorTransform != null)
            closedPosition = doorTransform.localPosition;
    }

    public void Interact()
    {
        if (isOpening || isOpen || doorTransform == null || InventoryManager.Instance == null)
            return;

        if (!InventoryManager.Instance.HasItem(requiredKeyId))
            return;

        // La puerta consume la llave al abrirse.
        if (!InventoryManager.Instance.RemoveItem(requiredKeyId))
            return;

        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        isOpening = true;

        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

        Vector3 targetPosition = closedPosition + openOffset;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / openDuration));
            doorTransform.localPosition = Vector3.Lerp(closedPosition, targetPosition, t);

            yield return null;
        }

        doorTransform.localPosition = targetPosition;

        isOpening = false;
        isOpen = true;

        if (objectiveManager != null)
            objectiveManager.CompleteObjective("galpon_abierto");
    }
}
