using UnityEngine;
using UnityEngine.AI;

public class MoveOnPlane : MonoBehaviour
{
    [Header("Player & Vision Settings")]
    public Camera playerCamera;               // Reference to the player's camera
    public Transform playerTransform;         // Reference to the player's transform
    public float minDistanceFromPlayer = 15f; // Minimum distance to maintain from the player
    public float fieldOfViewAngle = 110f;     // Field of view angle for the player (degrees)

    [Header("Movement Speeds")]
    public float normalSpeed = 0.5f;   // Normal speed for patrolling (base)
    public float fleeingSpeed = 12f;   // Speed when fleeing from the player (base)

    [Header("Timing")]
    public float wanderTimer = 8f;     // Time between switching waypoints in normal mode

    [Header("Destinations (Paired)")]
    public Transform[] possibleDestinations;  // Even length array. First half: L points, second half: R points.

    [Header("Toe-Based Speed Adjustment")]
    public Transform leftToe;          // Reference to the left toe transform
    public Transform rightToe;         // Reference to the right toe transform
    public float minToeDistance = 0.1f; // Minimum toe distance (small stride)
    public float maxToeDistance = 0.5f; // Maximum toe distance (large stride)
    public float maxToeSpeedMultiplier = 1.5f; // Speed multiplier at maximum toe distance
    

    [Header("Footstep Sound")]
    public AudioClip footstepClip;     // Single footstep sound clip
    public float toeRaycastDistance = 0.2f; // How far down to raycast from each toe to detect ground

    private NavMeshAgent agent;
    private Animator animator;

    private float timer;
    private bool isFleeing = false;

    private int numberOfPairs;
    private int currentPairIndex;
    private bool goingToLeftPoint = true;
    private float currentBaseSpeed; // The current base speed (either normalSpeed or fleeingSpeed) before toe adjustment

    private AudioSource audioSource;

    // For footstep detection
    private bool previousLeftToeGrounded = false;
    private bool previousRightToeGrounded = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        numberOfPairs = possibleDestinations.Length / 2;
        currentPairIndex = Random.Range(0, numberOfPairs);
        goingToLeftPoint = true;

        currentBaseSpeed = normalSpeed; // Start with normal speed
        timer = wanderTimer;

        animator.SetBool("IsRunning", false);

        // Set the initial destination
        SetDestinationToCurrentPair();

        // Start continuous checks
        InvokeRepeating("CheckPlayerVisibility", 0f, 0.2f); // Adjust frequency as needed

        // Set up the audio source for footsteps
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // Make it 3D
        audioSource.loop = false;
        audioSource.volume = 0.5f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Handle route switching after reaching destination or time elapsed
        if (timer >= wanderTimer || agent.remainingDistance < 0.5f)
        {
            if (!isFleeing)
            {
                goingToLeftPoint = !goingToLeftPoint;
                SetDestinationToCurrentPair();
                timer = 0f;
            }
        }

        // Adjust agent speed based on toe distance
        AdjustSpeedBasedOnToeDistance();

        // Update Animator's SpeedPercent parameter based on agent's velocity
        float speedPercent = agent.velocity.magnitude / fleeingSpeed;
        animator.SetFloat("SpeedPercent", speedPercent, 0.1f, Time.deltaTime);

