using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [SerializeField] private GameObject body, spoiler, blimp;
    [SerializeField] private GameObject blimpPrefab;

    [SerializeField] private Transform wheelsParent;

    [SerializeField] private GameObject frontLeftWheelPrefab, frontRightWheelPrefab;

    private GameObject fakeCar, fakeFrontLeftWheel, fakeFrontRightWheel;
    private GameObject realFrontLeftWheel, realFrontRightWheel;

    // Magic valur, seems to work
    private readonly float lerpSpeed = 10f;

    private readonly float blimpYpos = 300f;

    public float maxSpeed;

    public float speed;

    public void Init(Team team)
    {
        body.GetComponent<MeshRenderer>().material.mainTexture = team.carTexture;

        blimp = Instantiate(blimpPrefab, new Vector3(transform.position.x, blimpYpos, transform.position.z), Quaternion.Euler(90f, transform.eulerAngles.y, 0f));
        blimp.GetComponent<MeshRenderer>().material.color = team.color;
    }

    public void FakeCar()
    {
        Debug.Log("Fake car");
        // realFrontLeftWheel = GetComponent<Controller>().frontLeftWheelTransform.gameObject;
        // fakeFrontLeftWheel = Instantiate(realFrontLeftWheel,
        //      realFrontLeftWheel.transform.position, realFrontLeftWheel.transform.rotation);
        // realFrontLeftWheel.GetComponent<MeshRenderer>().enabled = false;

        // realFrontRightWheel = GetComponent<Controller>().frontRightWheelTransform.gameObject;
        // fakeFrontLeftWheel = Instantiate(realFrontRightWheel,
        //      realFrontRightWheel.transform.position, realFrontRightWheel.transform.rotation);
        // realFrontRightWheel.GetComponent<MeshRenderer>().enabled = false;

        Destroy(this.GetComponent<Controller>());
        Destroy(this.GetComponentInChildren<WheelsScript>());
        fakeCar = Instantiate(gameObject, transform.position, transform.rotation);
        Destroy(body);
        Destroy(spoiler);
        // Destroy(wheelsParent.gameObject);
        Destroy(fakeCar.GetComponent<NetworkRigidbody>());
        Destroy(fakeCar.GetComponent<NetworkTransform>());
        Destroy(fakeCar.GetComponent<NetworkObject>());
        Destroy(fakeCar.GetComponent<CarManager>());
        Destroy(fakeCar.GetComponent<Controller>());
    }

    public void SpawnWheels(Controller car)
    {
        Debug.Log("SpawnCar");
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

    void FixedUpdate()
    {
        if(fakeCar != null){
            fakeCar.transform.SetPositionAndRotation(
                Vector3.Lerp(fakeCar.transform.position, transform.position, lerpSpeed*Time.deltaTime),
                transform.rotation
            );

            // fakeFrontLeftWheel.transform.SetPositionAndRotation(
            //     Vector3.Lerp(fakeFrontLeftWheel.transform.position, realFrontLeftWheel.transform.position, lerpSpeed*Time.deltaTime),
            //     realFrontLeftWheel.transform.rotation
            // );

            // fakeFrontRightWheel.transform.SetPositionAndRotation(
            //     Vector3.Lerp(fakeFrontRightWheel.transform.position, realFrontRightWheel.transform.position, lerpSpeed*Time.deltaTime),
            //     realFrontRightWheel.transform.rotation
            // );
        }
    }
}
