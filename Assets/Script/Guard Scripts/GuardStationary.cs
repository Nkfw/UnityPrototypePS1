using UnityEngine;

public class GuardStationary : MonoBehaviour
{
    [SerializeField] private GuardChase chaseState;
    [SerializeField] private Transform head;

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
    }

    private void Update()
    {
        switch (currentGuardState)
        {
            case GuardState.Guarding:
                RotateHead();
                break;

            case GuardState.Alert:
                // Stop head rotation during alert
                // TODO: Play alert animation, look toward player
                break;

            case GuardState.Chasing:
                // Stop head rotation during chase
                // Chase is handled by GuardChase component
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
