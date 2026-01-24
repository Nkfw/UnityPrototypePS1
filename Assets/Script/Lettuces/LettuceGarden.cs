using System.Collections.Generic;
using UnityEngine;

// This script goes on the Garden GameObject
// Spawns lettuces at predefined spawn points
public class LettuceGarden : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject lettucePrefab;
    [SerializeField] private Transform[] spawnPoints; // Array of spawn point Transforms
    [SerializeField] private int maxLettuces = 4; // Maximum lettuces to maintain in level
    [SerializeField] private float spawnCheckInterval = 0.5f; // How often to check count
    [SerializeField] private LayerMask groundLayer = -1; // What counts as ground
    [SerializeField] private float groundRaycastDistance = 10f; // How far to raycast for ground

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private List<GameObject> spawnedLettuces = new List<GameObject>();
    private float nextCheckTime = 0f;
    private Dictionary<int, GameObject> spawnPointOccupancy = new Dictionary<int, GameObject>(); // Track which spawn point has which lettuce

    void Start()
    {
        // Validate setup
        if (lettucePrefab == null)
        {
            Debug.LogError("LettuceGarden: Lettuce Prefab is not assigned in Inspector!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("LettuceGarden: No spawn points assigned! Create empty GameObjects as children and assign them.");
            return;
        }

        // Spawn initial lettuces (up to maxLettuces)
        int toSpawn = Mathf.Min(maxLettuces, spawnPoints.Length);
        for (int i = 0; i < toSpawn; i++)
        {
            SpawnLettuceAt(i);
        }
    }

    void Update()
    {
        // Periodically check lettuce count
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + spawnCheckInterval;
            CheckAndSpawnLettuces();
        }
    }

    private void CheckAndSpawnLettuces()
    {
        // Remove null references (destroyed lettuces)
        spawnedLettuces.RemoveAll(lettuce => lettuce == null);

        // Clean up null entries from dictionary (destroyed/eaten lettuces)
        List<int> keysToRemove = new List<int>();
        foreach (var kvp in spawnPointOccupancy)
        {
            if (kvp.Value == null)
            {
                keysToRemove.Add(kvp.Key);
            }
        }
        foreach (int key in keysToRemove)
        {
            spawnPointOccupancy.Remove(key);
        }

        // Count total lettuces in world (including carried)
        int totalLettuces = GameObject.FindGameObjectsWithTag("Lettuce").Length;

        // Only spawn if below max
        if (totalLettuces >= maxLettuces)
        {
            return; // Already at max capacity
        }

        // Try to spawn at each spawn point until we reach maxLettuces
        for (int i = 0; i < spawnPoints.Length && totalLettuces < maxLettuces; i++)
        {
            if (IsSpawnPointEmpty(i))
            {
                SpawnLettuceAt(i);
                totalLettuces++; // Increment count after spawning
            }
        }
    }

    private void SpawnLettuceAt(int spawnIndex)
    {
        if (lettucePrefab == null || spawnPoints == null || spawnIndex >= spawnPoints.Length)
        {
            Debug.LogError("LettuceGarden: Invalid spawn configuration!");
            return;
        }

        Transform spawnPoint = spawnPoints[spawnIndex];
        if (spawnPoint == null)
        {
            Debug.LogError($"LettuceGarden: Spawn point at index {spawnIndex} is null!");
            return;
        }

        Vector3 spawnPosition = spawnPoint.position;

        // Raycast down from spawn point to find ground
        Vector3 rayStart = spawnPosition + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRaycastDistance, groundLayer))
        {
            spawnPosition.y = hit.point.y + 0.1f;
        }
        else
        {
            // Use spawn point's Y position if no ground found
            Debug.LogWarning($"LettuceGarden: No ground found at spawn point {spawnIndex}, using spawn point height");
        }

        // Spawn lettuce
        GameObject newLettuce = Instantiate(lettucePrefab, spawnPosition, Quaternion.identity);
        spawnedLettuces.Add(newLettuce);

        // Register this spawn point as occupied in dictionary
        spawnPointOccupancy[spawnIndex] = newLettuce;

        Debug.Log($"Garden spawned lettuce at spawn point {spawnIndex}: {spawnPosition}");
    }

    private bool IsSpawnPointEmpty(int spawnIndex)
    {
        if (spawnIndex >= spawnPoints.Length) return false;

        Transform spawnPoint = spawnPoints[spawnIndex];
        if (spawnPoint == null) return false;

        // Check dictionary first (most reliable - no physics race conditions)
        if (spawnPointOccupancy.ContainsKey(spawnIndex))
        {
            GameObject occupyingLettuce = spawnPointOccupancy[spawnIndex];
            if (occupyingLettuce != null)
            {
                // Check if lettuce is still grounded at this point
                Lettuce lettuce = occupyingLettuce.GetComponent<Lettuce>();
                if (lettuce != null && !lettuce.IsBeingCarried)
                {
                    return false; // Still occupied by grounded lettuce
                }
            }
            // If lettuce was destroyed or is being carried, remove from dictionary
            spawnPointOccupancy.Remove(spawnIndex);
        }

        // Fallback: Check if there's already a lettuce near this spawn point using physics
        // Use a slightly larger radius (0.75f) to account for lettuce size
        Collider[] colliders = Physics.OverlapSphere(spawnPoint.position, 0.75f);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Lettuce"))
            {
                // Double-check: only count if lettuce is not being carried
                Lettuce lettuce = col.GetComponent<Lettuce>();
                if (lettuce != null && !lettuce.IsBeingCarried)
                {
                    return false; // Spawn point occupied by grounded lettuce
                }
            }
        }

        return true; // Spawn point empty
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw spawn points
        if (spawnPoints != null)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                {
                    // Draw X mark at spawn point
                    Gizmos.color = Color.green;
                    Vector3 pos = spawnPoints[i].position;

                    // Draw X
                    float size = 0.3f;
                    Gizmos.DrawLine(pos + new Vector3(-size, 0, -size), pos + new Vector3(size, 0, size));
                    Gizmos.DrawLine(pos + new Vector3(-size, 0, size), pos + new Vector3(size, 0, -size));

                    // Draw line from garden to spawn point
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, pos);

                    // Draw spawn point index
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label(pos + Vector3.up * 0.5f, $"SP {i}");
                    #endif
                }
            }
        }

        // Draw lines to spawned lettuces (runtime only)
        if (Application.isPlaying && spawnedLettuces != null)
        {
            Gizmos.color = Color.yellow;
            foreach (GameObject lettuce in spawnedLettuces)
            {
                if (lettuce != null)
                {
                    Gizmos.DrawLine(transform.position, lettuce.transform.position);
                }
            }
        }
    }
}
