using UnityEngine;

public class NoteItem : CollectibleItem
{
    [Header("Contenido de la nota")]
    [SerializeField] private string noteTitle;

    [TextArea(5, 15)]
    [SerializeField] private string noteContent;

    public string NoteTitle => noteTitle;
    public string NoteContent => noteContent;

    public override void Collect()
    {
        if (Collected)
            return;

        Debug.Log("Nota encontrada: " + noteTitle);

        base.Collect();
    }
}