
using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private NotesCounterUI notesCounterUI;

    private readonly List<CollectibleItem> items = new List<CollectibleItem>();

    public event Action OnInventoryChanged;

    public IReadOnlyList<CollectibleItem> Items => items;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Hay más de un InventoryManager en la escena.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool AddItem(CollectibleItem item)
    {
        if (item == null)
            return false;

        if (string.IsNullOrWhiteSpace(item.ItemId))
        {
            Debug.LogError($"El objeto {item.ItemName} no tiene un Item ID asignado.", item);
            return false;
        }

        if (HasItem(item.ItemId))
            return false;

        items.Add(item);

        // Contar la nota solamente cuando se agrega correctamente.
        if (item is NoteItem && notesCounterUI != null)
        {
            notesCounterUI.RegisterNote();
        }

        OnInventoryChanged?.Invoke();

        return true;
    }

    public List<CollectibleItem> GetItemsByCategory(InventoryCategory category)
    {
        return items.FindAll(item => item.Category == category);
    }

    public bool HasItem(string itemId)
    {
        return items.Exists(item => item.ItemId == itemId);
    }

    public CollectibleItem GetItem(string itemId)
    {
        return items.Find(item => item.ItemId == itemId);
    }

    public bool RemoveItem(string itemId)
    {
        CollectibleItem item = GetItem(itemId);

        if (item == null)
            return false;

        items.Remove(item);
        OnInventoryChanged?.Invoke();

        return true;
    }
}
