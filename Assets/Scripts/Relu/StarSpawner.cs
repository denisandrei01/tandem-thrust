using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private float distanceBetweenStars = 2.0f;
    [SerializeField] private int numberOfStars = 10;

    [SerializeField] private List<Transform> waypoints;

    public void SpawnStars()
    {
        foreach (Transform waypoint in waypoints)
        {
            float yPosition = waypoint.position.y; // Get the Y-axis position of the waypoint

            for (int i = 0; i < numberOfStars; i++)
            {
                // Randomize the X and Z positions while keeping the Y position the same
                Vector3 spawnPosition = new(
                    waypoint.position.x + Random.Range(-distanceBetweenStars, distanceBetweenStars),
                    yPosition,
                    waypoint.position.z + Random.Range(-distanceBetweenStars, distanceBetweenStars)
                );

                var star = Instantiate(starPrefab, spawnPosition, Quaternion.identity);
                star.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
