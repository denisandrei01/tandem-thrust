using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedometer : MonoBehaviour
{
    private const float MAX_SPEED_ANGLE = -263;
    private const float ZERO_SPEED_ANGLE = 0;

    [SerializeField] private Transform needleTransform;
    private CarManager car;

    public void Init(CarManager carManager)
    {
        car = carManager;
        gameObject.SetActive(true);
    }

    void FixedUpdate()
    {
        if(GameManagers.gameplayManager == null || !GameManagers.gameplayManager.gameRuning){
            return;
        }

        needleTransform.eulerAngles = new Vector3(0, 0, GetSpeedRotation());
    }

    private float GetSpeedRotation()
    {
        float totalAngleSize = ZERO_SPEED_ANGLE - MAX_SPEED_ANGLE;
        float speedNormalized = car.speed / car.maxSpeed;

        return ZERO_SPEED_ANGLE - speedNormalized * totalAngleSize;
    }
}
