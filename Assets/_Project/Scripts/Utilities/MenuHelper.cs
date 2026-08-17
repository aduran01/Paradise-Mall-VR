using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuHelper : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Exact scene name (Build Settings). Leave blank to quit.")]
    public string sceneName;

    // Called when the user presses the Select / Trigger while hovering
    public void OnPointerClick(PointerEventData eventData)
    {
        // Trim to avoid hidden spaces; treat pure whitespace as 'empty'
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            QuitApplication();
        }
        else
        {
            LoadScene(sceneName.Trim());
        }
    }

    /* ------------ Helpers ------------ */

    static void LoadScene(string name)
    {
        // Optional: throw a friendly error if the scene isn’t in Build Settings
        if (!Application.CanStreamedLevelBeLoaded(name))
        {
            Debug.LogError($"Scene “{name}” is not in Build Settings (File ▸ Build Settings).");
            return;
        }

        sceneSaver.Instance.sceneName = name;
        SceneManager.LoadScene("Loading Scene", LoadSceneMode.Single);
    }

    static void QuitApplication()
    {
        Application.Quit();

#if UNITY_EDITOR
        // Stop Play‑mode inside the editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
