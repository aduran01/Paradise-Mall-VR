using UnityEngine;
using System.Collections.Generic;

public class FriesSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] prefabs;           // The prefabs to spawn

    [Header("Spawn Parameters")]
    public int numberToSpawn = 5;          // How many to spawn each cycle
    public int numberInTank = 20;          // Maximum total number allowed in the scene
    public float verticalOffset = 0.05f;   // Vertical offset for spawning

    [Header("Timing")]
    public float spawnInterval = 1f;       // Spawn cycle interval (seconds)

    // Tracks current total number of spawned prefabs.
    public int currentNumber { get; private set; }

    private Collider objectCollider;
    private Bounds colliderBounds;
    private List<GameObject> spawnedPrefabs = new List<GameObject>();

    private void Start()
    {
        // Safety checks
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs assigned. Nothing will be spawned.");
            return;
        }

        objectCollider = GetComponent<Collider>();
        if (objectCollider == null)
        {
            Debug.LogError("No Collider found on this GameObject. Cannot spawn on top of it.");
            return;
        }

        colliderBounds = objectCollider.bounds;

        // Begin the repeated spawn cycle
        InvokeRepeating(nameof(SpawnCycle), spawnInterval, spawnInterval);
    }

    private void SpawnCycle()
    {
        // If adding 'numberToSpawn' new objects would exceed 'numberInTank',
        // remove (destroy) enough older prefabs to stay within limits.
        int currentCount = spawnedPrefabs.Count;
        int capacityOverflow = (currentCount + numberToSpawn) - numberInTank;

        if (capacityOverflow > 0)
        {
            // Destroy oldest prefabs to make space for the new ones
            for (int i = 0; i < capacityOverflow; i++)
            {
                if (spawnedPrefabs.Count == 0) break;

                GameObject oldest = spawnedPrefabs[0];
                spawnedPrefabs.RemoveAt(0);
                Destroy(oldest);
            }
        }

        // Now spawn the requested number of new prefabs
        SpawnObjects(numberToSpawn);

        // Update currentNumber and log it
        currentNumber = spawnedPrefabs.Count;
        Debug.Log("Number of prefabs currently in the scene: " + currentNumber);
    }

    private void SpawnObjects(int count)
    {
        float xMin = colliderBounds.center.x - colliderBounds.extents.x;
        float xMax = colliderBounds.center.x + colliderBounds.extents.x;
        float zMin = colliderBounds.center.z - colliderBounds.extents.z;
        float zMax = colliderBounds.center.z + colliderBounds.extents.z;
        float topY = colliderBounds.center.y + colliderBounds.extents.y;

        for (int i = 0; i < count; i++)
        {
            // Pick a random prefab
            GameObject prefabToSpawn = prefabs[Random.Range(0, prefabs.Length)];

            // Random X and Z within collider
            float randomX = Random.Range(xMin, xMax);
            float randomZ = Random.Range(zMin, zMax);

            // Raycast from just above the top surface to ensure we hit our collider
            Vector3 rayStart = new Vector3(randomX, topY + 1f, randomZ);
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 5f))
            {
                // Ensure the ray hits this object's collider
                if (hit.collider == objectCollider)
                {
                    Vector3 spawnPos = hit.point + Vector3.up * verticalOffset;

                    Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), Random.Range(0f, 360f));


                    GameObject newObj = Instantiate(prefabToSpawn, spawnPos, randomRotation);
                    spawnedPrefabs.Add(newObj);
                }
            }
        }
    }
}
