
using UnityEngine;
using UnityEngine.AI;

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

    protected EstadoSeguimiento estadoActual =
        EstadoSeguimiento.Patrullando;

    protected Vector3 ultimaPosicionJugador;

    protected bool tieneUltimaPosicion;

    protected float temporizadorBusqueda;

    // Cada enemigo define cómo detecta al jugador.
    protected abstract bool PuedeDetectarJugador();

    // Cada enemigo define si puede mantener la persecución.
    protected abstract bool PuedeContinuarPersiguiendo();

    // Cada enemigo define cuándo debe abandonar la persecución.
    protected abstract bool DebeAbandonarPersecucion();

    protected override void Update()
    {
        if (agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
            return;

        if (jugador == null)
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

            case EstadoSeguimiento.Atacando:
                break;

            case EstadoSeguimiento.Retirandose:
                break;
        }
    }

    // --------------------------------------------------
    // PATRULLA
    // --------------------------------------------------

    protected virtual void ActualizarPatrulla()
    {
        PatrolUpdate();

        if (PuedeDetectarJugador())
        {
            ComenzarPersecucion();
        }
    }

    // --------------------------------------------------
    // PERSECUCIÓN
    // --------------------------------------------------

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
            ultimaPosicionJugador = jugador.position;
            tieneUltimaPosicion = true;

            SetAgentDestination(ultimaPosicionJugador);
            return;
        }

        if (!PuedeContinuarPersiguiendo())
        {
            ComenzarBusqueda();
        }
        else
        {
            SetAgentDestination(jugador.position);
        }
    }

    protected virtual void ComenzarPersecucion()
    {
        estadoActual = EstadoSeguimiento.Persiguiendo;

        SetAgentSpeed(velocidadPersecucion);

        ultimaPosicionJugador = jugador.position;
        tieneUltimaPosicion = true;

        SetAgentDestination(ultimaPosicionJugador);
    }

    // --------------------------------------------------
    // BÚSQUEDA
    // --------------------------------------------------

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

        // Si vuelve a detectar al jugador, retoma la persecución.
        if (PuedeDetectarJugador())
        {
            ComenzarPersecucion();
            return;
        }

        temporizadorBusqueda -= Time.deltaTime;

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            StopAgent();
        }

        if (temporizadorBusqueda <= 0f)
        {
            VolverAPatrulla();
        }
    }

    // --------------------------------------------------
    // FINALIZAR PERSECUCIÓN
    // --------------------------------------------------

    protected virtual void VolverAPatrulla()
    {
        estadoActual = EstadoSeguimiento.Patrullando;

        tieneUltimaPosicion = false;
        temporizadorBusqueda = 0f;

        SetAgentSpeed(velocidadPersecucion);

        ResumePatrolAtNextPoint();
    }

    // --------------------------------------------------
    // UTILIDADES
    // --------------------------------------------------

    protected float DistanciaAlJugador()
    {
        if (jugador == null)
            return Mathf.Infinity;

        return Vector3.Distance(
            transform.position,
            jugador.position
        );
    }

    protected virtual void ActualizarUltimaPosicion()
    {
        if (jugador == null)
            return;

        ultimaPosicionJugador = jugador.position;
        tieneUltimaPosicion = true;
    }

    protected void CambiarEstado(EstadoSeguimiento nuevoEstado)
    {
        estadoActual = nuevoEstado;
    }

    protected bool EstaPersiguiendo()
    {
        return estadoActual == EstadoSeguimiento.Persiguiendo;
    }

    protected bool EstaBuscando()
    {
        return estadoActual == EstadoSeguimiento.Buscando;
    }

    protected bool EstaPatrullando()
    {
        return estadoActual == EstadoSeguimiento.Patrullando;
    }
}
