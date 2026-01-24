using UnityEngine;

public class GuardVisionCone : MonoBehaviour
{
    [SerializeField] float viewDistance = 15f;      
    [SerializeField] float viewAngle = 60f;         
    [SerializeField] LayerMask obstacleLayer;       

    [SerializeField] Transform player = null;
    [SerializeField] float playerHeightOffset = 1f; // Check at player's chest height

    GuardStationary guardStationary;
    bool isPlayerSpotted = false;

    private void Awake()
    {
        guardStationary = GetComponentInParent<GuardStationary>();

        if (guardStationary == null)
        {
            Debug.LogError("GuardVisionCone: No GuardStationary found on parent!", this);
        }

        if (player == null)
        {
            var found = GameObject.FindWithTag("Player");
            if (found != null)
            {
                player = found.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null || guardStationary == null)
            return;

        bool wasPlayerSpotted = isPlayerSpotted;
        isPlayerSpotted = PlayerSpotted();

        // Player just entered vision (rising edge detection)
        if (isPlayerSpotted && !wasPlayerSpotted)
        {
            guardStationary.OnPlayerSpotted();
        }
        // Player just left vision (falling edge detection)
        else if (!isPlayerSpotted && wasPlayerSpotted)
        {
            guardStationary.OnPlayerLost();
        }
    }

    bool PlayerSpotted()
    {
        // Check to player's center/chest height instead of feet
        Vector3 playerCenter = player.position + Vector3.up * playerHeightOffset;
        Vector3 distance = (playerCenter - transform.position);

        if (distance.magnitude > viewDistance)
        {
            return false;
        }

        Vector3 directionToPlayer = distance.normalized;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > (viewAngle / 2))
        {
            return false;
        }

        // Raycast only to player's actual distance, not the full viewDistance
        // This prevents hitting obstacles behind the player
        if (Physics.Raycast(transform.position, directionToPlayer, distance.magnitude, obstacleLayer))
        {
            return false; // Obstacle blocking line of sight
        }

        return true; // Player is visible!
    }

    // Debug visualization - shows vision cone in Scene view
    private void OnDrawGizmos()
    {
        // Calculate vision cone boundaries (used in both edit and play mode)
        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;

        // Draw static cone in edit mode
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, leftBoundary);
            Gizmos.DrawRay(transform.position, rightBoundary);

            Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
            Gizmos.DrawWireSphere(transform.position, viewDistance);
            return;
        }

        // Runtime visualization with player tracking
        if (player == null)
            return;

        // Draw line to player (red if spotted, yellow if not)
        Gizmos.color = isPlayerSpotted ? Color.red : Color.yellow;
        Vector3 playerCenter = player.position + Vector3.up * playerHeightOffset;
        Gizmos.DrawLine(transform.position, playerCenter);

        // Draw vision cone boundaries
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);

        // Draw view distance sphere
        Gizmos.color = new Color(1f, 1f, 0f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }

}