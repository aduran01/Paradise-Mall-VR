using UnityEngine;

public class sceneSaver : MonoBehaviour
{
    public static sceneSaver Instance;

    public string sceneName; // This is the string that persists

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicate instances
        }

        if (string.IsNullOrEmpty(sceneSaver.Instance.sceneName))
        {
            Debug.Log("no string");
            }
            else
            {
                Debug.Log("Stored string is: " + sceneSaver.Instance.sceneName);
                }
    }
}
