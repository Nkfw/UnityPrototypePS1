using UnityEngine;

// This script goes on each Sheep GameObject
// Handles basic sheep state (idle, carried)
public class Sheep : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool isBeingCarried = false;

    [Header("References")]
    private Rigidbody rb;
    private Collider sheepCollider;

    // Original parent before being picked up (usually null)
    private Transform originalParent;

    public bool IsBeingCarried => isBeingCarried;

    private void Awake()
    {
        // Get references to physics components
        rb = GetComponent<Rigidbody>();
        sheepCollider = GetComponent<Collider>();

        // Lock rotation so sheep doesn't tilt when moving/colliding
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    // Called by PlayerInteraction when sheep is picked up
    public void OnPickedUp(Transform carryPosition)
    {
        isBeingCarried = true;

        // Store original parent
        originalParent = transform.parent;

        // Parent sheep to carry position
        transform.SetParent(carryPosition);
        transform.localPosition = Vector3.zero; // Center on carry position
        transform.localRotation = Quaternion.identity; // Reset rotation

        // Disable physics while being carried
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable collision while being carried (optional)
        if (sheepCollider != null)
        {
            sheepCollider.enabled = false;
        }

        Debug.Log($"Sheep {name} picked up!");
    }

    // Called by PlayerInteraction when sheep is dropped
    public void OnDropped(Vector3 dropPosition)
    {
        isBeingCarried = false;

        // Unparent sheep
        transform.SetParent(originalParent);

        // Set position on ground
        transform.position = dropPosition;

        // Keep kinematic for SheepAttraction movement system
        if (rb != null)
        {
            rb.isKinematic = true; // Must be kinematic for rb.MovePosition()
            rb.useGravity = false; // No gravity, arcade-style
            // Note: Cannot set linearVelocity on kinematic bodies
        }

        // Re-enable collision
        if (sheepCollider != null)
        {
            sheepCollider.enabled = true;
        }

        Debug.Log($"Sheep {name} dropped at {dropPosition}");
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        // Draw different color based on state
        if (isBeingCarried)
        {
            Gizmos.color = Color.green;
        }
        else
        {
            Gizmos.color = Color.white;
        }

        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
