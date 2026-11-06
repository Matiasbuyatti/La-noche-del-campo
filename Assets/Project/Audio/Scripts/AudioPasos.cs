using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AudioPasos : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SfxVariado sfxPasos;
    [SerializeField] private SfxVariado sfxPasosCorrer;

    [Header("Velocidades")]
    [SerializeField] private float velocidadMinima = 0.5f;
    [SerializeField] private float velocidadCorrer = 4.5f;

    [Header("Intervalo entre pasos (segundos)")]
    [SerializeField] private float intervaloCaminar = 0.55f;
    [SerializeField] private float intervaloCorrer = 0.35f;

    private Rigidbody rb;
    private float temporizador;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (sfxPasos == null)
            return;

        Vector3 velocidadPlana = rb.linearVelocity;
        velocidadPlana.y = 0f;

        float velocidad = velocidadPlana.magnitude;

        if (velocidad < velocidadMinima)
        {
            temporizador = 0f;
            return;
        }

        temporizador -= Time.deltaTime;

        if (temporizador > 0f)
            return;

        bool corriendo = velocidad >= velocidadCorrer;

        SfxVariado sfx = corriendo && sfxPasosCorrer != null
            ? sfxPasosCorrer
            : sfxPasos;

        sfx.Reproducir();

        temporizador = corriendo
            ? intervaloCorrer
            : intervaloCaminar;
    }
}