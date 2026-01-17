using UnityEngine;

// This script goes on Lettuce GameObjects
// Handles pickup, drop, and throw interactions
public class Lettuce : MonoBehaviour
{
    [Header("State")]
    private bool isBeingCarried = false;

    [Header("References")]
    private Rigidbody rb;
    private Collider lettuceCollider;
    private Transform originalParent;

    public bool IsBeingCarried => isBeingCarried;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        lettuceCollider = GetComponent<Collider>();
    }

    // Called by PlayerInteraction when picked up
    public void OnPickedUp(Transform carryPosition)
    {
        isBeingCarried = true;
        originalParent = transform.parent;

        // Parent to player carry position
        transform.SetParent(carryPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Disable physics while being carried
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable collider so sheep don't detect it while carried
        if (lettuceCollider != null)
        {
            lettuceCollider.enabled = false;
        }

        Debug.Log($"Lettuce {name} picked up!");
    }

    // Called by PlayerInteraction when dropped (E key)
    public void OnDropped(Vector3 dropPosition)
    {
        isBeingCarried = false;

        // Unparent
        transform.SetParent(originalParent);
        transform.position = dropPosition;

        // Re-enable physics so it can fall
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Re-enable collider
        if (lettuceCollider != null)
        {
            lettuceCollider.enabled = true;
        }

        Debug.Log($"Lettuce {name} dropped at {dropPosition}");
    }
}
