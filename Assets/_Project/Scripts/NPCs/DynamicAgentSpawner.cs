// DynamicAgentSpawner.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class DynamicAgentSpawner : MonoBehaviour
{
    [Header("Camera & Player")]
    [SerializeField] Camera playerCamera;
    [SerializeField] Transform player;

    [Header("Agent Prefab & Pool")]
    [SerializeField] PatrolAgent[] agentPrefabs;
    [SerializeField] int maxAgents = 15;
    [SerializeField] float initialSpawnGap = 1.0f;

    [Header("Spawn Distances")]
    [SerializeField] float minSpawnDistance = 8f;
    [SerializeField] float probeRadius = 20f;

    [Header("Patrol Destinations (shared list)")]
    [SerializeField] Transform[] destinations;

    readonly List<PatrolAgent> _agents = new();

    void Start() => StartCoroutine(SpawnRoutine());
    void Update() => CullDeadAgents();

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (_agents.Count < maxAgents && TryGetSpawnLocation(out Vector3 pos))
            {
                var prefab = agentPrefabs[Random.Range(0, agentPrefabs.Length)];
                var agent = Instantiate(prefab, pos, Quaternion.identity);
                agent.Init(playerCamera, player, destinations);
                _agents.Add(agent);
            }
            yield return new WaitForSeconds(initialSpawnGap);
        }
    }

    void CullDeadAgents()
    {
        for (int i = _agents.Count - 1; i >= 0; --i)
        {
            if (_agents[i] == null || (_agents[i].HasExpired && !IsInView(_agents[i].transform.position)))
            {
                if (_agents[i] != null) Destroy(_agents[i].gameObject);
                _agents.RemoveAt(i);
            }
        }
    }

    bool TryGetSpawnLocation(out Vector3 pos)
    {
        for (int i = 0; i < 25; i++)
        {
            Vector3 dir = Random.insideUnitSphere * probeRadius;
            dir.y = 0;
            Vector3 candidate = player.position + dir;

            if (dir.magnitude < minSpawnDistance) continue;
            if (!IsInView(candidate))
            {
                pos = candidate;
                return true;
            }
        }
        pos = Vector3.zero;
        return false;
    }

    bool IsInView(Vector3 worldPos)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(playerCamera);
        return GeometryUtility.TestPlanesAABB(planes, new Bounds(worldPos, Vector3.one * 0.5f));
    }
}