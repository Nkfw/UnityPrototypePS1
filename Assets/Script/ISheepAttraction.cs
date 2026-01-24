using UnityEngine;

/// <summary>
/// Interface for all objects that can attract sheep
/// Implement this on any GameObject that should attract sheep (lettuce, buttons, flowers, smells, etc.)
/// </summary>
public interface ISheepAttraction
{
    /// <summary>
    /// Get the world position of this attraction
    /// </summary>
    Vector3 GetPosition();

    /// <summary>
    /// Check if this attraction is currently available for sheep to interact with
    /// (e.g., not being carried by player, not already used, etc.)
    /// </summary>
    bool IsAvailable();

    /// <summary>
    /// Get the priority of this attraction (higher = more attractive)
    /// Default = 1.0, can be used for gameplay tuning
    /// </summary>
    float GetPriority();

    /// <summary>
    /// Called when the sheep reaches and interacts with this attraction
    /// Use this for custom interaction logic (play sounds, trigger effects, etc.)
    /// </summary>
    void OnSheepInteract(Sheep sheep);

    /// <summary>
    /// Should this attraction be destroyed after the sheep interacts with it?
    /// Lettuce: true (gets eaten)
    /// Button: false (stays in level)
    /// Flower: false (sheep just sniffs it)
    /// </summary>
    bool ShouldDestroyAfterInteraction();

    /// <summary>
    /// Get the GameObject this attraction is attached to
    /// Used for destruction and reference tracking
    /// </summary>
    GameObject GetGameObject();
}
