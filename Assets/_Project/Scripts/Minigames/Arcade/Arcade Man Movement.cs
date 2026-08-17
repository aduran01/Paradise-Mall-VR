using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class ArcadeManMovement : MonoBehaviour
{
    [Header("Wandering")]
    [Tooltip("Radius (in metres) inside which new points are chosen, relative to the agent’s start position.")]
    public float wanderRadius = 25f;

    [Tooltip("Minimum straight‑line distance to a new point before it is accepted.")]
    public float minPointDistance = 3f;

    [Header("Idling")]
    [Range(0f, 1f)]
    [Tooltip("Probability that the agent will idle instead of immediately picking a new point " +
             "once it reaches its destination.")]
    public float idleChance = 0.5f;

    [Tooltip("Idle time range in seconds (x = min, y = max).")]
    public Vector2 idleTimeRange = new Vector2(1.5f, 4f);

    [Header("Animation")]
    [Tooltip("Animator parameter that switches between walk and idle states (bool).")]
    public string walkBool = "IsMoving";

    
    

    private NavMeshAgent agent;
    private Animator animator;

    private Vector3 homePosition;
    private float idleTimer = 0f;      // > 0 ⇒ currently idling
    private readonly int maxSampleTries = 20;

    // ──────────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        homePosition = transform.position;
    }

    void Start()
    {
        PickNextDestination();
    }

    void Update()
    {
        // If we’re idling, count down and resume walking when finished
        if (idleTimer > 0f)
        {
            idleTimer -= Time.deltaTime;

            if (idleTimer <= 0f)
            {
                agent.isStopped = false;   
                PickNextDestination();
            }
        }
        else
        {
            // Walking: check if we reached the current destination
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                MaybeIdleOrContinue();
            }
        }

        // Keep the Animator in sync
       bool walking = idleTimer <= 0f;
        animator.SetBool(walkBool, walking);

    
    }

    // ──────────────────────────────────────────────────────────────────────────────
    private void PickNextDestination()
    {
        // Simple rejection‑sampling inside a sphere until a NavMesh point is found
        for (int i = 0; i < maxSampleTries; i++)
        {
            Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
            randomDir.y = 0f;                              // keep it on the X‑Z plane
            Vector3 targetPos = homePosition + randomDir;

            if (Vector3.Distance(transform.position, targetPos) < minPointDistance)
                continue; // too close to the current spot

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPos, out hit, 2f, agent.areaMask))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        // Fallback: if all attempts failed, try again next frame
    }

    private void MaybeIdleOrContinue()
    {
        if (Random.value < idleChance)
        {
            // Enter idle
            idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y);
            agent.isStopped = true;
           // animator.SetBool(walkBool, false);

        }
        else
        {
            // Immediately walk somewhere else
            agent.isStopped = false;
           // animator.SetBool(walkBool, true);
            PickNextDestination();
        }
    }
}
