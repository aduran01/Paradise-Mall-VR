using UnityEngine;

public class manChanger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player's VR camera (representing the headset).")]
    public Transform playerCamera;
    
    [Header("Replacement Prefabs")]
    [Tooltip("A list of prefabs to randomly choose from when switching.")]
    public GameObject[] replacementPrefabs;

    [Header("Settings")]
    [Tooltip("How far (in degrees) the player can look away from this object before it switches.")]
    public float viewThreshold = 30f;

    [Tooltip("If true, the replacement will only happen once. If false, it will happen every time the player looks away.")]
    public bool oneTimeSwitch = true;

    private bool hasSwitched = false;

    private void Update()
    {
        if (!playerCamera) 
            return;

        Vector3 directionToObject = (transform.position - playerCamera.position).normalized;
        Vector3 playerForward = playerCamera.forward;

        float angle = Vector3.Angle(playerForward, directionToObject);

        if (angle > viewThreshold)
        {
            if (!hasSwitched || !oneTimeSwitch)
            {
                SwitchToRandomPrefab();
            }
        }
    }

    private void SwitchToRandomPrefab()
    {
        if (replacementPrefabs == null || replacementPrefabs.Length == 0)
        {
            Debug.LogWarning("No replacement prefabs assigned. Cannot switch.");
            return;
        }

        // Pick a random index
        int randomIndex = Random.Range(0, replacementPrefabs.Length);
        GameObject selectedPrefab = replacementPrefabs[randomIndex];

        // Instantiate the randomly chosen prefab
        Instantiate(selectedPrefab, transform.position, transform.rotation);

        if (oneTimeSwitch)
        {
            hasSwitched = true;
        }

        // Destroy the current object since it's replaced
        Destroy(gameObject);
    }
}