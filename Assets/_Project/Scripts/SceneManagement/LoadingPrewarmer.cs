// LoadingPrewarmer.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingPrewarmer : MonoBehaviour
{
    private IEnumerator Start()
    {
        // Already in memory? Great, bail.
        if (SceneManager.GetSceneByName("Loading Scene").isLoaded)
            yield break;

        // 1. Load additively, invisible.
        AsyncOperation op = SceneManager.LoadSceneAsync("Loading Scene", LoadSceneMode.Additive);
        op.allowSceneActivation = true;
        yield return op;

        // 2. Disable every root GO so it doesn’t render yet.
        foreach (GameObject root in SceneManager.GetSceneByName("Loading Scene").GetRootGameObjects())
            root.SetActive(false);

        // Scene now sits idle in RAM for instant swapping later.
        Debug.Log("Loading Scene pre‑warmed.");
    }
}
