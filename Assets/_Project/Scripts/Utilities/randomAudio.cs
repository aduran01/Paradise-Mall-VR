using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class randomAudio : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource.clip == null)
        {
            Debug.LogWarning("AudioSource has no AudioClip assigned.");
            return;
        }

        // Set the playback time to a random point in the clip's duration
        float randomTime = Random.Range(0f, audioSource.clip.length);
        audioSource.time = randomTime;

        audioSource.Play();
        Debug.Log($"Playing '{audioSource.clip.name}' starting at {randomTime:F2} seconds.");
    }
}
