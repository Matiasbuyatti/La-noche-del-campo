
using UnityEngine;

public class NoteItem : CollectibleItem
{
    [Header("Contenido de la nota")]
    [SerializeField] private string noteTitle;

    [TextArea(5, 15)]
    [SerializeField] private string noteContent;

    [Header("Audio")]
    [SerializeField] private AudioClip vozNota;

    public string NoteTitle => noteTitle;
    public string NoteContent => noteContent;

    public override void Collect()
    {
        if (Collected)
            return;

        base.Collect();

        if (Collected)
        {
            AudioJuego.NotaRecogida(vozNota);
        }
    }
}
