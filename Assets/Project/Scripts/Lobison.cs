using System.Collections;
using UnityEngine;

public class Lobison : EnemyBase
{
    private enum Estado
    {
        Patrullando,
        Persiguiendo,
        Buscando,
        Atacando
    }

    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Light luzLinterna;
    [SerializeField] private Animator animator;

    [Header("Detección de movimiento")]
    [SerializeField] private bool detectarMovimiento = true;
    [SerializeField] private float radioMovimiento = 12f;
    [SerializeField] private float velocidadMinimaDeteccion = 0.15f;
    [SerializeField] private float tiempoMemoriaMovimiento = 0.3f;

    [Header("Detección de luz")]
    [SerializeField] private bool detectarLuz = true;
    [SerializeField] private float radioLuz = 20f;

    [Header("Velocidades")]
    [SerializeField] private float velocidadPatrulla = 3.5f;
    [SerializeField] private float velocidadPersecucion = 6f;

    [Header("Persecución y búsqueda")]
    [SerializeField] private float distanciaMaximaPersecucion = 25f;
    [SerializeField] private float tiempoBusqueda = 5f;

    [Header("Ataque")]
    [SerializeField] private float distanciaAtaque = 1.7f;
    [SerializeField] private float tiempoHastaImpacto = 0.45f;
    [SerializeField] private float tiempoRecuperacion = 1.5f;
    [SerializeField] private float tiempoEntreAtaques = 2f;

    [Header("Animaciones")]
    [SerializeField] private string parametroCaminar = "Walk";
    [SerializeField] private string parametroCorrer = "Run";
    [SerializeField] private string triggerAtaque = "Hit";

    [Header("Audio")]
    [SerializeField] private SfxVariado sfxGrunido;

    private Estado estadoActual = Estado.Patrullando;

    private Vector3 ultimaPosicionJugador;
    private Vector3 posicionAnteriorJugador;
    private float velocidadActualJugador;

    private float temporizadorBusqueda;
    private float temporizadorMovimiento;
    private float temporizadorEntreAtaques;

    private bool tieneUltimaPosicion;
    private bool impactoAplicado;

    protected override void Start()
    {
        base.Start();

        if (jugador != null)
        {
            posicionAnteriorJugador = jugador.position;
            ultimaPosicionJugador = jugador.position;
        }

        SetAgentSpeed(velocidadPatrulla);
        OnWalk();
    }

