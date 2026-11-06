using UnityEngine;

public class BatteryItem : CollectibleItem
{
    [Header("Configuración de batería")]
    [SerializeField] private float rechargeAmount = 40f;

    public override void UseItem()
    {
        if (InventoryManager.Instance == null)
            return;

        PlayerFlashlightController flashlight =
            FindFirstObjectByType<PlayerFlashlightController>();

        if (flashlight == null || !flashlight.TryAddBatteryToFlashlight(rechargeAmount))
            return;

        InventoryManager.Instance.RemoveItem(ItemId);
    }
}
