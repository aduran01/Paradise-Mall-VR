using UnityEngine;

public class Mover : MonoBehaviour
{
    public float moveSpeed = .5f;
    private Animator animator;
    private AudioSource audioSource;

     private bool isStopped = false;


    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
         AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Moving"))
        {
            // Move forward and stop audio if it's playing
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            
            if (isStopped)
            {
                isStopped = false;
                if (audioSource.isPlaying)
                    audioSource.Stop();
            }
        }
        else if (!stateInfo.IsName("Moving"))
        {
            if (!isStopped)
            {
                isStopped = true;
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided");
        if (collision.collider.name == "Turning Point" || collision.collider.name == "Turning Point (1)")
        {
            TurnAround();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided");
        if (other.name == "Turning Point" || other.name == "Turning Point (1)")
        {
            TurnAround();
        }
    }

    void TurnAround()
    {
        transform.Rotate(0f, 180f, 0f);
    }
}
