
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    private Estado estadoActual = Estado.Patrullando;

    private Vector3 ultimaPosicionJugador;
    private Vector3 posicionAnteriorJugador;

    private float temporizadorBusqueda;
    private float temporizadorEntreAtaques;

    private bool tieneUltimaPosicion;
    private bool impactoAplicado;

    private Coroutine rutinaAtaque;

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
        if (agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh ||
            jugador == null)
            return;

        if (temporizadorEntreAtaques > 0f)
            temporizadorEntreAtaques -= Time.deltaTime;

        // Medimos el movimiento real del jugador.
        bool jugadorSeMueve = JugadorSeEstaMoviendo();

        // Comprobamos si existe alguna señal detectable.
        bool jugadorDetectado =
            EstaDetectandoMovimiento(jugadorSeMueve) ||
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

            case Estado.Atacando:
                // La coroutine controla el ataque.
                break;
        }
    }

    // -----------------------------------------
    // DETECCIÓN DE MOVIMIENTO
    // -----------------------------------------

    private bool JugadorSeEstaMoviendo()
    {
        if (jugador == null || Time.deltaTime <= 0f)
            return false;

        Vector3 desplazamiento =
            jugador.position - posicionAnteriorJugador;

        desplazamiento.y = 0f;

        float velocidad =
            desplazamiento.magnitude / Time.deltaTime;

        posicionAnteriorJugador = jugador.position;

        return velocidad >= velocidadMinimaDeteccion;
    }

    private bool EstaDetectandoMovimiento(bool jugadorSeMueve)
    {
        if (!detectarMovimiento || !jugadorSeMueve)
            return false;

        float distancia =
            Vector3.Distance(transform.position, jugador.position);

        return distancia <= radioMovimiento;
    }

    // -----------------------------------------
    // DETECCIÓN DE LUZ
    // -----------------------------------------

    private bool EstaDetectandoLuz()
    {
        if (!detectarLuz ||
            luzLinterna == null ||
            !luzLinterna.enabled ||
            !luzLinterna.gameObject.activeInHierarchy)
            return false;

        Vector3 origen = luzLinterna.transform.position;
        Vector3 objetivo = transform.position + Vector3.up;

        Vector3 direccion = objetivo - origen;

        float distancia = direccion.magnitude;

        if (distancia > radioLuz)
            return false;

        // Comprueba si el Lobisón está dentro del cono de luz.
        if (Vector3.Angle(
            luzLinterna.transform.forward,
            direccion) > luzLinterna.spotAngle * 0.5f)
            return false;

        // Comprueba que no haya obstáculos entre la luz y el enemigo.
        RaycastHit[] impactos = Physics.RaycastAll(
            origen,
            direccion.normalized,
            distancia,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        System.Array.Sort(
            impactos,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit impacto in impactos)
        {
            Transform hitTransform = impacto.transform;

            // Ignoramos los colliders del jugador.
            if (hitTransform == jugador ||
                hitTransform.IsChildOf(jugador))
                continue;

            // El primer objeto que recibe la luz debe ser el Lobisón.
            return hitTransform == transform ||
                   hitTransform.IsChildOf(transform);
        }

        return false;
    }

    // -----------------------------------------
    // PATRULLA
    // -----------------------------------------

    private void ActualizarPatrulla(bool jugadorDetectado)
    {
        if (jugadorDetectado)
        {
            ComenzarPersecucion();
            return;
        }

        SetAgentSpeed(velocidadPatrulla);

        PatrolUpdate();

        OnWalk();
    }

    // -----------------------------------------
    // PERSECUCIÓN
    // -----------------------------------------

    private void ComenzarPersecucion()
    {
        estadoActual = Estado.Persiguiendo;

        ActualizarUltimaPosicion();

        SetAgentSpeed(velocidadPersecucion);
        SetAgentDestination(jugador.position);

        OnRun();
    }

    private void ActualizarPersecucion(bool jugadorDetectado)
    {
        float distancia =
            Vector3.Distance(transform.position, jugador.position);

        if (distancia >= distanciaMaximaPersecucion)
        {
            VolverAPatrulla();
            return;
        }

        if (distancia <= distanciaAtaque &&
            temporizadorEntreAtaques <= 0f)
        {
            ComenzarAtaque();
            return;
        }

        if (jugadorDetectado)
        {
            ActualizarUltimaPosicion();

            SetAgentSpeed(velocidadPersecucion);
            SetAgentDestination(jugador.position);

            OnRun();
        }
        else
        {
            ComenzarBusqueda();
        }
    }

    private void ActualizarUltimaPosicion()
    {
        ultimaPosicionJugador = jugador.position;
        tieneUltimaPosicion = true;
    }

    // -----------------------------------------
    // BÚSQUEDA
    // -----------------------------------------

    private void ComenzarBusqueda()
    {
        estadoActual = Estado.Buscando;
        temporizadorBusqueda = tiempoBusqueda;

        if (tieneUltimaPosicion)
            SetAgentDestination(ultimaPosicionJugador);

        SetAgentSpeed(velocidadPersecucion);

        OnWalk();
    }

    private void ActualizarBusqueda(bool jugadorDetectado)
    {
        if (jugadorDetectado)
        {
            ComenzarPersecucion();
            return;
        }

        temporizadorBusqueda -= Time.deltaTime;

        if (tieneUltimaPosicion &&
            HasReachedDestination())
        {
            StopAgent();
        }

        if (temporizadorBusqueda <= 0f)
        {
            VolverAPatrulla();
        }
    }

    // -----------------------------------------
    // VOLVER A PATRULLAR
    // -----------------------------------------

    private void VolverAPatrulla()
    {
        estadoActual = Estado.Patrullando;

        tieneUltimaPosicion = false;

        SetAgentSpeed(velocidadPatrulla);

        ResumePatrolAtNextPoint();

        OnWalk();
    }

    // -----------------------------------------
    // ATAQUE
    // -----------------------------------------

    private void ComenzarAtaque()
    {
        if (estadoActual == Estado.Atacando)
            return;

        estadoActual = Estado.Atacando;

        StopAgent();

        // Girar hacia el jugador.
        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direccion);

        OnAttack();

        impactoAplicado = false;

        rutinaAtaque = StartCoroutine(RutinaAtaque());
    }

    private IEnumerator RutinaAtaque()
    {
        yield return new WaitForSeconds(tiempoHastaImpacto);

        AplicarDanio();

        yield return new WaitForSeconds(tiempoRecuperacion);

        temporizadorEntreAtaques = tiempoEntreAtaques;

        rutinaAtaque = null;

        if (jugador == null)
            yield break;

        float distancia =
            Vector3.Distance(transform.position, jugador.position);

        if (distancia >= distanciaMaximaPersecucion)
        {
            VolverAPatrulla();
            yield break;
        }

        // Si todavía detecta al jugador, continúa persiguiendo.
        // Si no, busca su última posición.
        bool detectado =
            EstaDetectandoMovimiento(
                velocidadActualJugador >= velocidadMinimaDeteccion
            ) || EstaDetectandoLuz();

        if (detectado)
            ComenzarPersecucion();
        else
            ComenzarBusqueda();
    }

    private float velocidadActualJugador;

    private void AplicarDanio()
    {
        if (impactoAplicado || jugador == null)
            return;

        impactoAplicado = true;

        if (Vector3.Distance(transform.position, jugador.position) >
            distanciaAtaque + 0.4f)
            return;

        IDamageable objetivo =
            jugador.GetComponentInParent<IDamageable>();

        if (objetivo == null)
            objetivo = jugador.GetComponentInChildren<IDamageable>();

        if (objetivo != null)
        {
            objetivo.TakeDamage();

            Debug.Log("El Lobisón golpeó al jugador.", this);
        }
        else
        {
            Debug.LogWarning(
                "El jugador no tiene un componente IDamageable.",
                this
            );
        }
    }

    // -----------------------------------------
    // ANIMACIONES
    // -----------------------------------------

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

    // -----------------------------------------
    // GIZMOS
    // -----------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioMovimiento);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioLuz);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            distanciaMaximaPersecucion
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            transform.position,
            distanciaAtaque
        );
    }
}
