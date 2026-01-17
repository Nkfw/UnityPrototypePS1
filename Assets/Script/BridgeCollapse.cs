using UnityEngine;

public class BridgeCollapse : MonoBehaviour
{
    [Header("Debug - Read Only")]
    [SerializeField] private bool playerOnBridge;
    [SerializeField] private bool sheepOnBridge; // Note: Will be false if sheep is carried

    private PlayerInteraction playerReference;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerReference = other.GetComponent<PlayerInteraction>();
            playerOnBridge = true;
            CheckCollapse(); // Check immediately after setting reference
        }

        if (other.CompareTag("Sheep"))
        {
            sheepOnBridge = true;
            CheckCollapse();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerReference = null;
            playerOnBridge = false;
        }

        if (other.CompareTag("Sheep"))
        {
            sheepOnBridge = false;
        }
    }

    private void CheckCollapse()
    {
        // Early exit if no player reference
        if (playerReference == null)
            return;

        // DEBUG: Log the current state
        Debug.Log($"Bridge Check: playerOnBridge={playerOnBridge}, sheepOnBridge={sheepOnBridge}, IsCarryingSheep={playerReference.IsCarryingSheep}");

        // Bridge collapses if player is carrying sheep
        if (playerReference.IsCarryingSheep && playerOnBridge)
        {
            Debug.Log("Bridge collapsed! (Player carrying sheep)");
            Destroy(gameObject);
            return;
        }

        // Bridge also collapses if player and separate sheep are both on bridge
        if (playerOnBridge && sheepOnBridge)
        {
            Debug.Log("Bridge collapsed! (Player and sheep both on bridge)");
            Destroy(gameObject);
        }
    }
}
