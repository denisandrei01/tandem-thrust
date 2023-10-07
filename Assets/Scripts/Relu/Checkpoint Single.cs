using Unity.Netcode;
using UnityEngine;

public class CheckpointSingle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            GameManagers.serverManager.CarThroughCheckpoint(other.GetComponent<NetworkObject>().NetworkObjectId, this);
        }
    }
}
