using System.Collections.Generic;
using UnityEngine;

// Attach this to a GameObject in the scene (suggest: Create empty GameObject called "CheckpointManager")
// Manages saving and loading game state at checkpoints
// Stores player position, sheep positions, and what the player is carrying
public class CheckpointManager : MonoBehaviour
{
    // Singleton pattern - only one CheckpointManager exists in the scene
    public static CheckpointManager Instance;

    [Header("References - Assign in Inspector")]
    [SerializeField] private GameObject player; // The player GameObject
    [SerializeField] private List<GameObject> allSheep = new List<GameObject>(); // All sheep in the level
    [SerializeField] private List<GameObject> bridges = new List<GameObject>(); // Bridges (BridgeCollapse components) - respawn on death
    [SerializeField] private List<GameObject> guards = new List<GameObject>(); // Guards that need position reset on death

    [Header("Debug - Read Only")]
    [SerializeField] private bool hasCheckpointData = false; // Do we have saved data?
    [SerializeField] private Vector3 savedPlayerPosition; // Last saved player position
    [SerializeField] private int sheepCount = 0; // How many sheep we're tracking

    [Header("Debug Settings")]
    [SerializeField] private bool showDebugLogs = true; // Show checkpoint messages in console

    // Data structure to store checkpoint information
    // This holds everything we need to restore the game state
    private class CheckpointData
    {
        public Checkpoint checkpoint; // Reference to the checkpoint (for spawn position)
        public Vector3 playerPosition; // Where the player was standing (fallback if no checkpoint)
        public Quaternion playerRotation; // Which way the player was facing
        public Dictionary<Sheep, SheepData> sheepStates; // State of each sheep
        public List<GameObject> activeBridges; // Bridges that were active when checkpoint saved
        public Dictionary<GameObject, GuardData> guardStates; // State of each guard

        // Constructor - initializes the collections
        public CheckpointData()
        {
            sheepStates = new Dictionary<Sheep, SheepData>();
            activeBridges = new List<GameObject>();
            guardStates = new Dictionary<GameObject, GuardData>();
        }
    }

    // Data structure to store individual sheep state
    private class SheepData
    {
        public Vector3 position; // Where the sheep was
        public Quaternion rotation; // Which way the sheep was facing
        public bool wasBeingCarried; // Was the player carrying this sheep?
    }

    // Data structure to store individual guard state
    private class GuardData
    {
        public Vector3 position; // Where the guard was
        public Quaternion rotation; // Which way the guard was facing
    }

    // The current saved checkpoint data
    private CheckpointData currentCheckpoint;

    // Store INITIAL guard positions (never changes after level start)
    private Dictionary<GameObject, GuardData> initialGuardPositions = new Dictionary<GameObject, GuardData>();

    private void Awake()
    {
        // Singleton setup - ensure only one CheckpointManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // If another CheckpointManager already exists, destroy this one
            Destroy(gameObject);
            return;
        }

