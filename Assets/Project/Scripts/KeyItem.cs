using UnityEngine;

public class KeyItem : CollectibleItem
{
    [Header("Información de progreso")]
    [SerializeField] private string progressDescription;

    public string ProgressDescription => progressDescription;

    public override void Collect()
    {
        if (Collected)
            return;

        Debug.Log("Objeto de progreso obtenido: " + ItemName);

        base.Collect();
    }
}