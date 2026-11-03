using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyBase : MonoBehaviour
{
    [Header("Patrulla")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] protected float waitAtPoint = 2f;
    [SerializeField] private bool patrolOnStart = true;

    protected NavMeshAgent agent;

    private int currentPointIndex;
    private float waitTimer;
    private bool isWaiting;
    private bool patrolStarted;

    protected int CurrentPointIndex => currentPointIndex;
    protected bool IsWaiting => isWaiting;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        if (patrolOnStart)
            StartPatrol();
    }

    protected virtual void Update()
    {
        PatrolUpdate();
    }

    // Se llama desde Update en los enemigos que utilizan
    // la patrulla normal de EnemyBase.
    protected bool PatrolUpdate()
    {
        if (!patrolStarted || agent == null ||
            !agent.isActiveAndEnabled || !agent.isOnNavMesh)
            return false;

        if (patrolPoints == null || patrolPoints.Length == 0)
            return false;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                GoToNextPatrolPoint();
            }

            return true;
        }

        if (HasReachedDestination())
        {
            ArrivedAtPoint();
            return true;
        }

        return false;
    }

    public virtual void StartPatrol()
    {
        if (agent == null || patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: faltan puntos de patrulla.", this);
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name}: no está sobre el NavMesh.", this);
            return;
        }

        currentPointIndex = 0;
        isWaiting = false;
        patrolStarted = true;

        GoToCurrentPatrolPoint();
    }

    protected void GoToCurrentPatrolPoint()
    {
        if (!HasValidPatrolPoints())
            return;

        Transform point = patrolPoints[currentPointIndex];

        if (point == null)
        {
            GoToNextPatrolPoint();
            return;
        }

        isWaiting = false;
        agent.isStopped = false;
        agent.SetDestination(point.position);
    }

    // Avanza al siguiente punto y comienza a desplazarse inmediatamente.
    protected void GoToNextPatrolPoint()
    {
        if (!HasValidPatrolPoints())
            return;

        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        GoToCurrentPatrolPoint();
    }

    protected void ArrivedAtPoint()
    {
        isWaiting = true;
        waitTimer = waitAtPoint;
        agent.isStopped = true;
    }

    protected bool HasReachedDestination()
    {
        return !agent.pathPending &&
               agent.hasPath &&
               agent.remainingDistance <= agent.stoppingDistance + 0.15f;
    }

    protected bool HasValidPatrolPoints()
    {
        return patrolPoints != null && patrolPoints.Length > 0;
    }

    protected void SetAgentSpeed(float speed)
    {
        if (agent != null)
            agent.speed = speed;
    }

    protected void SetAgentDestination(Vector3 destination)
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        isWaiting = false;
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    protected void StopAgent()
    {
        if (agent == null)
            return;

        isWaiting = false;
        agent.isStopped = true;
        agent.ResetPath();
    }

    // Permite volver a patrullar desde el siguiente punto
    // después de interrumpir una persecución.
    protected void ResumePatrolAtNextPoint()
    {
        if (!HasValidPatrolPoints())
            return;

        isWaiting = false;
        patrolStarted = true;

        GoToNextPatrolPoint();
    }
}