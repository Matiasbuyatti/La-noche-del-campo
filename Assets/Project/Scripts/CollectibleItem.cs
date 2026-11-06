using UnityEngine;

public abstract class CollectibleItem : MonoBehaviour
{
    [Header("Información del objeto")]
    [SerializeField] protected string itemId;
    [SerializeField] protected string itemName;

    [TextArea]
    [SerializeField] protected string description;

    [Header("Inventario")]
    [SerializeField] private InventoryCategory inventoryCategory;
    [SerializeField] private Sprite inventoryIcon;
    [SerializeField] private bool canUse;

    [Header("Objetivo relacionado")]
    [SerializeField] protected ObjectiveManager objectiveManager;
    [SerializeField] protected string objectiveId;

    private bool collected;

    public string ItemId => itemId;
    public string ItemName => itemName;
    public string Description => description;
    public bool Collected => collected;
    public InventoryCategory Category => inventoryCategory;
    public Sprite InventoryIcon => inventoryIcon;
    public bool CanUse => canUse;

    public virtual void Collect()
    {
        if (collected)
            return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError($"No existe InventoryManager en la escena. No se pudo recoger: {itemName}");
            return;
        }

        if (!InventoryManager.Instance.AddItem(this))
            return;

        collected = true;

        if (objectiveManager != null && !string.IsNullOrEmpty(objectiveId))
            objectiveManager.CompleteObjective(objectiveId);

        gameObject.SetActive(false);
    }

    public virtual void UseItem()
    {
    }
}
