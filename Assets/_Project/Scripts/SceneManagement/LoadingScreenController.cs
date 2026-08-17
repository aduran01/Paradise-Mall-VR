using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressBar;           // Assign in inspector

    [Header("Behaviour")]
    [Min(0f)] public float minDisplayTime = 0.5f;  // Avoid a blink if loading is <‑‑ this fast
    public bool autoActivate = true;               // Turn off if you want a “Press A to continue”

    private void Start() => StartCoroutine(LoadRoutine());

    private IEnumerator LoadRoutine()
    {
        // ------------------------------------------------------------------
        // 0. Safety: make sure somebody set SceneTransition.NextScene
        // ------------------------------------------------------------------
        string target = sceneSaver.Instance.sceneName;
        if (string.IsNullOrEmpty(target))
        {
            Debug.LogError("LoadingScreen: SceneTransition.NextScene was never set!");
            yield break;
        }

        float startTime = Time.unscaledTime;

        // ------------------------------------------------------------------
        // 1. Begin async loading of the target scene
        // ------------------------------------------------------------------
        AsyncOperation op = SceneManager.LoadSceneAsync(target);
        op.allowSceneActivation = false;    // We’ll flip this when we’re ready
        
        //flip it back for next use :DDD
        sceneSaver.Instance.sceneName = "General Mall";

        // ------------------------------------------------------------------
        // 2. Update UI while Unity loads (progress goes 0‑0.9)
        // ------------------------------------------------------------------
        while (op.progress < 0.9f)
        {
            UpdateUI(op.progress);
            yield return null;
        }

        // ------------------------------------------------------------------
        // 3. Loading finished; wait out minDisplayTime (optional)
        // ------------------------------------------------------------------
        while (Time.unscaledTime - startTime < minDisplayTime)
        {
            UpdateUI(0.9f);
            yield return null;
        }

        UpdateUI(1f);   // Full bar

        // ------------------------------------------------------------------
        // 4. Hand‑off to the new scene
        // ------------------------------------------------------------------
        if (autoActivate)
        {
            op.allowSceneActivation = true;     // 🚀
        }
        else
        {
            // Wait for a button press, fade, etc., then:
            // op.allowSceneActivation = true;
        }
    }

    // ----------------------------------------------------------------------
    // Helper: pushes progress (0‑0.9) → (0‑1) into UI elements
    // ----------------------------------------------------------------------
    private void UpdateUI(float rawProgress)
    {
        float p = Mathf.Clamp01(rawProgress / 0.9f);
        if (progressBar) progressBar.value = p;
       
    }
}