    protected override void Update()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh || jugador == null)
            return;

        if (temporizadorEntreAtaques > 0f)
            temporizadorEntreAtaques -= Time.deltaTime;

        // Se recuerda el movimiento unos instantes para evitar cortes de un solo frame.
        if (JugadorSeEstaMoviendo())
            temporizadorMovimiento = tiempoMemoriaMovimiento;
        else if (temporizadorMovimiento > 0f)
            temporizadorMovimiento -= Time.deltaTime;

        bool jugadorDetectado =
            EstaDetectandoMovimiento(temporizadorMovimiento > 0f) ||
            EstaDetectandoLuz();

        switch (estadoActual)
        {
            case Estado.Patrullando:
                ActualizarPatrulla(jugadorDetectado);
                break;

            case Estado.Persiguiendo:
                ActualizarPersecucion(jugadorDetectado);
                break;

            case Estado.Buscando:
                ActualizarBusqueda(jugadorDetectado);
                break;
        }
    }

    private float DistanciaAlJugador()
    {
        return Vector3.Distance(transform.position, jugador.position);
    }

    private bool JugadorSeEstaMoviendo()
    {
        if (Time.deltaTime <= 0f)
            return false;

        Vector3 desplazamiento = jugador.position - posicionAnteriorJugador;
        desplazamiento.y = 0f;

        velocidadActualJugador = desplazamiento.magnitude / Time.deltaTime;
        posicionAnteriorJugador = jugador.position;

        return velocidadActualJugador >= velocidadMinimaDeteccion;
    }

    private bool EstaDetectandoMovimiento(bool jugadorSeMueve)
    {
        return detectarMovimiento && jugadorSeMueve && DistanciaAlJugador() <= radioMovimiento;
    }

    private bool EstaDetectandoLuz()
    {
        if (!detectarLuz || luzLinterna == null || !luzLinterna.enabled ||
            !luzLinterna.gameObject.activeInHierarchy)
            return false;

        Vector3 origen = luzLinterna.transform.position;
        Vector3 direccion = transform.position + Vector3.up - origen;
        float distancia = direccion.magnitude;

        if (distancia > radioLuz)
            return false;

        if (Vector3.Angle(luzLinterna.transform.forward, direccion) > luzLinterna.spotAngle * 0.5f)
            return false;

        RaycastHit[] impactos = Physics.RaycastAll(
            origen,
            direccion.normalized,
            distancia,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);

        System.Array.Sort(impactos, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit impacto in impactos)
        {
            Transform objeto = impacto.transform;

            if (objeto == jugador || objeto.IsChildOf(jugador))
                continue;

            return objeto == transform || objeto.IsChildOf(transform);
        }

        return false;
    }

    private void ActualizarPatrulla(bool jugadorDetectado)
    {
        if (jugadorDetectado)
        {
            if (sfxGrunido != null)
                sfxGrunido.Reproducir();

            ComenzarPersecucion();
            return;
        }

        SetAgentSpeed(velocidadPatrulla);
        PatrolUpdate();
        OnWalk();
    }

    private void ComenzarPersecucion()
    {
        estadoActual = Estado.Persiguiendo;

        ActualizarUltimaPosicion();

        SetAgentSpeed(velocidadPersecucion);
        SetAgentDestination(jugador.position);

        OnRun();
        AudioJuego.ReportarAmenaza(this, EstadoAmenaza.Persiguiendo);
    }

    private void ActualizarPersecucion(bool jugadorDetectado)
    {
        float distancia = DistanciaAlJugador();

        if (distancia >= distanciaMaximaPersecucion)
        {
            VolverAPatrulla();
            return;
        }

        if (distancia <= distanciaAtaque && temporizadorEntreAtaques <= 0f)
        {
            ComenzarAtaque();
            return;
        }

        if (!jugadorDetectado)
        {
            ComenzarBusqueda();
            return;
        }

        ActualizarUltimaPosicion();

        SetAgentSpeed(velocidadPersecucion);
        SetAgentDestination(jugador.position);

        OnRun();
    }

    private void ActualizarUltimaPosicion()
    {
        ultimaPosicionJugador = jugador.position;
        tieneUltimaPosicion = true;
    }

    private void ComenzarBusqueda()
    {
        estadoActual = Estado.Buscando;
        temporizadorBusqueda = tiempoBusqueda;

        if (tieneUltimaPosicion)
            SetAgentDestination(ultimaPosicionJugador);

        SetAgentSpeed(velocidadPersecucion);

        OnWalk();
        AudioJuego.ReportarAmenaza(this, EstadoAmenaza.Buscando);
    }

    private void ActualizarBusqueda(bool jugadorDetectado)
    {
        if (jugadorDetectado)
        {
            ComenzarPersecucion();
            return;
        }

        temporizadorBusqueda -= Time.deltaTime;

        if (tieneUltimaPosicion && HasReachedDestination())
            StopAgent();

        if (temporizadorBusqueda <= 0f)
            VolverAPatrulla();
    }

    private void VolverAPatrulla()
    {
        estadoActual = Estado.Patrullando;
        tieneUltimaPosicion = false;

        SetAgentSpeed(velocidadPatrulla);
        ResumePatrolAtNextPoint();

        OnWalk();
        AudioJuego.ReportarAmenaza(this, EstadoAmenaza.Ninguna);
    }

    private void ComenzarAtaque()
    {
        if (estadoActual == Estado.Atacando)
            return;

        estadoActual = Estado.Atacando;

        StopAgent();

        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direccion);

        OnAttack();

        impactoAplicado = false;
        StartCoroutine(RutinaAtaque());
    }

    private IEnumerator RutinaAtaque()
    {
        yield return new WaitForSeconds(tiempoHastaImpacto);

        AplicarDanio();

        yield return new WaitForSeconds(tiempoRecuperacion);

        temporizadorEntreAtaques = tiempoEntreAtaques;

        if (jugador == null)
            yield break;

        if (DistanciaAlJugador() >= distanciaMaximaPersecucion)
        {
            VolverAPatrulla();
            yield break;
        }

        bool detectado =
            EstaDetectandoMovimiento(velocidadActualJugador >= velocidadMinimaDeteccion) ||
            EstaDetectandoLuz();

        if (detectado)
            ComenzarPersecucion();
        else
            ComenzarBusqueda();
    }

    private void AplicarDanio()
    {
        if (impactoAplicado || jugador == null)
            return;

        impactoAplicado = true;

        if (DistanciaAlJugador() > distanciaAtaque + 0.4f)
            return;

        IDamageable objetivo = jugador.GetComponentInParent<IDamageable>();

        if (objetivo == null)
            objetivo = jugador.GetComponentInChildren<IDamageable>();

        if (objetivo != null)
            objetivo.TakeDamage();
    }

    private void OnWalk()
    {
        if (animator == null)
            return;

        animator.SetBool(parametroCaminar, true);
        animator.SetBool(parametroCorrer, false);
    }

    private void OnRun()
    {
        if (animator == null)
            return;

        animator.SetBool(parametroCaminar, false);
        animator.SetBool(parametroCorrer, true);
    }

    private void OnAttack()
    {
        if (animator == null)
            return;

        animator.SetBool(parametroCaminar, false);
        animator.SetBool(parametroCorrer, false);
        animator.SetTrigger(triggerAtaque);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioMovimiento);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioLuz);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaMaximaPersecucion);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}
