everyusing UnityEngine;

[CreateAssetMenu(menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Meta")]
    public string id;            // Stable GUID‑like key (no spaces)
    public string displayName;   // Shown in UI

    [Header("The Prefab")]
    public GameObject worldPrefab;  // The pickup object in the scene

    [Header("Stacking")]
    public bool stackable = false;
    public int maxStack = 1;
}

