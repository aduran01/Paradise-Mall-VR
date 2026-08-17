using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stringSender : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

     private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Remember where we actually want to go…
      
       // SceneTransition.NextScene = destinationScene.Trim();

        sceneSaver.Instance.sceneName = gameObject.name.Trim();
        Debug.Log($"[Trigger] NextScene set to “{sceneSaver.Instance.sceneName}”");

        
    }
}
