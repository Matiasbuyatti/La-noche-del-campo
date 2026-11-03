using UnityEngine;

public abstract class CollectibleItem : MonoBehaviour
{
    [Header("Información del objeto")]
    [SerializeField] protected string itemId;
    [SerializeField] protected string itemName;
    [TextArea]
    [SerializeField] protected string description;

    [Header("Objetivo relacionado")]
    [SerializeField] protected ObjectiveManager objectiveManager;
    [SerializeField] protected string objectiveId;

    private bool collected;

    public string ItemId => itemId;
    public string ItemName => itemName;
    public string Description => description;
    public bool Collected => collected;

    public virtual void Collect()
    {
        if (collected)
            return;

        collected = true;

        // Actualiza el objetivo relacionado, si existe.
        if (objectiveManager != null && !string.IsNullOrEmpty(objectiveId))
        {
            objectiveManager.CompleteObjective(objectiveId);
        }

        Debug.Log("Objeto recogido: " + itemName);

        // Desaparece del escenario.
        gameObject.SetActive(false);
    }
}