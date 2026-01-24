using UnityEngine;

// This script goes on Lettuce GameObjects
// Handles pickup, drop, and throw interactions
public class Lettuce : MonoBehaviour, ISheepAttraction
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

    // ===== ISheepAttraction Interface Implementation =====

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public bool IsAvailable()
    {
        // Lettuce is available if it's not being carried by the player
        return !isBeingCarried;
    }

    public float GetPriority()
    {
        // Default priority for lettuce
        return 1.0f;
    }

    public void OnSheepInteract(Sheep sheep)
    {
        // Lettuce doesn't need special interaction logic
        // The destruction is handled by SheepAttraction based on ShouldDestroyAfterInteraction()
        Debug.Log($"Sheep {sheep.name} is eating lettuce {name}");
    }

    public bool ShouldDestroyAfterInteraction()
    {
        // Lettuce gets eaten and destroyed after sheep interacts with it
        return true;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
