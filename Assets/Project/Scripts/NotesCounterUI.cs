using TMPro;
using UnityEngine;

public class NotesCounterUI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private int totalNotes = 5;

    private int collectedNotes;

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
    }

    private void UpdateCounter()
    {
        if (counterText != null)
            counterText.text = $"{collectedNotes}/{totalNotes}";
    }
}
