using System.Collections;
using System.Linq;
using UnityEngine;

// This script goes on the Sheep GameObject
// Handles attraction to lettuce and movement toward it
public class SheepAttraction : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f; // How far sheep can smell lettuce
    [SerializeField] private float sniffDelay = 1f; // Time before starting to move (seconds)
    [SerializeField] private float eatDuration = 2f; // Time spent eating (seconds)

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f; // Speed when walking to lettuce
    [SerializeField] private float reachDistance = 0.5f; // How close to get before eating
    [SerializeField] private LayerMask groundLayer = -1; // What counts as ground (set in Inspector)
    [SerializeField] private float groundRaycastDistance = 10f; // How far to raycast for ground
    [SerializeField] private float maxHeightAboveGround = 10f; // Safety check to prevent flying sheep

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool showGroundDebug = false; // Enable detailed ground positioning logs

    // Current state
    public enum SheepState
    {
        Idle,       // Standing around, looking for lettuce
        Sniffing,   // Detected lettuce, preparing to move
        Walking,    // Moving toward lettuce
        Eating,     // At lettuce, eating it
        Returning   // Returning to home position (territory system)
    }

    private SheepState currentState = SheepState.Idle;
    private ISheepAttraction targetAttraction;
    private Sheep sheepComponent;
    private Rigidbody rb;
    private Vector3 homePosition; // Where to return when in territory

    // Collider info for ground positioning
    private CapsuleCollider sheepCapsule;
    private float colliderBottomOffset; // How far collider extends below transform origin

    // Public property to expose current state (for SheepFollowingZone)
    public SheepState CurrentState => currentState;

    private void Awake()
    {
        sheepComponent = GetComponent<Sheep>();
        rb = GetComponent<Rigidbody>();

        // Ensure Rigidbody is set up correctly for rb.MovePosition()
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Get collider info for proper ground positioning
        sheepCapsule = GetComponent<CapsuleCollider>();
        if (sheepCapsule != null)
        {
            // Use the collider's actual bounds to calculate offset
            // Bounds are already in world space and account for scale automatically
            Bounds colliderBounds = sheepCapsule.bounds;

            // Calculate how far the bottom of the collider is below the transform position
            // transform.position.y is where the transform pivot is
            // bounds.min.y is the bottom of the collider in world space
            colliderBottomOffset = transform.position.y - colliderBounds.min.y;
        }
        else
        {
            Debug.LogWarning($"[SHEEP COLLIDER] {name} - No CapsuleCollider found! Using default offset.");
            colliderBottomOffset = 0.5f; // Fallback value
        }
    }

    private void Update()
    {
        // Don't do anything if being carried (check this FIRST before ground checks)
        if (sheepComponent != null && sheepComponent.IsBeingCarried)
        {
            // Reset state when picked up
            if (currentState != SheepState.Idle)
            {
                ResetState();
            }
            return;
        }

        // Check if sheep is standing on ground (only when NOT carried)
        // Ignore trigger colliders for ground detection
        RaycastHit groundChecker;
        if (!Physics.Raycast(transform.position, Vector3.down, out groundChecker, 0.5f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            // No ground below - enable falling
            if (rb != null && rb.isKinematic)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                Debug.Log($"Sheep {name}: Ground disappeared, falling!");
            }
            return; // Don't do any other AI logic while falling
        }

        // Normal AI behavior (sheep is on ground and not carried)
        switch (currentState)
        {
            case SheepState.Idle:
                LookForAttractions();
                break;

            case SheepState.Walking:
                MoveTowardTarget();
                break;

            case SheepState.Returning:
                ReturnToHome();
                break;

                // Sniffing and Eating are handled by coroutines
        }
    }

    private void ResetState()
    {
        // Cancel any ongoing coroutines
        StopAllCoroutines();

        // Clear target and reset to idle
        targetAttraction = null;
        currentState = SheepState.Idle;
    }

    private void LookForAttractions()
    {
        // Find all objects that implement ISheepAttraction
        ISheepAttraction[] allAttractions = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISheepAttraction>()
            .ToArray();

        if (allAttractions.Length == 0)
        {
            return;
        }

        // Find closest available attraction within detection radius
        ISheepAttraction closestAttraction = null;
        float closestDistance = detectionRadius;
        float highestPriority = 0f;

        foreach (ISheepAttraction attraction in allAttractions)
        {
            // Skip unavailable attractions (e.g., being carried)
            if (!attraction.IsAvailable())
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, attraction.GetPosition());

            // Check if within detection radius
            if (distance < closestDistance)
            {
                // Consider priority when choosing between attractions at similar distances
                float priority = attraction.GetPriority();

                if (closestAttraction == null || distance < closestDistance * 0.9f || priority > highestPriority)
                {
                    closestAttraction = attraction;
                    closestDistance = distance;
                    highestPriority = priority;
                }
            }
        }

        // If found attraction, start sniffing
        if (closestAttraction != null)
        {
            targetAttraction = closestAttraction;
            StartCoroutine(SniffBeforeMoving());
        }
    }

    private IEnumerator SniffBeforeMoving()
    {
        currentState = SheepState.Sniffing;

        // Wait for sniff delay
        yield return new WaitForSeconds(sniffDelay);

        // Check if attraction still exists and is available
        if (targetAttraction == null || !targetAttraction.IsAvailable())
        {
            targetAttraction = null;
            currentState = SheepState.Idle;
            yield break;
        }

        // Start walking
        currentState = SheepState.Walking;
    }

    private void MoveTowardTarget()
    {
        // Check if attraction was destroyed or became unavailable
        if (targetAttraction == null || !targetAttraction.IsAvailable())
        {
            targetAttraction = null;
            currentState = SheepState.Idle;
            return;
        }

        // Calculate direction to attraction
        Vector3 direction = (targetAttraction.GetPosition() - transform.position);
        direction.y = 0; // Keep movement horizontal
        float distance = direction.magnitude;

        // Check if reached attraction
        if (distance < reachDistance)
        {
            StartCoroutine(InteractWithTarget());
            return;
        }

        // Move toward lettuce
        Vector3 movement = direction.normalized * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + movement;

        // Raycast down to find ground level
        // Start from a high point to ensure we can find ground even if sheep is elevated
        // QueryTriggerInteraction.Ignore prevents hitting trigger colliders (like bridge triggers)
        Vector3 rayStart = new Vector3(newPosition.x, transform.position.y + 5f, newPosition.z);
        bool hitGround = Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRaycastDistance, groundLayer, QueryTriggerInteraction.Ignore);

        if (hitGround)
        {
            // Snap to ground with collider-aware positioning
            // Add colliderBottomOffset so the collider's bottom sits on the ground
            newPosition.y = hit.point.y + colliderBottomOffset + 0.05f; // 0.05f = small skin width

            // Debug visualization
            Debug.DrawLine(rayStart, hit.point, Color.green, 0.1f);

            // Detailed ground positioning debug
            if (showGroundDebug)
            {
                Debug.Log($"[GROUND] {name} - Hit: {hit.point.y:F2}, Transform will be: {newPosition.y:F2}, " +
                          $"Offset: {colliderBottomOffset:F2}, Collider on: {hit.collider.name}");
            }
        }
        else
        {
            // Fallback: keep current Y position if no ground found
            newPosition.y = transform.position.y;

            // Debug warnings
            Debug.LogWarning($"Sheep {name}: No ground found at position {newPosition}! Current Y: {transform.position.y}");
            Debug.DrawLine(rayStart, rayStart + Vector3.down * groundRaycastDistance, Color.red, 0.5f);
        }

        // Safety check: If sheep is unreasonably high, try to find ground from a very high point
        if (newPosition.y > maxHeightAboveGround)
        {
            Debug.LogWarning($"Sheep {name}: Detected unreasonable height {newPosition.y}! Attempting emergency ground detection.");

            // Try raycast from much higher with longer distance
            Vector3 emergencyRayStart = new Vector3(newPosition.x, 100f, newPosition.z);
            if (Physics.Raycast(emergencyRayStart, Vector3.down, out RaycastHit emergencyHit, 150f, groundLayer, QueryTriggerInteraction.Ignore))
            {
                newPosition.y = emergencyHit.point.y + colliderBottomOffset + 0.05f;
                Debug.Log($"Sheep {name}: Emergency ground found at Y={emergencyHit.point.y}");
            }
            else
            {
                // Last resort: reset to Y=0
                newPosition.y = 0.5f;
                Debug.LogError($"Sheep {name}: No ground found even with emergency raycast! Resetting to Y=0.5f");
            }
        }

        // Use Rigidbody to move (since sheep has kinematic Rigidbody)
        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }

        // Rotate to face lettuce
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    private IEnumerator InteractWithTarget()
    {
        currentState = SheepState.Eating;

        // Notify the attraction that sheep is interacting
        if (targetAttraction != null)
        {
            targetAttraction.OnSheepInteract(sheepComponent);
        }

        // Wait while interacting
        yield return new WaitForSeconds(eatDuration);

        // Check if attraction should be destroyed after interaction
        if (targetAttraction != null)
        {
            if (targetAttraction.ShouldDestroyAfterInteraction())
            {
                Destroy(targetAttraction.GetGameObject());
            }

            targetAttraction = null;
        }

        // Return to idle
        currentState = SheepState.Idle;
    }

    // ===== TERRITORY SYSTEM - Called by SheepFollowingZone =====

    public void StartReturningHome(Vector3 home)
    {
        homePosition = home;
        currentState = SheepState.Returning;
    }

    // Called by CheckpointManager when restoring sheep from checkpoint
    // Resets sheep to Idle state so it can detect attractions again
    public void ResetToIdleState()
    {
        currentState = SheepState.Idle;
        targetAttraction = null;
    }

    private void ReturnToHome()
    {
        // First, check if new attraction appeared (auto-cancel return)
        LookForAttractions();

        // If attraction found, state changed to Sniffing, exit
        if (currentState != SheepState.Returning)
        {
            return;
        }

        // Calculate distance to home
        float distanceToHome = Vector3.Distance(transform.position, homePosition);

        // Reached home?
        if (distanceToHome < reachDistance)
        {
            currentState = SheepState.Idle;
            return;
        }

        // Move toward home (same logic as MoveTowardTarget)
        Vector3 direction = (homePosition - transform.position).normalized;
        Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;

        if (rb != null)
        {
            rb.MovePosition(newPosition);
        }

        // Rotate to face home
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw line to target attraction
        if (targetAttraction != null && (currentState == SheepState.Walking || currentState == SheepState.Eating))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetAttraction.GetPosition());
        }

        // Draw line to home when returning
        if (currentState == SheepState.Returning)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, homePosition);

            // Draw home position marker
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(homePosition, 0.5f);
        }

        // Draw state indicator
        Vector3 statePos = transform.position + Vector3.up * 2f;
        Gizmos.color = currentState switch
        {
            SheepState.Idle => Color.white,
            SheepState.Sniffing => Color.cyan,
            SheepState.Walking => Color.green,
            SheepState.Eating => Color.red,
            SheepState.Returning => Color.magenta,
            _ => Color.white
        };
        Gizmos.DrawWireSphere(statePos, 0.3f);
    }
}
