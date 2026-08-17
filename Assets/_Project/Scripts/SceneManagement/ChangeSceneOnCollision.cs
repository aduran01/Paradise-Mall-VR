// SceneLoadTrigger.cs
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class ChangeSceneOnCollision : MonoBehaviour
{
    [Tooltip("Scene that should load AFTER the loading screen finishes.")]
    
    //reference to loadingscreen
    public LoadingScreenController loadRef;
    public string destinationScene;


    [Tooltip("Tag of the player's XR rig collider.")]
    public string playerTag = "Player";

    void Start() {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // …and jump instantly to the tiny loading scene.
        SceneManager.LoadScene("Loading Scene", LoadSceneMode.Single);
    }
}
