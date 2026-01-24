using UnityEngine;

// Attach this to the Following Zone GameObject (the one with Box Collider)
// This script detects when sheep enter/exit the territory
public class TerritoryZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if a sheep entered the zone
        if (other.CompareTag("Sheep"))
        {
            SheepFollowingZone sheepZone = other.GetComponent<SheepFollowingZone>();
            if (sheepZone != null)
            {
                sheepZone.OnEnteredTerritory();
                Debug.Log($"Sheep {other.name} entered territory zone");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if a sheep left the zone
        if (other.CompareTag("Sheep"))
        {
            SheepFollowingZone sheepZone = other.GetComponent<SheepFollowingZone>();
            if (sheepZone != null)
            {
                sheepZone.OnExitedTerritory();
                Debug.Log($"Sheep {other.name} exited territory zone - ESCAPED!");
            }
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange transparent
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
}
