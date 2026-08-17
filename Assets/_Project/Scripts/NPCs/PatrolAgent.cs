
// PatrolAgent.cs
using UnityEngine;

public sealed class PatrolAgent : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] float lifeSeconds = 4f;
    [Header("Movement")]
    [SerializeField] float moveSpeed = 0.6f;
    [Header("Idle Behaviour")]
    [SerializeField, Range(0f, 1f)] float idleProbability = 0.40f;
    [SerializeField] Vector2 idleRange = new(1, 4);

    Transform[] _destinations;
    Transform _currentTarget;
    bool _isIdling, _ready;
    float _deathAt;
    Animator _anim;
    Camera _cam;
    Transform _player;

    public bool HasExpired => Time.time >= _deathAt;

    void Awake() => _anim = GetComponent<Animator>();

    public void Init(Camera cam, Transform player, Transform[] destinations)
    {
        _cam = cam;
        _player = player;
        _destinations = destinations;
        _ready = SetupPatrol();
    }

    bool SetupPatrol()
    {
        if (_destinations == null || _destinations.Length == 0)
        {
            Debug.LogError("Destinations array is empty");
            return false;
        }

        SetNextDestination();
        _deathAt = Time.time + lifeSeconds;
        return true;
    }

    void Update()
    {
        if (!_ready || HasExpired || _isIdling) return;

        if (Vector3.Distance(transform.position, _currentTarget.position) < 0.1f)
        {
            if (Random.value < idleProbability)
                StartCoroutine(IdleRoutine());
            else
                SetNextDestination();
        }

        MoveTowardsTarget();
        UpdateAnimator();
    }

    void MoveTowardsTarget()
    {
        if (_currentTarget == null) return;
        
        Vector3 direction = (_currentTarget.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        transform.LookAt(_currentTarget.position);
    }

    void UpdateAnimator()
    {
        float speedPct = _isIdling ? 0f : 1f;
        _anim.SetFloat("SpeedPercent", speedPct, 0.1f, Time.deltaTime);
    }

    System.Collections.IEnumerator IdleRoutine()
    {
        _isIdling = true;
        _anim.SetFloat("SpeedPercent", 0);
        
        yield return new WaitForSeconds(Random.Range(idleRange.x, idleRange.y));
        
        _isIdling = false;
        SetNextDestination();
    }

    void SetNextDestination()
    {
        if (_destinations.Length == 0) return;

        Transform newTarget;
        do {
            newTarget = _destinations[Random.Range(0, _destinations.Length)];
        } while (newTarget == _currentTarget);

        _currentTarget = newTarget;
    }
}