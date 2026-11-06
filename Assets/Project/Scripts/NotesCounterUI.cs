
using TMPro;
using UnityEngine;

public class NotesCounterUI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private int totalNotes = 5;

    private int collectedNotes;
    private bool finalTriggered;

    private void Start()
    {
        UpdateCounter();
    }

    public void RegisterNote()
    {
        if (collectedNotes >= totalNotes)
            return;

        collectedNotes++;

        UpdateCounter();

        if (collectedNotes >= totalNotes && !finalTriggered)
        {
            finalTriggered = true;

            if (FinalGameController.Instance != null)
            {
                FinalGameController.Instance.IniciarFinal();
            }
            else
            {
                Debug.LogError("No se encontró FinalGameController en la escena.");
            }
        }
    }

    private void UpdateCounter()
    {
        if (counterText != null)
        {
            counterText.text = $"{collectedNotes}/{totalNotes}";
        }
    }
}
