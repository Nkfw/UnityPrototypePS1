using UnityEngine;

public class GuardStationary : MonoBehaviour
{
    [SerializeField] private GuardChase chaseState;
    [SerializeField] private Transform head;
    [SerializeField] private Transform player; // Reference to player for head tracking

    enum GuardState
    {
        Guarding,
        Alert,
        Chasing
    }
    [SerializeField] private GuardState currentGuardState = GuardState.Guarding;

    [Header("Head Rotation Settings")]
    [SerializeField] private float minAngle = -10f;
    [SerializeField] private float maxAngle = 20f;
    [SerializeField] private float headRotationSpeed = 10f;
    [SerializeField] private float headTrackingSpeed = 5f; // Speed for smooth head tracking during chase

    [Header("Alert Settings")]
    [SerializeField] private float alertDelay = 0.5f; // Time before transitioning from Alert to Chasing

    // Internal state
    private int headDirection = 1;
    private float currentAngle = 0f;

    private void Awake()
    {
        chaseState = GetComponent<GuardChase>();

        if (chaseState == null)
        {
            Debug.LogError("GuardStationary: GuardChase component not found!", this);
        }

        if (head == null)
        {
            Debug.LogError("GuardStationary: Head Transform not assigned!", this);
        }

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("GuardStationary: Player not found! Make sure player has 'Player' tag.", this);
            }
        }
    }

    private void Update()
    {
        switch (currentGuardState)
        {
            case GuardState.Guarding:
                RotateHead();
                break;

            case GuardState.Alert:
                // Look at player during alert
                LookAtPlayer();
                break;

            case GuardState.Chasing:
                // Keep looking at player during chase
                LookAtPlayer();
                break;
        }
    }

    private void RotateHead()
    {
        currentAngle += (headRotationSpeed * headDirection * Time.deltaTime);

        // Reverse direction when hitting bounds
        if (currentAngle >= maxAngle)
        {
            currentAngle = maxAngle; // Clamp to max
            headDirection = -1;
        }

        if (currentAngle <= minAngle)
        {
            currentAngle = minAngle; // Clamp to min
            headDirection = 1;
        }

        head.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }

    private void LookAtPlayer()
    {
        if (player == null || head == null) return;

        // Calculate direction from head to player (only on horizontal plane)
        Vector3 directionToPlayer = player.position - head.position;
        directionToPlayer.y = 0f; // Ignore vertical difference

        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

        // Convert to local space relative to guard body
        Quaternion localTargetRotation = Quaternion.Inverse(transform.rotation) * targetRotation;

        // Smoothly rotate head toward player
        head.localRotation = Quaternion.Slerp(
            head.localRotation,
            localTargetRotation,
            headTrackingSpeed * Time.deltaTime
        );
    }

    // ===== PUBLIC METHODS - Called by GuardVisionCone =====

    public void OnPlayerSpotted()
    {
        // Only transition from Guarding state
        if (currentGuardState == GuardState.Guarding)
        {
            currentGuardState = GuardState.Alert;
            Debug.Log("Guard: Player spotted! Entering Alert state...");

            // After alert delay, start chasing
            Invoke(nameof(StartChasing), alertDelay);
        }
    }

    public void OnPlayerLost()
    {
        if (currentGuardState == GuardState.Alert)
        {
            // Cancel delayed chase if player lost during alert
            CancelInvoke(nameof(StartChasing));
            currentGuardState = GuardState.Guarding;
            Debug.Log("Guard: Player lost during alert. Returning to Guarding state.");
        }
        else if (currentGuardState == GuardState.Chasing)
        {
            // Player escaped during active chase
            StopChasing();
            Debug.Log("Guard: Lost sight of player during chase. Stopping pursuit.");
        }
    }

    private void StartChasing()
    {
        currentGuardState = GuardState.Chasing;
        chaseState.StartChase(null); // Pass null since GuardChase already finds player
        Debug.Log("Guard: Starting chase!");
    }

    public void StopChasing()
    {
        // Cancel any pending delayed StartChasing() calls
        CancelInvoke(nameof(StartChasing));

        currentGuardState = GuardState.Guarding;
        chaseState.StopChase();
        Debug.Log("Guard: Stopped chasing. Returning to Guarding state.");
    }

    // Helper method to check current state (useful for other systems)
    public bool IsChasing()
    {
        return currentGuardState == GuardState.Chasing;
    }

    public bool IsAlert()
    {
        return currentGuardState == GuardState.Alert;
    }
}
