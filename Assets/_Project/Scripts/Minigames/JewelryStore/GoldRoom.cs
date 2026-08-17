using UnityEngine;
using System.Collections.Generic;

public class GoldRoom : MonoBehaviour
{
    public Material newMaterial; 
    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    private Renderer[] renderers;
    private bool isActive = false;

    void Awake()
    {
        renderers = FindObjectsOfType<Renderer>(); // Cache all renderers in the scene
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isActive)
            {
                isActive = true;
                ChangeMaterials();
            }
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            if (isActive)
            {
                isActive = false;
                RestoreMaterials();
            }
        }
    }

    void ChangeMaterials()
    {
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0) continue; // Safety checks
            
            // Store original materials only if not already stored
            if (!originalMaterials.ContainsKey(renderer.gameObject))
            {
                originalMaterials[renderer.gameObject] = renderer.sharedMaterials.Clone() as Material[];
            }

            // Apply the new material to all material slots
            Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = newMaterial;
            }
            renderer.materials = newMaterials;
        }
    }

    void RestoreMaterials()
    {
        foreach (var entry in originalMaterials)
        {
            if (entry.Key == null) continue; // Skip destroyed objects

            Renderer renderer = entry.Key.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.materials = entry.Value; // Restore original materials
            }
        }
    }
}

// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;
// using System.Collections.Generic;

// public class GoldRoom : MonoBehaviour
// {
//     public Material newMaterial; 
//     private XRGrabInteractable grabInteractable;
//     private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
//     private Renderer[] renderers;
//     private bool isActive = false;

//     void Awake()
//     {
//         grabInteractable = GetComponent<XRGrabInteractable>();
        
//         renderers = FindObjectsOfType<Renderer>(); // Cache renderers at startup
//     }

//     void OnEnable()
//     {
//         grabInteractable.selectEntered.AddListener(OnSelectEntered);
//         grabInteractable.selectExited.AddListener(OnSelectExited);
//     }

//     void OnDisable()
//     {
//         grabInteractable.selectEntered.RemoveListener(OnSelectEntered);
//         grabInteractable.selectExited.RemoveListener(OnSelectExited);
//     }

//     void OnSelectEntered(SelectEnterEventArgs args)
//     {
//         if (!isActive)
//         {
//             isActive = true;
//             ChangeMaterials();
//         }
//     }

//     void OnSelectExited(SelectExitEventArgs args)
//     {
//         if (isActive)
//         {
//             isActive = false;
//             RestoreMaterials();
//         }
//     }

//     void ChangeMaterials()
//     {
//         foreach (Renderer renderer in renderers)
//         {
//             if (renderer == null || renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0) continue; // Safety checks
            
//             // Store original materials only if not already stored
//             if (!originalMaterials.ContainsKey(renderer.gameObject))
//             {
//                 originalMaterials[renderer.gameObject] = renderer.sharedMaterials.Clone() as Material[];
//             }

//             // Check if materials are already changed, skip if they are
//             bool alreadyChanged = true;
//             foreach (Material mat in renderer.sharedMaterials)
//             {
//                 if (mat != newMaterial)
//                 {
//                     alreadyChanged = false;
//                     break;
//                 }
//             }
//             if (alreadyChanged) continue; // Skip redundant changes

//             // Apply the new material to all material slots
//             Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
//             for (int i = 0; i < newMaterials.Length; i++)
//             {
//                 newMaterials[i] = newMaterial;
//             }
//             renderer.materials = newMaterials; // Apply new materials
//         }
//     }

//     void RestoreMaterials()
//     {
//         List<GameObject> restoredObjects = new List<GameObject>(); // Track restored objects

//         foreach (var entry in originalMaterials)
//         {
//             if (entry.Key == null) continue; // Skip destroyed objects

//             Renderer renderer = entry.Key.GetComponent<Renderer>();
//             if (renderer != null)
//             {
//                 renderer.materials = entry.Value; // Restore original materials
//                 restoredObjects.Add(entry.Key); // Mark for removal
//             }
//         }

//         // Remove only restored entries to avoid unnecessary memory usage
//         foreach (GameObject obj in restoredObjects)
//         {
//             originalMaterials.Remove(obj);
//         }
//     }
// }
