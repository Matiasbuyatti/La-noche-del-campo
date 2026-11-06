using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [Header("Componentes del slot")]
    [SerializeField] private Button button;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private GameObject selectionBorder;

    private CollectibleItem currentItem;
    private Action<CollectibleItem> onSelected;

    public void Setup(
        CollectibleItem item,
        Action<CollectibleItem> selectionCallback,
        bool selected)
    {
        currentItem = item;
        onSelected = selectionCallback;

        if (currentItem == null)
            return;

        if (itemIcon != null)
        {
            itemIcon.sprite = currentItem.InventoryIcon;
            itemIcon.enabled = currentItem.InventoryIcon != null;
        }

        if (itemName != null)
            itemName.text = currentItem.ItemName;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(SelectItem);
        }

        SetSelected(selected);
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.SetActive(selected);
    }

    private void SelectItem()
    {
        if (currentItem != null)
            onSelected?.Invoke(currentItem);
    }
}
