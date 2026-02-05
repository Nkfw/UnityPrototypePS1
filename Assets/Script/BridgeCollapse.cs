using UnityEngine;

// Handles bridge collapse logic
// Bridges are HAZARD-type destructibles: they always respawn on death (prevents soft-locking)
// When you add walls/obstacles that should stay destroyed, create a separate BreakableWall.cs script
public class BridgeCollapse : MonoBehaviour
{
    [Header("Debug - Read Only")]
    [SerializeField] private bool playerOnBridge;
    [SerializeField] private bool sheepOnBridge; // Note: Will be false if sheep is carried

    private PlayerInteraction playerReference;
    private float enableTime; // Time when bridge was last enabled
    private const float safetyDelay = 0.5f; // Delay before bridge can collapse after being restored

    // Called when the GameObject is re-enabled (e.g., when CheckpointManager restores it)
    private void OnEnable()
    {
        // Reset state variables to prevent old state from causing issues
        playerOnBridge = false;
        sheepOnBridge = false;
        playerReference = null;
        enableTime = Time.time; // Record when bridge was enabled
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerReference = other.GetComponent<PlayerInteraction>();
            playerOnBridge = true;
            CheckCollapse();
        }

        if (other.CompareTag("Sheep"))
        {
            sheepOnBridge = true;
            CheckCollapse();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerReference = null;
            playerOnBridge = false;
        }

        if (other.CompareTag("Sheep"))
        {
            sheepOnBridge = false;
        }
    }

    private void CheckCollapse()
    {
        // Early exit if no player reference
        if (playerReference == null) return;

        // Safety delay - don't allow collapse immediately after being restored
        if (Time.time < enableTime + safetyDelay) return;

        // Bridge collapses if player is carrying sheep
        if (playerReference.IsCarryingSheep && playerOnBridge)
        {
            CollapseBridge();
            return;
        }

        // Bridge also collapses if player and separate sheep are both on bridge
        if (playerOnBridge && sheepOnBridge)
        {
            CollapseBridge();
        }
    }

    // Collapses the bridge by disabling it
    private void CollapseBridge()
    {
        gameObject.SetActive(false);
    }
}
