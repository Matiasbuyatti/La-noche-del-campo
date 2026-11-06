using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("Panel principal")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Button closeInventoryButton;

    [Header("Botones de categorías")]
    [SerializeField] private Button usablesButton;
    [SerializeField] private Button documentsButton;
    [SerializeField] private Button collectiblesButton;

    [Header("Cuadrícula de objetos")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private ItemSlot itemSlotPrefab;

    [Header("Información del objeto seleccionado")]
    [SerializeField] private TMP_Text selectedItemNameText;
    [SerializeField] private TMP_Text selectedItemDescriptionText;
    [SerializeField] private Image selectedItemIcon;

    [Header("Acciones")]
    [SerializeField] private Button useItemButton;

    private InventoryCategory currentCategory = InventoryCategory.Usables;
    private CollectibleItem selectedItem;
    private InventoryManager inventoryManager;

    private void Awake()
    {
        Bind(closeInventoryButton, CloseInventory);
        Bind(usablesButton, ShowUsables);
        Bind(documentsButton, ShowDocuments);
        Bind(collectiblesButton, ShowCollectibles);
        Bind(useItemButton, UseSelectedItem);
    }

    private void Start()
    {
        inventoryManager = InventoryManager.Instance;

        if (inventoryManager == null)
            return;

        inventoryManager.OnInventoryChanged += RefreshInventory;

        ClearSelectedItem();

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        RefreshInventory();
    }

    private void Update()
    {
        // No se abre durante la pausa.
        if (Keyboard.current == null || Time.timeScale == 0f)
            return;

        if (Keyboard.current.iKey.wasPressedThisFrame)
            ToggleInventory();
    }

    private void OnDestroy()
    {
        GameCursor.SetFree(this, false);

        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged -= RefreshInventory;

        Unbind(closeInventoryButton, CloseInventory);
        Unbind(usablesButton, ShowUsables);
        Unbind(documentsButton, ShowDocuments);
        Unbind(collectiblesButton, ShowCollectibles);
        Unbind(useItemButton, UseSelectedItem);
    }

    private static void Bind(Button button, UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    private static void Unbind(Button button, UnityAction action)
    {
        if (button != null)
            button.onClick.RemoveListener(action);
    }

    public void OpenInventory()
    {
        if (inventoryPanel == null)
            return;

        inventoryPanel.SetActive(true);
        GameCursor.SetFree(this, true);
        RefreshInventory();
    }

    public void CloseInventory()
    {
        if (inventoryPanel == null)
            return;

        inventoryPanel.SetActive(false);
        GameCursor.SetFree(this, false);
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
            return;

        if (inventoryPanel.activeSelf)
            CloseInventory();
        else
            OpenInventory();
    }

    private void ShowUsables() => ChangeCategory(InventoryCategory.Usables);
    private void ShowDocuments() => ChangeCategory(InventoryCategory.Documents);
    private void ShowCollectibles() => ChangeCategory(InventoryCategory.Collectibles);

    private void ChangeCategory(InventoryCategory category)
    {
        currentCategory = category;
        ClearSelectedItem();
        RefreshInventory();
    }

    private void RefreshInventory()
    {
        if (inventoryManager == null || itemsContainer == null || itemSlotPrefab == null)
            return;

        for (int i = itemsContainer.childCount - 1; i >= 0; i--)
            Destroy(itemsContainer.GetChild(i).gameObject);

        List<CollectibleItem> categoryItems =
            inventoryManager.GetItemsByCategory(currentCategory);

        foreach (CollectibleItem item in categoryItems)
        {
            ItemSlot slot = Instantiate(itemSlotPrefab, itemsContainer);
            slot.Setup(item, SelectItem, item == selectedItem);
        }

        if (selectedItem != null && selectedItem.Category != currentCategory)
            ClearSelectedItem();
    }

    private void SelectItem(CollectibleItem item)
    {
        selectedItem = item;

        if (selectedItemNameText != null)
            selectedItemNameText.text = item.ItemName;

        if (selectedItemDescriptionText != null)
            selectedItemDescriptionText.text = item.Description;

        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite = item.InventoryIcon;
            selectedItemIcon.enabled = item.InventoryIcon != null;
        }

        if (useItemButton != null)
            useItemButton.interactable = item.CanUse;

        RefreshInventory();
    }

    private void ClearSelectedItem()
    {
        selectedItem = null;

        if (selectedItemNameText != null)
            selectedItemNameText.text = "Seleccioná un objeto";

        if (selectedItemDescriptionText != null)
            selectedItemDescriptionText.text = "";

        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite = null;
            selectedItemIcon.enabled = false;
        }

        if (useItemButton != null)
            useItemButton.interactable = false;
    }

    private void UseSelectedItem()
    {
        if (selectedItem == null || !selectedItem.CanUse)
            return;

        string usedItemId = selectedItem.ItemId;

        selectedItem.UseItem();

        if (inventoryManager != null && !inventoryManager.HasItem(usedItemId))
        {
            ClearSelectedItem();
            RefreshInventory();
        }
    }
}
