using System.Collections.Generic;
using UnityEngine;

// This script goes on the Garden GameObject
// Maintains exactly 4 lettuces in the world at all times
public class LettuceGarden : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject lettucePrefab;
    [SerializeField] private int maxLettuces = 4;
    [SerializeField] private float spawnRadius = 3f; // Spawn area around garden
    [SerializeField] private float spawnCheckInterval = 0.5f; // How often to check count
    [SerializeField] private LayerMask groundLayer = -1; // What counts as ground
    [SerializeField] private float groundRaycastDistance = 10f; // How far to raycast for ground

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    private List<GameObject> spawnedLettuces = new List<GameObject>();
    private float nextCheckTime = 0f;

    void Start()
    {
        // Spawn initial 4 lettuces
        for (int i = 0; i < maxLettuces; i++)
        {
            SpawnLettuce();
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

        // Count how many lettuces exist in world (including carried ones)
        int totalLettuces = GameObject.FindGameObjectsWithTag("Lettuce").Length;

        // Spawn new ones if below max
        int toSpawn = maxLettuces - totalLettuces;
        for (int i = 0; i < toSpawn; i++)
        {
            SpawnLettuce();
        }
    }

    private void SpawnLettuce()
    {
        // Check if prefab is assigned
        if (lettucePrefab == null)
        {
            Debug.LogError("LettuceGarden: Lettuce Prefab is not assigned in Inspector!");
            return;
        }

        // Random position within spawn radius (horizontal only)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = new Vector3(
            transform.position.x + randomCircle.x,
            transform.position.y,
            transform.position.z + randomCircle.y
        );

        // Raycast down to find ground
        Vector3 rayStart = spawnPosition + Vector3.up * 5f; // Start raycast above
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRaycastDistance, groundLayer))
        {
            // Spawn at ground level with small offset
            spawnPosition.y = hit.point.y + 0.1f;
        }
        else
        {
            // Fallback: use garden's Y position if no ground found
            spawnPosition.y = transform.position.y;
            Debug.LogWarning($"LettuceGarden: No ground found at spawn position, using garden height");
        }

        // Spawn lettuce
        GameObject newLettuce = Instantiate(lettucePrefab, spawnPosition, Quaternion.identity);
        spawnedLettuces.Add(newLettuce);

        Debug.Log($"Garden spawned lettuce at {spawnPosition}");
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw spawn radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Draw lines to spawned lettuces
        if (Application.isPlaying)
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
