using UnityEngine;

public class KeyItem : CollectibleItem
{
    [Header("Información de progreso")]
    [SerializeField] private string progressDescription;

    public string ProgressDescription => progressDescription;
}
