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

    // Current state
    private enum SheepState
    {
        Idle,       // Standing around, looking for lettuce
        Sniffing,   // Detected lettuce, preparing to move
        Walking,    // Moving toward lettuce
        Eating      // At lettuce, eating it
    }

    private SheepState currentState = SheepState.Idle;
    private ISheepAttraction targetAttraction;
    private Sheep sheepComponent;
    private Rigidbody rb;

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
        RaycastHit groundChecker;
        if (!Physics.Raycast(transform.position, Vector3.down, out groundChecker, 0.5f, groundLayer))
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

                // Sniffing and Interacting are handled by coroutines
        }
    }

    private void ResetState()
    {
        // Cancel any ongoing coroutines
        StopAllCoroutines();

        // Clear target and reset to idle
        targetAttraction = null;
        currentState = SheepState.Idle;

        Debug.Log($"Sheep {name}: State reset (picked up)");
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
        Debug.Log($"Sheep {name}: Sniffing attraction...");

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
        Debug.Log($"Sheep {name}: Walking to attraction!");
    }

    private void MoveTowardTarget()
    {
        // Check if attraction was destroyed or became unavailable
        if (targetAttraction == null || !targetAttraction.IsAvailable())
        {
            Debug.Log($"Sheep {name}: Attraction no longer available, stopping pursuit");
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
        Vector3 rayStart = new Vector3(newPosition.x, transform.position.y + 5f, newPosition.z);
        bool hitGround = Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRaycastDistance, groundLayer);

        if (hitGround)
        {
            // Snap to ground with slight offset
            newPosition.y = hit.point.y + 0.1f;

            // Debug visualization
            Debug.DrawLine(rayStart, hit.point, Color.green, 0.1f);
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
            if (Physics.Raycast(emergencyRayStart, Vector3.down, out RaycastHit emergencyHit, 150f, groundLayer))
            {
                newPosition.y = emergencyHit.point.y + 0.1f;
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
        Debug.Log($"Sheep {name}: Interacting with attraction...");

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
                Debug.Log($"Sheep {name}: Finished interacting! Destroying attraction.");
                Destroy(targetAttraction.GetGameObject());
            }
            else
            {
                Debug.Log($"Sheep {name}: Finished interacting! Attraction remains.");
            }

            targetAttraction = null;
        }

        // Return to idle
        currentState = SheepState.Idle;
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

        // Draw state indicator
        Vector3 statePos = transform.position + Vector3.up * 2f;
        Gizmos.color = currentState switch
        {
            SheepState.Idle => Color.white,
            SheepState.Sniffing => Color.cyan,
            SheepState.Walking => Color.green,
            SheepState.Eating => Color.red,
            _ => Color.white
        };
        Gizmos.DrawWireSphere(statePos, 0.3f);
    }
}
