
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Pombero : EnemySeguimiento
{
    [Header("Animaciones")]
    [SerializeField] private Animator animator;
    [SerializeField] private string estadoAnimacionStun = "LightHit";

    [Header("Velocidad de patrulla")]
    [SerializeField] private float velocidadPatrulla = 3.5f;

    [Header("Detección visual")]
    [SerializeField] private float radioVision = 10f;
    [SerializeField] private float anguloVision = 120f;
    [SerializeField] private LayerMask obstaculosVision;

    [Header("Linterna")]
    [SerializeField] private Light luzLinterna;
    [SerializeField] private float tiempoReaccionLinterna = 1.2f;

    [Header("Ataque")]
    [SerializeField] private float distanciaAtaque = 1.7f;
    [SerializeField] private float tiempoHastaImpacto = 0.45f;
    [SerializeField] private float tiempoRecuperacion = 1.5f;
    [SerializeField] private float tiempoEntreAtaques = 2f;

    private float temporizadorEntreAtaques;

    private bool impactoAplicado;
    private bool reaccionandoALinterna;
    private bool bloqueoLinterna;

    private Coroutine rutinaAtaque;
    private Coroutine rutinaLinterna;

    // Congelamiento del Transform
    private Vector3 posicionCongelada;
    private Quaternion rotacionCongelada;
    private bool posicionBloqueada;

    private bool rootMotionAnterior;
    private bool updatePositionAnterior;
    private bool updateRotationAnterior;

    protected override void Start()
    {
        base.Start();

        SetAgentSpeed(velocidadPatrulla);

        OnWalk();
    }

    protected override void Update()
    {
        if (agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
            return;

        if (jugador == null)
            return;

        if (reaccionandoALinterna)
            return;

        if (bloqueoLinterna && !IluminadoPorLinterna())
            bloqueoLinterna = false;

        if (!bloqueoLinterna && IluminadoPorLinterna())
        {
            ComenzarReaccionLinterna();
            return;
        }

        if (temporizadorEntreAtaques > 0f)
            temporizadorEntreAtaques -= Time.deltaTime;

        base.Update();
    }

    private void LateUpdate()
    {
        if (!posicionBloqueada)
            return;

        transform.SetPositionAndRotation(
            posicionCongelada,
            rotacionCongelada
        );
    }

    // -----------------------------------------
    // CONGELAMIENTO
    // -----------------------------------------

    private void CongelarPosicion()
    {
        posicionCongelada = transform.position;
        rotacionCongelada = transform.rotation;

        if (agent != null && agent.isActiveAndEnabled)
        {
            updatePositionAnterior = agent.updatePosition;
            updateRotationAnterior = agent.updateRotation;

            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        if (animator != null)
        {
            rootMotionAnterior = animator.applyRootMotion;
            animator.applyRootMotion = false;
        }

        posicionBloqueada = true;
    }

    private void DescongelarPosicion()
    {
        transform.SetPositionAndRotation(
            posicionCongelada,
            rotacionCongelada
        );

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;

            if (agent.isOnNavMesh)
            {
                agent.Warp(posicionCongelada);
                agent.velocity = Vector3.zero;
            }

            agent.updatePosition = updatePositionAnterior;
            agent.updateRotation = updateRotationAnterior;
        }

        if (animator != null)
            animator.applyRootMotion = rootMotionAnterior;

        posicionBloqueada = false;
    }

    // -----------------------------------------
    // DETECCIÓN Y SEGUIMIENTO
    // -----------------------------------------

    protected override bool PuedeDetectarJugador()
    {
        return JugadorVisible();
    }

    protected override bool PuedeContinuarPersiguiendo()
    {
        return false;
    }

    protected override bool DebeAbandonarPersecucion()
    {
        return DistanciaAlJugador() >= distanciaMaximaPersecucion;
    }

    protected override void ActualizarPatrulla()
    {
        SetAgentSpeed(velocidadPatrulla);

        base.ActualizarPatrulla();

        if (EstaPatrullando())
            OnWalk();

        if (EstaPersiguiendo())
            OnRun();
    }

    protected override void ActualizarPersecucion()
    {
        if (DistanciaAlJugador() <= distanciaAtaque &&
            temporizadorEntreAtaques <= 0f)
        {
            ComenzarAtaque();
            return;
        }

        base.ActualizarPersecucion();

        if (EstaPersiguiendo())
            OnRun();

        if (EstaBuscando())
            OnWalk();
    }

    protected override void ComenzarPersecucion()
    {
        base.ComenzarPersecucion();

        OnRun();
    }

    protected override void ComenzarBusqueda()
    {
        base.ComenzarBusqueda();

        OnWalk();
    }

    protected override void ActualizarBusqueda()
    {
        base.ActualizarBusqueda();

        if (EstaBuscando())
            OnWalk();

        if (EstaPersiguiendo())
            OnRun();

        if (EstaPatrullando())
            OnWalk();
    }

    protected override void VolverAPatrulla()
    {
        base.VolverAPatrulla();

        SetAgentSpeed(velocidadPatrulla);

        OnWalk();
    }

    // -----------------------------------------
    // REACCIÓN A LA LINTERNA
    // -----------------------------------------

    private void DetenerCompletamente()
    {
        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    private void ComenzarReaccionLinterna()
    {
        if (reaccionandoALinterna)
            return;

        reaccionandoALinterna = true;
        bloqueoLinterna = true;

        CambiarEstado(EstadoSeguimiento.Retirandose);

        tieneUltimaPosicion = false;

        if (rutinaAtaque != null)
        {
            StopCoroutine(rutinaAtaque);
            rutinaAtaque = null;
        }

        DetenerCompletamente();

        // Congelar posición y rotación antes de reproducir el stun.
        CongelarPosicion();

        if (animator != null)
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
            animator.SetTrigger("LightHit");
        }

        rutinaLinterna = StartCoroutine(RutinaReaccionLinterna());
    }

    private IEnumerator RutinaReaccionLinterna()
    {
        if (animator != null)
        {
            // Esperar hasta que el Animator entre en LightHit.
            while (!animator.GetCurrentAnimatorStateInfo(0)
                       .IsName(estadoAnimacionStun))
            {
                yield return null;
            }

            // Esperar a que termine LightHit y también
            // la transición de salida hacia el siguiente estado.
            while (true)
            {
                AnimatorStateInfo estado =
                    animator.GetCurrentAnimatorStateInfo(0);

                bool sigueEnStun =
                    estado.IsName(estadoAnimacionStun);

                bool estaEnTransicion =
                    animator.IsInTransition(0);

                if (!sigueEnStun && !estaEnTransicion)
                    break;

                yield return null;
            }
        }
        else
        {
            // Respaldo si no hay Animator asignado.
            yield return new WaitForSeconds(tiempoReaccionLinterna);
        }

        DescongelarPosicion();

        CambiarEstado(EstadoSeguimiento.Patrullando);

        SetAgentSpeed(velocidadPatrulla);

        ResumePatrolAtNextPoint();

        OnWalk();

        reaccionandoALinterna = false;
        rutinaLinterna = null;
    }

    // -----------------------------------------
    // ATAQUE
    // -----------------------------------------

    private void ComenzarAtaque()
    {
        if (EstaAtacando())
            return;

        CambiarEstado(EstadoSeguimiento.Atacando);

        DetenerCompletamente();

        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direccion);

        OnHit();

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

        if (DistanciaAlJugador() >= distanciaMaximaPersecucion)
        {
            VolverAPatrulla();
            yield break;
        }

        ComenzarPersecucion();
    }

    private bool EstaAtacando()
    {
        return estadoActual == EstadoSeguimiento.Atacando;
    }

    private void AplicarDanio()
    {
        if (impactoAplicado || jugador == null)
            return;

        impactoAplicado = true;

        if (DistanciaAlJugador() > distanciaAtaque + 0.4f)
            return;

        IDamageable objetivo =
            jugador.GetComponentInParent<IDamageable>();

        if (objetivo == null)
            objetivo = jugador.GetComponentInChildren<IDamageable>();

        if (objetivo != null)
        {
            objetivo.TakeDamage();
            Debug.Log("El Pombero golpeó al jugador.", this);
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
    // VISIÓN DEL JUGADOR
    // -----------------------------------------

    private bool JugadorVisible()
    {
        Vector3 origen = transform.position + Vector3.up * 1.2f;
        Vector3 destino = jugador.position + Vector3.up;

        Vector3 direccion = destino - origen;

        float distancia = direccion.magnitude;

        if (distancia > radioVision)
            return false;

        if (Vector3.Angle(transform.forward, direccion) >
            anguloVision * 0.5f)
            return false;

        if (Physics.Raycast(
            origen,
            direccion.normalized,
            out RaycastHit hit,
            distancia,
            obstaculosVision,
            QueryTriggerInteraction.Ignore))
        {
            return hit.transform == jugador ||
                   hit.transform.IsChildOf(jugador);
        }

        return true;
    }

    // -----------------------------------------
    // DETECCIÓN DE LA LINTERNA
    // -----------------------------------------

    private bool IluminadoPorLinterna()
    {
        if (luzLinterna == null ||
            !luzLinterna.enabled ||
            !luzLinterna.gameObject.activeInHierarchy)
            return false;

        Vector3 origen = luzLinterna.transform.position;
        Vector3 objetivo = transform.position + Vector3.up;

        Vector3 direccion = objetivo - origen;

        float distancia = direccion.magnitude;

        if (distancia > luzLinterna.range)
            return false;

        if (Vector3.Angle(
            luzLinterna.transform.forward,
            direccion) > luzLinterna.spotAngle * 0.5f)
            return false;

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

            if (hitTransform == jugador ||
                hitTransform.IsChildOf(jugador))
                continue;

            return hitTransform == transform ||
                   hitTransform.IsChildOf(transform);
        }

        return false;
    }

    // -----------------------------------------
    // ANIMACIONES
    // -----------------------------------------

    private void OnWalk()
    {
        if (animator == null)
            return;

        animator.SetBool("Walk", true);
        animator.SetBool("Run", false);
    }

    private void OnRun()
    {
        if (animator == null)
            return;

        animator.SetBool("Walk", false);
        animator.SetBool("Run", true);
    }

    private void OnHit()
    {
        if (animator == null)
            return;

        animator.SetBool("Walk", false);
        animator.SetBool("Run", false);
        animator.SetTrigger("Hit");
    }

    // -----------------------------------------
    // GIZMOS
    // -----------------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioVision);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            distanciaMaximaPersecucion
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            distanciaAtaque
        );
    }
}