        // Check foot collisions with ground and play sounds if needed
        HandleFootsteps();
    }

    void CheckPlayerVisibility()
    {
        bool inSight = IsInPlayerLineOfSight(transform.position);
        bool tooClose = IsPlayerTooClose(transform.position);

        // If the agent is in the player's view or too close, switch pairs and flee
        if (inSight || tooClose)
        {
            if (!isFleeing)
            {
                isFleeing = true;
                currentBaseSpeed = fleeingSpeed;
                animator.SetBool("IsRunning", true);
                ChooseNewPair();
                timer = 0f;
            }
        }
        else
        {
            // If the agent is safe and was fleeing, switch back to normal speed
            if (isFleeing)
            {
                isFleeing = false;
                currentBaseSpeed = normalSpeed;
                animator.SetBool("IsRunning", false);
            }
        }
    }

    void ChooseNewPair()
    {
        int originalPair = currentPairIndex;
        int attempts = 0;
        bool found = false;

        while (attempts < numberOfPairs && !found)
        {
            int newPairIndex = Random.Range(0, numberOfPairs);
            if (newPairIndex != originalPair)
            {
                if (IsPairSuitable(newPairIndex))
                {
                    currentPairIndex = newPairIndex;
                    goingToLeftPoint = true;
                    SetDestinationToCurrentPair();
                    found = true;
                }
            }
            attempts++;
        }

        // If not found, remain on the same pair (fallback).
    }

    bool IsPairSuitable(int pairIndex)
    {
        Transform leftPoint = possibleDestinations[pairIndex];
        Transform rightPoint = possibleDestinations[pairIndex + numberOfPairs];

        bool leftOK = !IsInPlayerLineOfSight(leftPoint.position) && !IsPlayerTooClose(leftPoint.position);
        bool rightOK = !IsInPlayerLineOfSight(rightPoint.position) && !IsPlayerTooClose(rightPoint.position);

        return leftOK && rightOK;
    }

    void SetDestinationToCurrentPair()
    {
        Transform leftPoint = possibleDestinations[currentPairIndex];
        Transform rightPoint = possibleDestinations[currentPairIndex + numberOfPairs];

        Transform destination = goingToLeftPoint ? leftPoint : rightPoint;
        agent.SetDestination(destination.position);
    }

    void AdjustSpeedBasedOnToeDistance()
    {
        if (leftToe == null || rightToe == null)
        {
            // If toe references aren't set, just keep base speed
            agent.speed = currentBaseSpeed;
            return;
        }

        // Calculate toe distance
        float toeDistance = Vector3.Distance(leftToe.position, rightToe.position);

        // Normalize toe distance between 0 and 1 (0 at minToeDistance, 1 at maxToeDistance)
        float normalized = Mathf.InverseLerp(minToeDistance, maxToeDistance, toeDistance);

        // When toes are at minToeDistance, speed = 0
        // When toes are at maxToeDistance, speed = maxToeSpeedMultiplier * currentBaseSpeed
        float toeSpeedMultiplier = Mathf.Lerp(0f, maxToeSpeedMultiplier, normalized);

        // Set the agent's speed
        agent.speed = currentBaseSpeed * toeSpeedMultiplier;
    }

    bool IsInPlayerLineOfSight(Vector3 position)
    {
        Vector3 directionToAgent = position - playerTransform.position;
        float angle = Vector3.Angle(playerTransform.forward, directionToAgent);

        if (angle < fieldOfViewAngle * 0.5f)
        {
            RaycastHit hit;
            Vector3 rayStart = playerTransform.position + Vector3.up * 1.6f;
            if (Physics.Raycast(rayStart, directionToAgent.normalized, out hit))
            {
                if (hit.transform == this.transform)
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool IsPlayerTooClose(Vector3 position)
    {
        float distance = Vector3.Distance(position, playerTransform.position);
        return distance < minDistanceFromPlayer;
    }

    void HandleFootsteps()
    {
        if (footstepClip == null)
        {
            return; // No clip assigned
        }

        bool leftToeGrounded = IsToeOnGround(leftToe);
        bool rightToeGrounded = IsToeOnGround(rightToe);

        // Check left toe landing
        if (leftToeGrounded && !previousLeftToeGrounded)
        {
            PlayFootstepSoundWithRandomPitch();
        }

        // Check right toe landing
        if (rightToeGrounded && !previousRightToeGrounded)
        {
            PlayFootstepSoundWithRandomPitch();
        }

        // Update previous states
        previousLeftToeGrounded = leftToeGrounded;
        previousRightToeGrounded = rightToeGrounded;
    }

    bool IsToeOnGround(Transform toe)
    {
        if (toe == null)
            return false;

        RaycastHit hit;
        Vector3 start = toe.position;
        // Cast a short ray downwards to detect the ground
        if (Physics.Raycast(start, Vector3.down, out hit, toeRaycastDistance))
        {
            // You can add a layer check or tag check here if needed
            return true;
        }

        return false;
    }

    void PlayFootstepSoundWithRandomPitch()
    {
        // Randomize pitch slightly
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(footstepClip);
        
    }
}