        // Initialize checkpoint data
        currentCheckpoint = new CheckpointData();
    }

    private void Start()
    {
        // Automatically find all sheep in the scene if not manually assigned
        if (allSheep.Count == 0)
        {
            GameObject[] foundSheep = GameObject.FindGameObjectsWithTag("Sheep");
            foreach (GameObject sheepObj in foundSheep)
            {
                allSheep.Add(sheepObj);
            }

            if (showDebugLogs == true)
            {
                Debug.Log($"CheckpointManager: Automatically found {allSheep.Count} sheep in scene");
            }
        }

        // Update debug info
        sheepCount = allSheep.Count;

        // Save initial checkpoint at level start position
        SaveInitialCheckpoint();
    }

    // Saves the very first checkpoint when the level starts
    // This ensures player always has somewhere to respawn to
    private void SaveInitialCheckpoint()
    {
        if (player == null)
        {
            Debug.LogError("CheckpointManager: Player reference is missing! Assign it in Inspector.");
            return;
        }

        // No checkpoint reference for initial save (uses player position)
        currentCheckpoint.checkpoint = null;

        // Save player position
        currentCheckpoint.playerPosition = player.transform.position;
        currentCheckpoint.playerRotation = player.transform.rotation;

        // Save all sheep positions
        currentCheckpoint.sheepStates.Clear();
        foreach (GameObject sheepObj in allSheep)
        {
            if (sheepObj != null)
            {
                Sheep sheepComponent = sheepObj.GetComponent<Sheep>();
                if (sheepComponent != null)
                {
                    SheepData data = new SheepData();
                    data.position = sheepObj.transform.position;
                    data.rotation = sheepObj.transform.rotation;
                    data.wasBeingCarried = false; // Not being carried at start

                    currentCheckpoint.sheepStates[sheepComponent] = data;
                }
            }
        }

        // Save which bridges are active at start (all should be active)
        currentCheckpoint.activeBridges.Clear();
        foreach (GameObject bridge in bridges)
        {
            if (bridge != null && bridge.activeInHierarchy == true)
            {
                currentCheckpoint.activeBridges.Add(bridge);
            }
        }

        // Save all guard INITIAL positions (these never change)
        // Store in both initialGuardPositions (permanent) and checkpoint (for consistency)
        currentCheckpoint.guardStates.Clear();
        initialGuardPositions.Clear();
        foreach (GameObject guard in guards)
        {
            if (guard != null)
            {
                GuardData data = new GuardData();
                data.position = guard.transform.position;
                data.rotation = guard.transform.rotation;

                // Store in permanent initial positions dictionary
                initialGuardPositions[guard] = data;

                // Also store in checkpoint (for consistency)
                currentCheckpoint.guardStates[guard] = data;

                if (showDebugLogs == true)
                {
                    Debug.Log($"CheckpointManager: Saved INITIAL guard position for '{guard.name}': {data.position}");
                }
            }
        }

        hasCheckpointData = true;
        savedPlayerPosition = currentCheckpoint.playerPosition;

        if (showDebugLogs == true)
        {
            Debug.Log($"CheckpointManager: Initial checkpoint saved at {savedPlayerPosition}, {currentCheckpoint.guardStates.Count} guards, {currentCheckpoint.activeBridges.Count} bridges active");
        }
    }

    // Called by Checkpoint.cs when player enters a checkpoint trigger
    // Saves the current game state so we can restore it later
    public void SaveCheckpoint(Checkpoint checkpoint)
    {
        if (player == null)
        {
            Debug.LogError("CheckpointManager: Cannot save checkpoint - Player reference is missing!");
            return;
        }

        // Save checkpoint reference (for spawn position)
        currentCheckpoint.checkpoint = checkpoint;

        // Save player position and rotation (fallback if checkpoint doesn't have spawn point)
        currentCheckpoint.playerPosition = player.transform.position;
        currentCheckpoint.playerRotation = player.transform.rotation;

        // Save all sheep positions and states (even if being carried)
        currentCheckpoint.sheepStates.Clear();
        foreach (GameObject sheepObj in allSheep)
        {
            if (sheepObj != null)
            {
                Sheep sheepComponent = sheepObj.GetComponent<Sheep>();
                if (sheepComponent != null)
                {
                    SheepData data = new SheepData();
                    data.position = sheepObj.transform.position;
                    data.rotation = sheepObj.transform.rotation;
                    data.wasBeingCarried = sheepComponent.IsBeingCarried;

                    currentCheckpoint.sheepStates[sheepComponent] = data;
                }
            }
        }

        // Save which bridges are currently active
        currentCheckpoint.activeBridges.Clear();
        foreach (GameObject bridge in bridges)
        {
            if (bridge != null && bridge.activeInHierarchy == true)
            {
                currentCheckpoint.activeBridges.Add(bridge);
            }
        }

        // NOTE: We do NOT save guard positions here!
        // Guards always restore to their INITIAL spawn positions (stored in initialGuardPositions)
        // This prevents guards from "camping" checkpoints if player activates checkpoint during chase

        hasCheckpointData = true;
        savedPlayerPosition = currentCheckpoint.playerPosition;

        if (showDebugLogs == true)
        {
            Debug.Log($"CheckpointManager: Checkpoint '{checkpoint.name}' saved! Sheep tracked: {currentCheckpoint.sheepStates.Count}, Active bridges: {currentCheckpoint.activeBridges.Count}, Guards: {currentCheckpoint.guardStates.Count}");
        }
    }

    // Called by DeathManager.cs when player or sheep dies
    // Restores the game to the last saved checkpoint
    public void LoadLastCheckpoint()
    {
        // Check if we have checkpoint data to load
        if (hasCheckpointData == false)
        {
            Debug.LogError("CheckpointManager: No checkpoint data to load! Did you save a checkpoint first?");
            return;
        }

        if (player == null)
        {
            Debug.LogError("CheckpointManager: Cannot load checkpoint - Player reference is missing!");
            return;
        }

        // Restore player position and rotation
        // Use checkpoint spawn point if available, otherwise use saved position
        Vector3 spawnPosition;
        if (currentCheckpoint.checkpoint != null)
        {
            spawnPosition = currentCheckpoint.checkpoint.GetSpawnPosition();
            if (showDebugLogs == true)
            {
                Debug.Log($"CheckpointManager: Using checkpoint '{currentCheckpoint.checkpoint.name}' spawn position: {spawnPosition}");
            }
        }
        else
        {
            spawnPosition = currentCheckpoint.playerPosition;
            if (showDebugLogs == true)
            {
                Debug.Log($"CheckpointManager: No checkpoint reference, using saved position: {spawnPosition}");
            }
        }

        player.transform.position = spawnPosition;
        player.transform.rotation = currentCheckpoint.playerRotation;

        // CRITICAL: If player was carrying something when they died, we need to forcefully
        // clear the carrying state BEFORE restoring sheep positions
        // This prevents the bridge from thinking the player is still carrying sheep
        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
        if (playerInteraction != null)
        {
            if (playerInteraction.IsCarryingAnything == true)
            {
                if (showDebugLogs == true)
                {
                    Debug.Log($"CheckpointManager: Player was carrying something (IsCarryingSheep={playerInteraction.IsCarryingSheep}), forcing drop...");
                }

                // Force drop all carried items
                playerInteraction.ForceDropCarriedItem();

                if (showDebugLogs == true)
                {
                    Debug.Log($"CheckpointManager: After ForceDropCarriedItem - IsCarryingSheep={playerInteraction.IsCarryingSheep}, IsCarryingAnything={playerInteraction.IsCarryingAnything}");
                }
            }
        }

        // Restore all sheep positions and states
        if (showDebugLogs == true)
        {
            Debug.Log($"CheckpointManager: Starting to restore {currentCheckpoint.sheepStates.Count} sheep...");
        }

        foreach (KeyValuePair<Sheep, SheepData> entry in currentCheckpoint.sheepStates)
        {
            Sheep sheepComponent = entry.Key;
            SheepData data = entry.Value;

            if (sheepComponent != null)
            {
                if (showDebugLogs == true)
                {
                    Debug.Log($"CheckpointManager: Restoring sheep '{sheepComponent.name}' from {sheepComponent.transform.position} to {data.position}");
                }

                // Reset Rigidbody velocity so sheep doesn't continue falling after teleport
                Rigidbody sheepRigidbody = sheepComponent.GetComponent<Rigidbody>();
                if (sheepRigidbody != null)
                {
                    sheepRigidbody.linearVelocity = Vector3.zero;
                    sheepRigidbody.angularVelocity = Vector3.zero;

                    // Ensure Rigidbody is kinematic (sheep uses MovePosition, not physics)
                    if (sheepRigidbody.isKinematic == false)
                    {
                        sheepRigidbody.isKinematic = true;

                        if (showDebugLogs == true)
                        {
                            Debug.Log($"CheckpointManager: Set sheep '{sheepComponent.name}' Rigidbody to kinematic");
                        }
                    }

                    if (showDebugLogs == true)
                    {
                        Debug.Log($"CheckpointManager: Reset velocity for sheep '{sheepComponent.name}'");
                    }
                }
                else
                {
                    Debug.LogWarning($"CheckpointManager: Sheep '{sheepComponent.name}' has no Rigidbody!");
                }

                // Restore sheep position
                // If we have a checkpoint with spawn point, spawn sheep near it from above
                // Otherwise, restore to saved position
                Vector3 sheepSpawnPos;
                if (currentCheckpoint.checkpoint != null)
                {
                    // Spawn sheep offset from checkpoint spawn point (scattered around)
                    Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
                    sheepSpawnPos = currentCheckpoint.checkpoint.GetSpawnPosition() + offset;
                }
                else
                {
                    sheepSpawnPos = data.position;
                }

                sheepComponent.transform.position = sheepSpawnPos;
                sheepComponent.transform.rotation = data.rotation;

                if (showDebugLogs == true)
                {
                    Debug.Log($"CheckpointManager: Set sheep position to {sheepSpawnPos}");
                }

                // Make sure sheep GameObject is active
                if (sheepComponent.gameObject.activeSelf == false)
                {
                    sheepComponent.gameObject.SetActive(true);

                    if (showDebugLogs == true)
                    {
                        Debug.Log($"CheckpointManager: Reactivated sheep GameObject '{sheepComponent.name}'");
                    }
                }

                // Reset sheep state machine to Idle so it can detect attractions
                SheepAttraction sheepAttraction = sheepComponent.GetComponent<SheepAttraction>();
                if (sheepAttraction != null)
                {
                    // Make sure SheepAttraction component is enabled
                    if (sheepAttraction.enabled == false)
                    {
                        sheepAttraction.enabled = true;

                        if (showDebugLogs == true)
                        {
                            Debug.Log($"CheckpointManager: Re-enabled SheepAttraction for '{sheepComponent.name}'");
                        }
                    }

                    sheepAttraction.ResetToIdleState();
                }
                else
                {
                    Debug.LogWarning($"CheckpointManager: Sheep '{sheepComponent.name}' has no SheepAttraction component!");
                }
            }
            else
            {
                Debug.LogWarning("CheckpointManager: Found null sheep component in saved data!");
            }
        }

        // Restore bridges to their checkpoint state
        // Bridges always respawn on death (prevents soft-locking)
        foreach (GameObject bridge in bridges)
        {
            if (bridge != null)
            {
                bool shouldBeActive = currentCheckpoint.activeBridges.Contains(bridge);
                if (bridge.activeSelf != shouldBeActive)
                {
                    bridge.SetActive(shouldBeActive);
                    if (showDebugLogs == true)
                    {
                        Debug.Log($"CheckpointManager: Bridge '{bridge.name}' set active = {shouldBeActive}");
                    }
                }
            }
        }

        // Restore all guards to their INITIAL spawn positions
        // We always use initialGuardPositions (saved at level start), NOT checkpoint positions
        foreach (KeyValuePair<GameObject, GuardData> entry in initialGuardPositions)
        {
            GameObject guard = entry.Key;
            GuardData data = entry.Value;

            if (guard != null)
            {
                // CRITICAL: Disable CharacterController before moving guard
                // CharacterController has internal state that interferes with position changes
                CharacterController controller = guard.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }

                // Restore to INITIAL position and rotation
                guard.transform.position = data.position;
                guard.transform.rotation = data.rotation;

                // Re-enable CharacterController AFTER position change
                if (controller != null)
                {
                    controller.enabled = true;
                }

                if (showDebugLogs == true)
                {
                    Debug.Log($"CheckpointManager: Restored guard '{guard.name}' to INITIAL position {data.position}");
                }

                // Reset guard state machine to Guarding state
                GuardStationary guardStationary = guard.GetComponent<GuardStationary>();
                if (guardStationary != null)
                {
                    guardStationary.StopChasing();

                    if (showDebugLogs == true)
                    {
                        Debug.Log($"CheckpointManager: Reset guard '{guard.name}' to Guarding state");
                    }
                }

                // Ensure chase is stopped
                GuardChase guardChase = guard.GetComponent<GuardChase>();
                if (guardChase != null)
                {
                    guardChase.StopChase();
                }
            }
            else
            {
                Debug.LogWarning("CheckpointManager: Found null guard in initialGuardPositions!");
            }
        }

        if (showDebugLogs == true)
        {
            Debug.Log($"CheckpointManager: Checkpoint loaded! Player spawned at checkpoint, {currentCheckpoint.sheepStates.Count} sheep restored, {currentCheckpoint.activeBridges.Count} bridges restored, {initialGuardPositions.Count} guards restored to INITIAL positions");
        }
    }
}
