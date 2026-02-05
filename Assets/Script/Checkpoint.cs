using UnityEngine;

// Attach this script to checkpoint trigger zones throughout the level
// When the player enters a checkpoint, it saves the current game state
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isStartingCheckpoint = false; // Is this the level start checkpoint?
    [SerializeField] private Transform spawnPoint; // Where player/sheep spawn (if null, uses checkpoint position)
    [SerializeField] private float spawnHeight = 2f; // How high above spawn point to drop from (portal effect)

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showDebugGizmos = true;

    [Header("Visual Feedback")]
    [SerializeField] private Color inactiveColor = Color.yellow;
    [SerializeField] private Color activeColor = Color.green;

    // Internal state
    private bool hasBeenActivated = false;

    void Start()
    {
        // Starting checkpoint auto-activates and saves immediately
        if (isStartingCheckpoint == true)
        {
            if (showDebugLogs == true)
            {
                Debug.Log($"Checkpoint '{gameObject.name}': This is the starting checkpoint (auto-save on start)");
            }

            // Don't set hasBeenActivated yet - let it save when player spawns on it
            // Or we can activate it immediately since initial checkpoint is already saved by CheckpointManager
            hasBeenActivated = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs == true)
        {
            Debug.Log($"Checkpoint '{gameObject.name}': OnTriggerEnter detected object '{other.name}' with tag '{other.tag}'");
        }

        // Only activate checkpoint when player enters (not sheep or other objects)
        if (other.CompareTag("Player") == false)
        {
            if (showDebugLogs == true)
            {
                Debug.Log($"Checkpoint '{gameObject.name}': Not the player, ignoring.");
            }
            return; // Not the player, ignore
        }

        // If already activated, don't save again
        if (hasBeenActivated == true)
        {
            if (showDebugLogs == true)
            {
                Debug.Log($"Checkpoint '{gameObject.name}': Player re-entered, but checkpoint already active. Skipping save.");
            }
            return;
        }

        // First time activation - save the checkpoint
        ActivateCheckpoint();
    }

    private void ActivateCheckpoint()
    {
        hasBeenActivated = true;

        if (showDebugLogs == true)
        {
            Debug.Log($"Checkpoint '{gameObject.name}': ACTIVATED! Saving game state...");
        }

        // Tell CheckpointManager to save the current state
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SaveCheckpoint(this);
        }
        else
        {
            Debug.LogError($"Checkpoint '{gameObject.name}': CheckpointManager.Instance is null! Cannot save checkpoint.");
        }

        // TODO: Play checkpoint activation sound effect
        // TODO: Play checkpoint activation particle effect
        // TODO: Change checkpoint visual (e.g., turn on light, change material color)
    }

    // Debug visualization in Scene view
    private void OnDrawGizmos()
    {
        if (showDebugGizmos == false)
        {
            return;
        }

        // Get the trigger collider to visualize its bounds
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            // Choose color based on activation state
            Color gizmoColor;
            if (Application.isPlaying == true)
            {
                // During play mode, show green if activated, yellow if not
                gizmoColor = (hasBeenActivated == true) ? activeColor : inactiveColor;
            }
            else
            {
                // In edit mode, show starting checkpoint as green, others as yellow
                gizmoColor = (isStartingCheckpoint == true) ? activeColor : inactiveColor;
            }

            // Draw semi-transparent filled box
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);

            // Draw wireframe outline
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }

        // Draw checkpoint icon above the trigger (flag-like marker)
        Vector3 iconPosition = transform.position + Vector3.up * 2f;
        Gizmos.color = (hasBeenActivated == true || isStartingCheckpoint == true) ? activeColor : inactiveColor;
        Gizmos.DrawSphere(iconPosition, 0.3f);

        // Draw spawn point if assigned
        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);

            // Draw spawn height indicator
            Vector3 spawnFrom = spawnPoint.position + Vector3.up * spawnHeight;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(spawnFrom, 0.3f);
            Gizmos.DrawLine(spawnPoint.position, spawnFrom);
        }
    }

    // Public getters
    public Vector3 GetSpawnPosition()
    {
        // If spawn point is assigned, use it; otherwise use checkpoint position
        Vector3 basePosition = (spawnPoint != null) ? spawnPoint.position : transform.position;
        return basePosition + Vector3.up * spawnHeight;
    }
}
