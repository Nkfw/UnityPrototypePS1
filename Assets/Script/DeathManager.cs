using System.Collections;
using UnityEngine;

// Attach this to a GameObject in the scene (suggest: Create empty GameObject called "DeathManager")
// Handles all death events in the game (falling, guard catching, explosions, etc.)
// This is the central authority for what happens when player or sheep dies
public class DeathManager : MonoBehaviour
{
    // Singleton pattern - only one DeathManager exists in the scene
    public static DeathManager Instance;

    [Header("References")]
    [SerializeField] private ScreenFader screenFader; // Reference to the fade effect controller
    [SerializeField] private CheckpointManager checkpointManager; // Reference to checkpoint system

    [Header("Settings")]
    [SerializeField] private bool freezePlayerOnDeath = true; // Should we prevent player from moving during death?

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true; // Show death messages in console

    // Different ways the player or sheep can die
    // This helps us track what killed them (useful for statistics or different animations later)
    public enum DeathCause
    {
        PlayerFell,      // Player fell off the map
        SheepFell,       // Sheep fell off the map
        GuardCaught,     // Guard caught the player
        Explosion,       // Future: Bomb exploded
        Trap             // Future: Spike trap, etc.
    }

    private void Awake()
    {
        // Singleton setup - ensure only one DeathManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // If another DeathManager already exists, destroy this one
            Destroy(gameObject);
            return;
        }
    }

    // Called by any death source (KillZone, Guard, Bomb, etc.)
    // This is the main entry point for all deaths in the game
    public void OnDeath(DeathCause cause)
    {
        // Start the death sequence as a coroutine
        // This allows us to wait for fade animations to complete
        StartCoroutine(DeathSequenceCoroutine(cause));
    }

    // Coroutine that handles the full death sequence with proper timing
    // This waits for fade animations to complete before continuing
    private IEnumerator DeathSequenceCoroutine(DeathCause cause)
    {
        // Log what happened (helps with debugging)
        if (showDebugLogs == true)
        {
            Debug.Log($"DeathManager: Death triggered! Cause: {cause}");
        }

        // Step 1: Freeze player input so they can't move during respawn
        if (freezePlayerOnDeath == true)
        {
            FreezePlayerInput();
        }

        // Step 2: Fade screen to black and WAIT for it to complete
        if (screenFader != null)
        {
            screenFader.FadeToBlack();

            // Wait until the fade to black is finished
            while (screenFader.IsFading == true)
            {
                yield return null; // Wait one frame and check again
            }
        }
        else
        {
            Debug.LogWarning("DeathManager: ScreenFader reference is missing! Assign it in Inspector.");
        }

        // Step 3: Reload from last checkpoint (restores player/sheep positions)
        // This happens while screen is fully black (player can't see the teleport)
        if (checkpointManager != null)
        {
            checkpointManager.LoadLastCheckpoint();
        }
        else
        {
            Debug.LogError("DeathManager: CheckpointManager reference is missing! Assign it in Inspector.");
        }

        // Step 4: Fade screen back from black and WAIT for it to complete
        if (screenFader != null)
        {
            screenFader.FadeFromBlack();

            // Wait until the fade from black is finished
            while (screenFader.IsFading == true)
            {
                yield return null; // Wait one frame and check again
            }
        }

        // Step 5: Unfreeze player input so they can play again
        if (freezePlayerOnDeath == true)
        {
            UnfreezePlayerInput();
        }

        // Log completion
        if (showDebugLogs == true)
        {
            Debug.Log("DeathManager: Respawn complete!");
        }
    }

    // Prevents player from moving during death sequence
    private void FreezePlayerInput()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Disable the player controller script so they can't move
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            // Disable the player interaction script so they can't pickup/drop items
            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            if (interaction != null)
            {
                interaction.enabled = false;
            }
        }
    }

    // Allows player to move again after respawn
    private void UnfreezePlayerInput()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Re-enable the player controller script
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = true;
            }

            // Re-enable the player interaction script
            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            if (interaction != null)
            {
                interaction.enabled = true;
            }
        }
    }
}
