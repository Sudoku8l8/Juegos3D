using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Inteligencia Artificial para el enemigo/guardián.
/// Implementa una Máquina de Estados Finitos (FSM) con comportamientos de Patrulla y Persecución.
/// Aplica la técnica de optimización de 'Reducción de cálculos innecesarios' mediante
/// actualización periódica de rutas (Throttling) en lugar de recalcular en cada frame.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Chase,
        Attack
    }

    [Header("Máquina de Estados")]
    [SerializeField] private State currentState = State.Patrol;

    [Header("Patrulla (Waypoints)")]
    [Tooltip("Puntos de patrulla en el escenario")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float waypointTolerance = 1.2f;
    [SerializeField] private float waitAtWaypointTime = 1.5f;

    [Header("Persecución y Detección")]
    [SerializeField] private float chaseSpeed = 5.0f;
    [SerializeField] private float detectionRadius = 8.0f;
    [SerializeField] private float loseRadius = 12.0f;
    [SerializeField] private float fieldOfViewAngle = 140f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Ataque y Daño")]
    [SerializeField] private float attackRadius = 1.6f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Optimización: Frecuencia de Cálculo")]
    [Tooltip("Intervalo en segundos para recalcular la ruta NavMesh (evita saturar la CPU)")]
    [SerializeField] private float pathUpdateInterval = 0.2f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private PlayerHealth playerHealth;
    private int currentWaypointIndex = 0;
    private float waypointWaitTimer = 0f;
    private float nextPathUpdateTime = 0f;
    private float nextAttackTime = 0f;

    public State CurrentState => currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // Localizar al jugador por Tag o componente
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        else
        {
            playerController pc = FindFirstObjectByType<playerController>();
            if (pc != null)
            {
                playerTransform = pc.transform;
                playerHealth = pc.GetComponent<PlayerHealth>();
            }
        }

        agent.speed = patrolSpeed;
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case State.Patrol:
                HandlePatrolState(distanceToPlayer);
                break;

            case State.Chase:
                HandleChaseState(distanceToPlayer);
                break;

            case State.Attack:
                HandleAttackState(distanceToPlayer);
                break;
        }
    }

    #region Comportamiento de Patrulla
    private void HandlePatrolState(float distanceToPlayer)
    {
        // 1. Evaluar transición a Persecución
        if (distanceToPlayer <= detectionRadius && CanSeePlayer())
        {
            SwitchState(State.Chase);
            return;
        }

        // 2. Ejecutar patrulla cíclica entre Waypoints
        if (waypoints == null || waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= waypointTolerance)
        {
            waypointWaitTimer += Time.deltaTime;
            if (waypointWaitTimer >= waitAtWaypointTime)
            {
                waypointWaitTimer = 0f;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }
    }
    #endregion

    #region Comportamiento de Persecución
    private void HandleChaseState(float distanceToPlayer)
    {
        // 1. Evaluar transición a Ataque
        if (distanceToPlayer <= attackRadius)
        {
            SwitchState(State.Attack);
            return;
        }

        // 2. Evaluar pérdida de rastro (volver a Patrulla)
        if (distanceToPlayer > loseRadius)
        {
            SwitchState(State.Patrol);
            return;
        }

        // 3. Optimización: Actualizar destino periódicamente (no en cada frame)
        if (Time.time >= nextPathUpdateTime)
        {
            nextPathUpdateTime = Time.time + pathUpdateInterval;
            agent.SetDestination(playerTransform.position);
        }
    }
    #endregion

    #region Comportamiento de Ataque
    private void HandleAttackState(float distanceToPlayer)
    {
        // Mirar hacia el jugador
        Vector3 targetDirection = (playerTransform.position - transform.position).normalized;
        targetDirection.y = 0;
        if (targetDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * 8f);
        }

        // Si el jugador se aleja, volver a perseguir
        if (distanceToPlayer > attackRadius * 1.3f)
        {
            SwitchState(State.Chase);
            return;
        }

        // Aplicar daño si el cooldown terminó
        if (Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;
            ExecuteAttack();
        }
    }

    private void ExecuteAttack()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log("[EnemyAI] ¡Enemigo atacó al jugador!");
        }
    }
    #endregion

    private void SwitchState(State newState)
    {
        currentState = newState;

        switch (newState)
        {
            case State.Patrol:
                agent.speed = patrolSpeed;
                if (waypoints != null && waypoints.Length > 0)
                {
                    agent.SetDestination(waypoints[currentWaypointIndex].position);
                }
                break;

            case State.Chase:
                agent.speed = chaseSpeed;
                agent.SetDestination(playerTransform.position);
                break;

            case State.Attack:
                agent.ResetPath();
                break;
        }
    }

    /// <summary>
    /// Comprueba campo de visión y que no haya muros intermedios usando Raycast.
    /// </summary>
    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer <= fieldOfViewAngle / 2f)
        {
            Vector3 origin = transform.position + Vector3.up * 1.2f;
            Vector3 target = playerTransform.position + Vector3.up * 1.0f;

            if (!Physics.Linecast(origin, target, obstacleLayer))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // Radio de Detección (Amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Radio de Pérdida (Rojo suave)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRadius);

        // Radio de Ataque (Rojo)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
