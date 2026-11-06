using UnityEngine;

public class WellInteraction : MonoBehaviour
{
    [Header("Objeto que aparece en el pozo")]
    [SerializeField] private GameObject placedRope;

    [Header("Llave que aparece al colocar la soga")]
    [SerializeField] private GameObject hiddenKey;

    [Header("Inventario")]
    [SerializeField] private string ropeItemId = "cuerda_pozo";

    [Header("Objetivos")]
    [SerializeField] private ObjectiveManager objectiveManager;

    private bool ropePlaced;

    private void Start()
    {
        if (placedRope != null)
            placedRope.SetActive(false);

        if (hiddenKey != null)
            hiddenKey.SetActive(false);
    }

    public void Interact()
    {
        if (ropePlaced || InventoryManager.Instance == null)
            return;

        if (placedRope == null || hiddenKey == null)
            return;

        if (!InventoryManager.Instance.HasItem(ropeItemId))
            return;

        if (!InventoryManager.Instance.RemoveItem(ropeItemId))
            return;

        placedRope.SetActive(true);
        hiddenKey.SetActive(true);
        ropePlaced = true;

        if (objectiveManager != null)
            objectiveManager.CompleteObjective("soga_colocada");
    }
}
