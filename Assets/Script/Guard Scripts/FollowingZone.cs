//using Unity.VisualScripting;
//using UnityEngine;

//public class FollowingZone : MonoBehaviour
//{
//    private GuardStationary guard;

//    private void Start()
//    {
//        guard = GetComponent<GuardStationary>();

//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//            guard.OnPlayerEnterChaseZone(other.transform);
//    }

//    void OnTriggerExit(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            guard.OnPlayerExitChaseZone();
//        }
//    }
//}
