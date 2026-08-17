using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime‑only inventory that survives scene loads *and* enforces
/// scene‑wide side‑effects (pickup removal, VFX, material swapping).
/// </summary>
[DisallowMultipleComponent]
public sealed class VRInventoryManager : MonoBehaviour
{
    /* ───────── singleton ───────── */
    public static VRInventoryManager Instance { get; private set; }

    /* ───────── user‑tunable hooks ───────── */
    [Header("Pickup Feedback")]
    [Tooltip("Plays for one second every time AddItem succeeds.")]
    public ParticleSystem pickupEffect;

    [System.Serializable]                        // one line per rule
    public class MaterialSwap
    {
        [Header("Trigger")]
        public ItemData      triggerItem;        // the collected item
        [Header("Target")]
        public Renderer      targetRenderer;     // any renderer in scene
        public Material      newMaterial;        // material to apply
    }

    [Header("Material Swaps (max 5)")]
    public MaterialSwap[] swaps = new MaterialSwap[5];

    /* ───────── persistent state ───────── */
    readonly Dictionary<ItemData, int> items = new();
    public  IReadOnlyDictionary<ItemData, int> Items => items;

    /// <summary>
    /// IDs of every ItemData ever collected during this play‑session.
    /// Used to cull duplicates when we revisit a scene.
    /// </summary>
    readonly HashSet<string> collectedIDs = new();

    

    /* ───────── lifecycle ───────── */
    void Awake()
    {
        
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy() =>
        SceneManager.sceneLoaded -= OnSceneLoaded;

    /* ───────── public API ───────── */
    public bool AddItem(ItemData item, int amount = 1)
    {
        if (!item || amount <= 0) return false;

        /* 1 ── stacking rules */
        if (!items.ContainsKey(item)) items[item] = 0;
        
        if (!item.stackable && items[item] > 0) return false;
        if ( item.stackable && items[item] + amount > item.maxStack) return false;

        /* 2 ── commit */
        items[item] += amount;

        if (amount != 0)
        {
            Debug.Log("Item added into inventory");
        }

        /* 3 ── side‑effects */
        collectedIDs.Add(item.id);               // 3a  mark collected
        StartCoroutine(PlayPickupEffect());      // 3b  VFX
        ApplyMaterialSwaps(item);                // 3c  material rules

        return true;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        if (!item || amount <= 0 || !items.TryGetValue(item, out int owned) || owned < amount)
            return false;

        owned -= amount;
        if (owned == 0) items.Remove(item);
        else            items[item] = owned;
        return true;
    }

    public bool Contains(ItemData item, int atLeast = 1) =>
        item && items.TryGetValue(item, out int owned) && owned >= atLeast;

    /* ───────── private helpers ───────── */
    IEnumerator PlayPickupEffect()
    {
        if (!pickupEffect) yield break;

        

        pickupEffect.gameObject.SetActive(true);
        pickupEffect.Play();
        Debug.Log("pickup effect playing");
        yield return new WaitForSeconds(2f);

        pickupEffect.Stop(true,
            ParticleSystemStopBehavior.StopEmittingAndClear);
        pickupEffect.gameObject.SetActive(false);
    }

    void ApplyMaterialSwaps(ItemData newlyAdded)
    {
        foreach (var s in swaps)
        {
            if (!s.triggerItem)
            {
                Debug.LogWarning("Swap skipped: triggerItem is null.");
                continue;
            }
            if (!s.targetRenderer)
            {
                Debug.LogWarning("Swap skipped: targetRenderer is null.");
                continue;
            }
            if (!s.newMaterial)
            {
                Debug.LogWarning("Swap skipped: newMaterial is null.");
                continue;
            }

            if (s.triggerItem == newlyAdded)
            {
                s.targetRenderer.material = s.newMaterial;
                Debug.Log("Material successfully swapped for: " + s.triggerItem.name);
            }
            else
            {
                Debug.Log("TriggerItem does not match newlyAdded: " + s.triggerItem.name);
            }
        }

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        /* A ── remove world copies already picked up */
        var worldItems = FindObjectsOfType<ItemInScenes>(true);
        foreach (var wi in worldItems)
            if (wi && wi.data && collectedIDs.Contains(wi.data.id))
                Destroy(wi.gameObject);

        /* B ── re‑apply material swaps so they persist */
        // foreach (var s in swaps)
        // {
        //     if (!s.triggerItem || !s.targetRenderer || !s.newMaterial) continue;
        //     if (collectedIDs.Contains(s.triggerItem.id))
        //         s.targetRenderer.material = s.newMaterial;
        // }
    }
}
