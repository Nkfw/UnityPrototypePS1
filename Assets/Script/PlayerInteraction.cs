using UnityEngine;
using UnityEngine.InputSystem;

// This script goes on the Player
// Handles interaction with objects (pickup sheep, use items)
public class PlayerInteraction : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private Transform carryPosition; // Where sheep is held (in front of player)
    [SerializeField] private float pickupRange = 2f; // How close player needs to be
    [SerializeField] private LayerMask sheepLayer; // Optional: filter to only sheep layer

    [Header("Drop Settings")]
    [SerializeField] private float dropDistance = 1.4f; // How far in front to drop
    [SerializeField] private LayerMask groundLayer = -1; // What counts as ground
    [SerializeField] private float groundRaycastDistance = 10f; // How far to raycast for ground

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;

    // References
    private PlayerController playerController;
    private Sheep currentSheep; // The sheep being carried (null if not carrying)
    private Lettuce currentLettuce; // The lettuce being carried (null if not carrying)

    // Track nearby items
    private Sheep nearestSheep;

    // Item type tracking
    private enum CarriedItemType
    {
        None,
        Sheep,
        Lettuce
    }

    private CarriedItemType currentItemType = CarriedItemType.None;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();

        // If no carry position assigned, create one in front of player
        if (carryPosition == null)
        {
            GameObject carryPoint = new GameObject("CarryPosition");
            carryPoint.transform.SetParent(transform);
            carryPoint.transform.localPosition = new Vector3(0f, 1.5f, 1f); // Front and slightly up
            carryPosition = carryPoint.transform;
        }
    }

    // Called automatically by Input System when player presses E (Use action)
    public void OnUse(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // If carrying something, drop it
        if (currentItemType != CarriedItemType.None)
        {
            DropCurrentItem();
        }
        // Otherwise, try to pick up nearest item
        else
        {
            TryPickupNearestItem();
        }
    }

    private void Update()
    {
        // Only look for items if not currently carrying anything
        if (currentItemType == CarriedItemType.None)
        {
            FindNearestSheep();
        }
    }

    private void FindNearestSheep()
    {
        // Find all GameObjects with "Sheep" tag
        GameObject[] sheepObjects = GameObject.FindGameObjectsWithTag("Sheep");

        if (sheepObjects.Length == 0)
        {
            nearestSheep = null;
            return;
        }

        // Find the closest sheep within pickup range (in front of player)
        float closestDistance = pickupRange;
        Sheep closest = null;

        foreach (GameObject sheepObj in sheepObjects)
        {
            // Check if sheep is in front of player
            Vector3 directionToSheep = sheepObj.transform.position - transform.position;
            float dotProduct = Vector3.Dot(transform.forward, directionToSheep.normalized);

            // Only consider sheep in front (dot product > 0 means forward direction)
            if (dotProduct < 0.3f) continue; // 0.3f allows ~70 degree cone in front

            float distance = Vector3.Distance(transform.position, sheepObj.transform.position);

            if (distance < closestDistance)
            {
                Sheep sheep = sheepObj.GetComponent<Sheep>();
                if (sheep != null && !sheep.IsBeingCarried)
                {
                    closestDistance = distance;
                    closest = sheep;
                }
            }
        }

        nearestSheep = closest;
    }

    private void TryPickupNearestItem()
    {
        // Try sheep first
        if (nearestSheep != null)
        {
            PickupSheep(nearestSheep);
            return;
        }

        // Try lettuce second
        Lettuce nearestLettuce = FindNearestLettuce();
        if (nearestLettuce != null)
        {
            PickupLettuce(nearestLettuce);
            return;
        }
    }

    private Lettuce FindNearestLettuce()
    {
        GameObject[] lettuces = GameObject.FindGameObjectsWithTag("Lettuce");

        if (lettuces.Length == 0) return null;

        float closestDistance = pickupRange;
        Lettuce closest = null;

        foreach (GameObject lettuceObj in lettuces)
        {
            Lettuce lettuce = lettuceObj.GetComponent<Lettuce>();
            if (lettuce == null || lettuce.IsBeingCarried) continue;

            // Check if lettuce is in front of player
            Vector3 directionToLettuce = lettuceObj.transform.position - transform.position;
            float dotProduct = Vector3.Dot(transform.forward, directionToLettuce.normalized);

            // Only consider lettuce in front (dot product > 0 means forward direction)
            if (dotProduct < 0.3f) continue; // 0.3f allows ~70 degree cone in front

            float distance = Vector3.Distance(transform.position, lettuceObj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = lettuce;
            }
        }

        return closest;
    }

    private void PickupSheep(Sheep sheep)
    {
        currentSheep = sheep;
        currentItemType = CarriedItemType.Sheep;
        currentSheep.OnPickedUp(carryPosition);

        // Notify player controller that we're carrying something
        if (playerController != null)
        {
            playerController.SetCarryingState(true);
        }
    }

    private void PickupLettuce(Lettuce lettuce)
    {
        currentLettuce = lettuce;
        currentItemType = CarriedItemType.Lettuce;
        currentLettuce.OnPickedUp(carryPosition);
    }

    private void DropCurrentItem()
    {
        // Calculate drop position in front of player
        Vector3 dropPosition = transform.position + transform.forward * dropDistance;

        // Get sheep's collider offset if we're dropping a sheep
        float sheepOffset = 0.5f; // Default fallback
        if (currentSheep != null)
        {
            // Calculate offset from collider properties, not current position
            // (Current position is unreliable because sheep is being carried high in the air)
            CapsuleCollider sheepCollider = currentSheep.GetComponent<CapsuleCollider>();
            if (sheepCollider != null)
            {
                // Calculate from collider dimensions in local space
                // For a Y-axis capsule: offset = (height/2) - center.y
                float localHeight = sheepCollider.height;
                float localCenterY = sheepCollider.center.y;
                sheepOffset = (localHeight / 2f) - localCenterY;
            }
        }

        // Raycast down to find ground from player's feet position
        // QueryTriggerInteraction.Ignore ensures we don't hit trigger colliders (like bridge triggers)
        Vector3 rayStart = new Vector3(dropPosition.x, transform.position.y + 2f, dropPosition.z);
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRaycastDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            // Drop at ground level with sheep collider offset
            dropPosition.y = hit.point.y + sheepOffset + 0.05f;
            Debug.Log($"Drop raycast hit ground at Y={hit.point.y}, sheep offset={sheepOffset:F2}");
        }
        else
        {
            // Fallback: raycast from player position straight down to find our current ground
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit playerGroundHit, 5f, groundLayer, QueryTriggerInteraction.Ignore))
            {
                dropPosition.y = playerGroundHit.point.y + sheepOffset + 0.05f;
                Debug.LogWarning($"Drop position raycast failed, using player's ground level Y={playerGroundHit.point.y}");
            }
            else
            {
                // Last resort: use player's feet position
                dropPosition.y = transform.position.y - 1f; // Approximate feet position
                Debug.LogWarning("No ground found anywhere, using player's feet approximation");
            }
        }

        // Store the type before dropping (needed for switch)
        CarriedItemType droppedType = currentItemType;

        // IMPORTANT: Clear carrying state BEFORE calling drop methods
        // This prevents race conditions where re-enabled colliders trigger checks
        // while the player is still technically "carrying" the item
        currentItemType = CarriedItemType.None;

        switch (droppedType)
        {
            case CarriedItemType.Sheep:
                DropSheep(dropPosition);
                break;

            case CarriedItemType.Lettuce:
                currentLettuce.OnDropped(dropPosition);
                currentLettuce = null;
                break;
        }
    }

    private void DropSheep(Vector3 dropPosition)
    {
        if (currentSheep == null) return;

        // Drop the sheep
        currentSheep.OnDropped(dropPosition);

        // Notify player controller
        if (playerController != null)
        {
            playerController.SetCarryingState(false);
        }

        currentSheep = null;
    }

    // Public getters for other scripts to check carrying state
    public bool IsCarryingAnything => currentItemType != CarriedItemType.None;
    public bool IsCarryingSheep => currentItemType == CarriedItemType.Sheep;

    // Called by CheckpointManager to forcefully clear carrying state when loading checkpoint
    public void ForceDropCarriedItem()
    {
        if (currentItemType == CarriedItemType.None) return;

        // CRITICAL: Properly drop sheep using its OnDropped method
        // This ensures isBeingCarried is set to false, otherwise sheep will float
        if (currentSheep != null)
        {
            currentSheep.OnDropped(currentSheep.transform.position);
            currentSheep = null;
        }

        // Clear lettuce reference
        if (currentLettuce != null)
        {
            currentLettuce.transform.SetParent(null);

            Collider lettuceCollider = currentLettuce.GetComponent<Collider>();
            if (lettuceCollider != null)
            {
                lettuceCollider.enabled = true;
            }

            currentLettuce = null;
        }

        // Clear item type
        currentItemType = CarriedItemType.None;

        // Notify player controller
        if (playerController != null)
        {
            playerController.SetCarryingState(false);
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw pickup range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        // Draw carry position
        if (carryPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(carryPosition.position, 0.3f);
        }

        // Highlight nearest sheep
        if (nearestSheep != null && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, nearestSheep.transform.position);
        }
    }
}
