using UnityEngine;

// Attach this to the KillZone GameObject (large plane beneath the level)
// Detects when player or sheep falls off the map and triggers death/respawn
// The KillZone should have a Box Collider with "Is Trigger" enabled
public class KillZone : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true; // Show fall messages in console

    // Called when something enters the kill zone trigger
    // This fires when any collider enters the trigger volume
    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs == true)
        {
            Debug.Log($"KillZone: Detected '{other.name}' entering kill zone. Tag: '{other.tag}'");
        }

        // Check if the player fell off the map
        if (other.CompareTag("Player") == true)
        {
            if (showDebugLogs == true)
            {
                Debug.Log($"KillZone: Player fell off the map!");
            }

            // Tell DeathManager that player died by falling
            if (DeathManager.Instance != null)
            {
                DeathManager.Instance.OnDeath(DeathManager.DeathCause.PlayerFell);
            }
            else
            {
                Debug.LogError("KillZone: DeathManager.Instance is null! Make sure DeathManager exists in scene.");
            }
        }

        // Check if a sheep fell off the map
        // Sheep death = level failure (player loses)
        if (other.CompareTag("Sheep") == true)
        {
            Sheep sheepComponent = other.GetComponent<Sheep>();
            if (sheepComponent != null)
            {
                if (showDebugLogs == true)
                {
                    Debug.Log($"KillZone: Sheep '{other.name}' fell off the map! Level failed.");
                }

                // Tell DeathManager that sheep died by falling
                // This will trigger level restart from checkpoint
                if (DeathManager.Instance != null)
                {
                    if (showDebugLogs == true)
                    {
                        Debug.Log("KillZone: Calling DeathManager.OnDeath(SheepFell)...");
                    }
                    DeathManager.Instance.OnDeath(DeathManager.DeathCause.SheepFell);
                }
                else
                {
                    Debug.LogError("KillZone: DeathManager.Instance is NULL! Make sure GameManager exists in scene with DeathManager script attached!");
                }
            }
        }
    }

    // Debug visualization in Scene view
    // Shows the kill zone as a red plane so you can see where it is
    private void OnDrawGizmos()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            // Draw a red transparent box showing the kill zone
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f); // Red with 30% opacity
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);

            // Draw a red wireframe outline
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
}
