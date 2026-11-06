using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum EstadoAmenaza
{
    Ninguna,
    Persiguiendo,
    Buscando
}

public class AudioJuego : MonoBehaviour
{
    private static AudioJuego instancia;

    [Header("Snapshots de música")]
    [SerializeField] private AudioMixerSnapshot exploracion;
    [SerializeField] private AudioMixerSnapshot persecucion;
    [SerializeField] private AudioMixerSnapshot inmovil;

    [Header("Tiempos de transición (segundos)")]
    [SerializeField] private float haciaPersecucion = 0.3f;
    [SerializeField] private float haciaExploracion = 4f;
    [SerializeField] private float haciaInmovil = 1.5f;

    [Header("Notas")]
    [SerializeField] private AudioSource fuenteVoz;
    [SerializeField] private SfxVariado sfxPapel;

    private readonly Dictionary<Object, EstadoAmenaza> amenazas =
        new Dictionary<Object, EstadoAmenaza>();

    private void Awake()
    {
        instancia = this;
    }

    private void Start()
    {
        if (exploracion != null)
            exploracion.TransitionTo(0f);
    }

    public static void ReportarAmenaza(Object fuente, EstadoAmenaza estado)
    {
        if (instancia == null)
            return;

        instancia.amenazas[fuente] = estado;
        instancia.ActualizarMusica();
    }

    public static void NotaRecogida(AudioClip voz)
    {
        if (instancia == null)
            return;

        if (instancia.sfxPapel != null)
            instancia.sfxPapel.Reproducir();

        if (instancia.fuenteVoz != null && voz != null)
        {
            instancia.fuenteVoz.Stop();
            instancia.fuenteVoz.clip = voz;
            instancia.fuenteVoz.Play();
        }
    }

    private void ActualizarMusica()
    {
        if (exploracion == null || persecucion == null || inmovil == null)
            return;

        bool persigue = false;
        bool busca = false;

        foreach (EstadoAmenaza estado in amenazas.Values)
        {
            persigue |= estado == EstadoAmenaza.Persiguiendo;
            busca |= estado == EstadoAmenaza.Buscando;
        }

        // Si algo te persigue, importa más que quedarte quieto.
        if (persigue)
            persecucion.TransitionTo(haciaPersecucion);
        else if (busca)
            inmovil.TransitionTo(haciaInmovil);
        else
            exploracion.TransitionTo(haciaExploracion);
    }
}
