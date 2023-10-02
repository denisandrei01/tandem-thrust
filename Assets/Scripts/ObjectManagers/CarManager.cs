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
    private readonly float lerpSpeed = 100f, wheelsLerpSpeed = 100f;

    private readonly float blimpYpos = 300f;

    public float maxSpeed;

    public float speed;

    public void Init(Team team)
    {
        body.GetComponent<MeshRenderer>().material.mainTexture = team.carTexture;

        blimp = Instantiate(blimpPrefab, new Vector3(transform.position.x, blimpYpos, transform.position.z), Quaternion.Euler(90f, transform.eulerAngles.y, 0f));
        blimp.GetComponent<MeshRenderer>().material.color = team.color;
    }

    public IEnumerator FakeCar()
    {
        Destroy(this.GetComponent<Controller>());
        Destroy(this.GetComponentInChildren<WheelsScript>());
        fakeCar = Instantiate(gameObject, transform.position, transform.rotation);
        Destroy(body);
        Destroy(spoiler);
        Destroy(wheelsParent.parent.gameObject);
        Destroy(fakeCar.GetComponent<NetworkRigidbody>());
        Destroy(fakeCar.GetComponent<NetworkTransform>());
        Destroy(fakeCar.GetComponent<NetworkObject>());
        Destroy(fakeCar.GetComponent<CarManager>());
        Destroy(fakeCar.GetComponent<Controller>());
    
        realFrontLeftWheel = GameObject.Find("FrontLeftWheel(Clone)");
        realFrontRightWheel = GameObject.Find("FrontRightWheel(Clone)");

        while(realFrontLeftWheel == null || realFrontRightWheel == null){
            //Wait for the wheels to spawn
            yield return new WaitForSeconds(0.1f);
            realFrontLeftWheel = GameObject.Find("FrontLeftWheel(Clone)");
            realFrontRightWheel = GameObject.Find("FrontRightWheel(Clone)");
        }

        fakeFrontLeftWheel = Instantiate(realFrontLeftWheel,
             realFrontLeftWheel.transform.position, realFrontLeftWheel.transform.rotation);
        realFrontLeftWheel.GetComponent<MeshRenderer>().enabled = false;

        fakeFrontRightWheel = Instantiate(realFrontRightWheel,
             realFrontRightWheel.transform.position, realFrontRightWheel.transform.rotation);
        realFrontRightWheel.GetComponent<MeshRenderer>().enabled = false;
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
        if(fakeCar != null && fakeFrontLeftWheel != null && fakeFrontRightWheel != null){
            fakeCar.transform.SetPositionAndRotation(
                Vector3.Lerp(fakeCar.transform.position, transform.position, lerpSpeed*Time.deltaTime),
                transform.rotation
            );

            fakeFrontLeftWheel.transform.SetPositionAndRotation(
                Vector3.Lerp(fakeFrontLeftWheel.transform.position, realFrontLeftWheel.transform.position, wheelsLerpSpeed*Time.deltaTime),
                Quaternion.Lerp(fakeFrontLeftWheel.transform.rotation, realFrontLeftWheel.transform.rotation, wheelsLerpSpeed*Time.deltaTime)
            );

            fakeFrontRightWheel.transform.SetPositionAndRotation(
                Vector3.Lerp(fakeFrontRightWheel.transform.position, realFrontRightWheel.transform.position, wheelsLerpSpeed*Time.deltaTime),
                Quaternion.Lerp(fakeFrontRightWheel.transform.rotation, realFrontRightWheel.transform.rotation, wheelsLerpSpeed*Time.deltaTime)
            );
        }
    }
}
