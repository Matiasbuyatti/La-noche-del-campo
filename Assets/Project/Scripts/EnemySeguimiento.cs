using UnityEngine;

public abstract class EnemySeguimiento : EnemyBase
{
    public enum EstadoSeguimiento
    {
        Patrullando,
        Persiguiendo,
        Buscando,
        Atacando,
        Retirandose
    }

    [Header("Seguimiento")]
    [SerializeField] protected Transform jugador;
    [SerializeField] protected float velocidadPersecucion = 5.5f;
    [SerializeField] protected float distanciaMaximaPersecucion = 18f;

    [Header("Búsqueda")]
    [SerializeField] protected float tiempoBusqueda = 4f;

    protected EstadoSeguimiento estadoActual = EstadoSeguimiento.Patrullando;
    protected Vector3 ultimaPosicionJugador;
    protected bool tieneUltimaPosicion;
    protected float temporizadorBusqueda;

    // Cada enemigo define cómo detecta al jugador y cuándo abandona la persecución.
    protected abstract bool PuedeDetectarJugador();
    protected abstract bool PuedeContinuarPersiguiendo();
    protected abstract bool DebeAbandonarPersecucion();

    protected override void Update()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh || jugador == null)
            return;

        switch (estadoActual)
        {
            case EstadoSeguimiento.Patrullando:
                ActualizarPatrulla();
                break;

            case EstadoSeguimiento.Persiguiendo:
                ActualizarPersecucion();
                break;

            case EstadoSeguimiento.Buscando:
                ActualizarBusqueda();
                break;
        }
    }

    protected virtual void ActualizarPatrulla()
    {
        PatrolUpdate();

        if (PuedeDetectarJugador())
            ComenzarPersecucion();
    }

    protected virtual void ActualizarPersecucion()
    {
        SetAgentSpeed(velocidadPersecucion);

        if (DebeAbandonarPersecucion())
        {
            VolverAPatrulla();
            return;
        }

        if (PuedeDetectarJugador())
        {
            ActualizarUltimaPosicion();
            SetAgentDestination(ultimaPosicionJugador);
            return;
        }

        if (PuedeContinuarPersiguiendo())
            SetAgentDestination(jugador.position);
        else
            ComenzarBusqueda();
    }

    protected virtual void ComenzarPersecucion()
    {
        estadoActual = EstadoSeguimiento.Persiguiendo;

        SetAgentSpeed(velocidadPersecucion);
        ActualizarUltimaPosicion();
        SetAgentDestination(ultimaPosicionJugador);
    }

    protected virtual void ComenzarBusqueda()
    {
        if (!tieneUltimaPosicion)
        {
            VolverAPatrulla();
            return;
        }

        estadoActual = EstadoSeguimiento.Buscando;
        temporizadorBusqueda = tiempoBusqueda;

        SetAgentSpeed(velocidadPersecucion);
        SetAgentDestination(ultimaPosicionJugador);
    }

    protected virtual void ActualizarBusqueda()
    {
        SetAgentSpeed(velocidadPersecucion);

        if (DebeAbandonarPersecucion())
        {
            VolverAPatrulla();
            return;
        }

        if (PuedeDetectarJugador())
        {
            ComenzarPersecucion();
            return;
        }

        temporizadorBusqueda -= Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
            StopAgent();

        if (temporizadorBusqueda <= 0f)
            VolverAPatrulla();
    }

    protected virtual void VolverAPatrulla()
    {
        estadoActual = EstadoSeguimiento.Patrullando;

        tieneUltimaPosicion = false;
        temporizadorBusqueda = 0f;

        SetAgentSpeed(velocidadPersecucion);
        ResumePatrolAtNextPoint();
    }

    protected virtual void ActualizarUltimaPosicion()
    {
        if (jugador == null)
            return;

        ultimaPosicionJugador = jugador.position;
        tieneUltimaPosicion = true;
    }

    protected float DistanciaAlJugador()
    {
        if (jugador == null)
            return Mathf.Infinity;

        return Vector3.Distance(transform.position, jugador.position);
    }

    protected void CambiarEstado(EstadoSeguimiento nuevoEstado)
    {
        estadoActual = nuevoEstado;
    }

    protected bool EstaPersiguiendo() => estadoActual == EstadoSeguimiento.Persiguiendo;
    protected bool EstaBuscando() => estadoActual == EstadoSeguimiento.Buscando;
    protected bool EstaPatrullando() => estadoActual == EstadoSeguimiento.Patrullando;
}
