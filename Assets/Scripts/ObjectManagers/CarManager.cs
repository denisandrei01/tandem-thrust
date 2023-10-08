using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [SerializeField] private GameObject body, spoiler, blimp;
    [SerializeField] private GameObject blimpPrefab;

    [SerializeField] private Transform wheelsParent;

    [SerializeField] private GameObject frontLeftWheelPrefab, frontRightWheelPrefab, rearLeftWheelPrefab, rearRightWheelPrefab;

    private GameObject fakeCar, fakeFrontLeftWheel, fakeFrontRightWheel, fakeRearLeftWheel, fakeRearRightWheel;
    private GameObject realFrontLeftWheel, realFrontRightWheel, realRearLeftWheel, realRearRightWheel;

    // Magic valur, seems to work
    private readonly float carLerpSpeed = 15f, wheelsLerpSpeed = 60f;

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
    
        FindRealWheels();

        // while(realFrontLeftWheel == null || realFrontRightWheel == null || realRearLeftWheel == null || realRearRightWheel == null){
        //     //Wait for the wheels to spawn
        //     yield return new WaitForSeconds(0.1f);
        //     FindRealWheels();
        // }

        SpawnFakeWheel(ref fakeFrontLeftWheel, realFrontLeftWheel);
        SpawnFakeWheel(ref fakeFrontRightWheel, realFrontRightWheel);
        SpawnFakeWheel(ref fakeRearLeftWheel, realRearLeftWheel);
        SpawnFakeWheel(ref fakeRearRightWheel, realRearRightWheel);
    }

    private void FindRealWheels()
    {
        // Can be improved
        realFrontLeftWheel = GameObject.Find("FrontLeftWheel(Clone)");
        realFrontRightWheel = GameObject.Find("FrontRightWheel(Clone)");
        realRearLeftWheel = GameObject.Find("RearLeftWheel(Clone)");
        realRearRightWheel = GameObject.Find("RearRightWheel(Clone)");
    }

    private void SpawnFakeWheel(ref GameObject fakeWheel, GameObject realWheel)
    {
        fakeWheel = fakeCar.transform.Find(realWheel.name).gameObject;
        Destroy(fakeWheel.GetComponent<NetworkObject>());
        Destroy(fakeWheel.GetComponent<NetworkTransform>());
        realWheel.GetComponent<MeshRenderer>().enabled = false;
    }

    private GameObject SpawnWheel(GameObject wheelPrefab, Transform wheelParent)
    {
        var wheel = Instantiate(wheelPrefab, wheelsParent);
        wheel.GetComponent<NetworkObject>().Spawn();
        _ = wheel.GetComponent<NetworkObject>().TrySetParent(wheelParent);
        return wheel;
    }
    public void SpawnWheels(Transform car)
    {
        realFrontLeftWheel = SpawnWheel(frontLeftWheelPrefab, car);
        realFrontRightWheel = SpawnWheel(frontRightWheelPrefab, car);
        realRearLeftWheel = SpawnWheel(rearLeftWheelPrefab, car);
        realRearRightWheel = SpawnWheel(rearRightWheelPrefab, car);
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

    private void SetFakeWheelPositionAndRotation(Transform fakeWheelTransform, Transform realWheelTransform)
    {
        fakeWheelTransform.SetPositionAndRotation(
            new Vector3(fakeWheelTransform.position.x, realWheelTransform.position.y, fakeWheelTransform.position.z),
            Quaternion.Lerp(fakeWheelTransform.rotation, realWheelTransform.rotation, wheelsLerpSpeed*Time.deltaTime)
        );
    }

    void FixedUpdate()
    {
        if(fakeCar != null && fakeFrontLeftWheel != null && fakeFrontRightWheel != null){
            fakeCar.transform.SetPositionAndRotation(
                Vector3.Lerp(fakeCar.transform.position, transform.position, carLerpSpeed*Time.deltaTime),
                transform.rotation
            );

            SetFakeWheelPositionAndRotation(fakeFrontLeftWheel.transform, frontLeftWheelTransform);
            SetFakeWheelPositionAndRotation(fakeFrontRightWheel.transform, frontRightWheelTransform);
            SetFakeWheelPositionAndRotation(fakeRearLeftWheel.transform, rearLeftWheelTransform);
            SetFakeWheelPositionAndRotation(fakeRearRightWheel.transform, rearRightWheelTransform);
        }
    }

    public Transform frontLeftWheelTransform
    {
        get => realFrontLeftWheel.transform;
    }

    public Transform frontRightWheelTransform
    {
        get => realFrontRightWheel.transform;
    }

    public Transform rearLeftWheelTransform
    {
        get => realRearLeftWheel.transform;
    }

    public Transform rearRightWheelTransform
    {
        get => realRearRightWheel.transform;
    }
}
