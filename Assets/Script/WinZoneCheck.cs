using UnityEngine;

// This script goes on the Win Zone GameObject
// Detects when sheep enters the zone (either carried by player or dropped/teleported)
public class WinZoneCheck : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {

        // Sheep enters the zone directly
        if (other.CompareTag("Sheep"))
        {
            Debug.Log("You Win! Sheep entered the goal zone!");
            OnWin();
            return;
        }
    }

    private void OnWin()
    {
        // You can add more win logic here later:
        // - Show win UI
        // - Load next level
        // - Play victory sound
        // - Freeze player movement

        Debug.Log("=== LEVEL COMPLETE ===");
    }
}
