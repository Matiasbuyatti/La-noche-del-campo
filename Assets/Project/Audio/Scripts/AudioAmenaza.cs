using UnityEngine;
using UnityEngine.Audio;

public class AudioAmenaza : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioMixer mezcla;
    [SerializeField] private Transform jugador;
    [SerializeField] private Transform[] enemigos;

    [Header("Parámetro expuesto del mixer")]
    [SerializeField] private string parametro = "CutoffAmbience";

    [Header("Distancias (metros)")]
    [SerializeField] private float distanciaMinima = 4f;
    [SerializeField] private float distanciaMaxima = 20f;

    [Header("Filtro (Hz)")]
    [SerializeField] private float cutoffLibre = 22000f;
    [SerializeField] private float cutoffCercano = 600f;
    [SerializeField] private float suavizado = 3f;

    private float cutoffActual;

    private void Start()
    {
        cutoffActual = cutoffLibre;
    }

    private void Update()
    {
        if (mezcla == null || jugador == null)
            return;

        float masCercano = Mathf.Infinity;

        foreach (Transform enemigo in enemigos)
        {
            if (enemigo == null || !enemigo.gameObject.activeInHierarchy)
                continue;

            float distancia =
                Vector3.Distance(jugador.position, enemigo.position);

            masCercano = Mathf.Min(masCercano, distancia);
        }

        float objetivo = cutoffLibre;

        if (masCercano < distanciaMaxima)
        {
            float cercania = 1f - Mathf.InverseLerp(
                distanciaMinima, distanciaMaxima, masCercano);

            objetivo = Mathf.Exp(Mathf.Lerp(
                Mathf.Log(cutoffLibre),
                Mathf.Log(cutoffCercano),
                cercania));
        }

        cutoffActual = Mathf.Lerp(
            cutoffActual, objetivo, suavizado * Time.deltaTime);

        mezcla.SetFloat(parametro, cutoffActual);
    }
}