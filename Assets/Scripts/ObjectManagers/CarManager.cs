using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [SerializeField] private GameObject body, spoiler, blimp;
    [SerializeField] private GameObject blimpPrefab;

    [SerializeField] private Transform wheelsParent;

    [SerializeField] private GameObject frontLeftWheelPrefab, frontRightWheelPrefab;

    private readonly float blimpYpos = 300f;

    public float maxSpeed;

    public float speed;

    public void Init(Team team)
    {
        body.GetComponent<MeshRenderer>().material.mainTexture = team.carTexture;

        blimp = Instantiate(blimpPrefab, new Vector3(transform.position.x, blimpYpos, transform.position.z), Quaternion.Euler(90f, transform.eulerAngles.y, 0f));
        blimp.GetComponent<MeshRenderer>().material.color = team.color;
    }

    public void SpawnWheels(Controller car)
    {
        var frontLeftWheel = Instantiate(frontLeftWheelPrefab, wheelsParent);
        frontLeftWheel.GetComponent<NetworkObject>().Spawn();
        car.frontLeftWheelTransform = frontLeftWheel.transform;

        var frontRightWheel = Instantiate(frontRightWheelPrefab, wheelsParent);
        frontRightWheel.GetComponent<NetworkObject>().Spawn();
        car.frontRightWheelTransform = frontRightWheel.transform;
    }

    void LateUpdate()
    {
        if(blimp != null){
            blimp.transform.SetPositionAndRotation(
                new Vector3(transform.position.x, blimpYpos, transform.position.z), 
                Quaternion.Euler(90f, transform.eulerAngles.y, 0f)
            );
        }
    }
}
