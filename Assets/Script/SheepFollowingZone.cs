using UnityEngine;

// Attach this to the Sheep GameObject
// Handles territory behavior: sheep returns home if inside zone and idle for too long
public class SheepFollowingZone : MonoBehaviour
{
    [Header("Territory Settings")]
    [SerializeField] private float thinkingDuration = 5f; // How long to wait after eating before returning home

    [Header("Debug - Read Only")]
    [SerializeField] private bool isInsideZone = true; // Is sheep currently in territory?
    [SerializeField] private bool hasEscaped = false; // Has sheep ever left the zone?
    [SerializeField] private float thinkingTimer = 0f; // Current thinking countdown
    [SerializeField] private Vector3 homePosition; // Where sheep should return (saved on first Start)

    // References
    private SheepAttraction sheepAttraction;
    private Sheep sheep;

    private void Start()
    {
        // Store home position ONLY if not already set
        // This allows you to manually set it in Inspector if needed
        if (homePosition == Vector3.zero)
        {
            homePosition = transform.position;
            Debug.Log($"Sheep {name}: Home position set to {homePosition}");
        }

        // Get component references
        sheepAttraction = GetComponent<SheepAttraction>();
        sheep = GetComponent<Sheep>();

        // Initialize thinking timer
        thinkingTimer = thinkingDuration;
    }

    private void Update()
    {
        // Don't do anything if sheep has escaped the zone permanently
        if (hasEscaped)
        {
            return;
        }

        // Don't do anything if being carried by player
        if (sheep.IsBeingCarried)
        {
            // Reset timer when picked up
            thinkingTimer = thinkingDuration;
            return;
        }

        // Only do territory logic if inside the zone
        if (!isInsideZone)
        {
            return;
        }

        // Get current state from SheepAttraction
        SheepAttraction.SheepState currentState = sheepAttraction.CurrentState;

        // If sheep is doing something (not idle/returning), reset timer
        if (currentState != SheepAttraction.SheepState.Idle &&
            currentState != SheepAttraction.SheepState.Returning)
        {
            thinkingTimer = thinkingDuration; // Full reset
            return;
        }

        // If sheep is idle, count down
        if (currentState == SheepAttraction.SheepState.Idle)
        {
            thinkingTimer -= Time.deltaTime;

            // Timer expired! No new attractions found, time to go home
            if (thinkingTimer <= 0f)
            {
                sheepAttraction.StartReturningHome(homePosition);
                thinkingTimer = thinkingDuration; // Reset for next cycle
                Debug.Log($"Sheep {name}: Thinking time over ({thinkingDuration}s), returning home to {homePosition}!");
            }
        }

        // If returning, do nothing (timer paused, sheep already going home)
    }

    // ===== PUBLIC METHODS - Called by TerritoryZone.cs =====

    public void OnEnteredTerritory()
    {
        isInsideZone = true;
        Debug.Log($"Sheep {name}: Entered territory zone");
        // Note: We don't reset hasEscaped, because once escaped, sheep is free forever
    }

    public void OnExitedTerritory()
    {
        isInsideZone = false;
        hasEscaped = true; // Mark as escaped permanently
        Debug.Log($"Sheep {name}: Escaped territory! Now free forever.");
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        // Show home position during play mode
        if (Application.isPlaying && homePosition != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(homePosition, 0.3f);

            // Draw line from current position to home
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, homePosition);
        }
    }
}
