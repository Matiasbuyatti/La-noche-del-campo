using UnityEngine;

public class BatteryPowerPickup : MonoBehaviour
{
    [Header("Batería")]
    [Min(0f)]
    [SerializeField] private float batteryAmount = 25f;

    [Header("Configuración")]
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private bool requireFlashlightInHand = true;

    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
            return;

        PlayerFlashlightController player =
            other.GetComponentInParent<PlayerFlashlightController>();

        if (player == null)
            return;

        // Si se requiere que la linterna esté en la mano,
        // el controlador no permitirá recargar una linterna guardada.
        if (requireFlashlightInHand && !player.HasFlashlightHeld())
            return;

        player.AddBatteryToFlashlight(batteryAmount);
        collected = true;

        if (destroyOnPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}