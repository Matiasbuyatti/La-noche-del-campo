using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxVariado : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip[] clips;

    [Header("Variación")]
    [SerializeField] private Vector2 rangoPitch = new Vector2(0.95f, 1.05f);
    [SerializeField] private Vector2 rangoVolumen = new Vector2(0.85f, 1f);

    private AudioSource fuente;
    private int ultimo = -1;

    private void Awake()
    {
        fuente = GetComponent<AudioSource>();
    }

    public void Reproducir()
    {
        if (clips == null || clips.Length == 0)
            return;

        int indice = Random.Range(0, clips.Length);

        // Evita repetir el mismo clip dos veces seguidas.
        if (clips.Length > 1 && indice == ultimo)
            indice = (indice + 1) % clips.Length;

        ultimo = indice;

        fuente.pitch = Random.Range(rangoPitch.x, rangoPitch.y);

        fuente.PlayOneShot(
            clips[indice],
            Random.Range(rangoVolumen.x, rangoVolumen.y));
    }
}