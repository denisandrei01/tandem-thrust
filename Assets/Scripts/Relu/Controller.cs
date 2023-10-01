using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    /*  THE HOLY VARIA(BI)BLE:
     *  Aici is toate variabilele, Biblia codului
     * (nu o sa explic chestii precum horizontalInput, ca se intelege de la sine)
     * 
     * currentSteerAngle: valoare intre 0-100, e un handler pt steerAngle din Wheel Collider,
     * in el stochez preferintele cu care mai schimb una alta la modul de cotire, cu cat e mai
     * mare valoarea, cu atat ia mai brusc masina curba
     * 
     * isBraking: check daca se apasa sau nu Space, adica frana de mana
     * 
     * isAccelerating si isReversing: verific daca merg inainte sau inapoi, pt niste chestii mai incolo in cod
     * 
     * save: verific daca apas pe H, adica butonul care te salveaza din orice primejdie =)
     * (Poate scot butonul si il fac mai incolo sa te respawneze automat, mai vedem)
     * 
     * speed: variabila pe care o folosesc sa fac rost de magnitudinea velocitatii masini, ca sa imi dau seama cu ce
     * viteza merge masina intr-un moment dat, folosesc sqrMagnitude pt calcule ca e mai rapid decat magnitude
     * 
     * rb: variabila dummy pt Rigidbody-ul masinii
     * 
     * 
     * 
     * motorForce, brakeForce: variabile pt handle-uit proprietatile cu acelasi nume ale Wheel Collider-urilor,
     * motorForce e efectiv acceleratia, sau mai bine zis turatiile de motor, brakeForce se intelege de la sine
     * 
     * ATENTIE!!!!!
     * 
     * Cu cat e mai mare valoarea lui motorForce, cu atat e nevoie de brakeForce mai mare ca sa poata opri masina,
     * nu recomand sa schimbati valorile numa daca trebe neaparat
     * 
     * 
     * maxSteeringAngle: cu variabila asta schimb in Editor cat de mult vrei sa coteasca masina
     * 
     * maxSpeed: Viteza maxima pe care o poate lua masina, relativa la variabila "speed". Nu schimba numa daca stii ce faci =)
     * 
     * 
     * 
     * front/rear Left/Right WheelCollider: fac rost de Wheel Collider-ul de la fiecare roata. Aici se intampla toata magia =)
     * 
     * - II - Transform: fac rost de componenta Transform (pozitie + rotatie) a obiectelor de tip Wheel. Pt animatia rotilor si asa,
     * sa le vezi cum se invartesc =)))
    */

    public float horizontalInput;
    public float verticalInput;
    private float currentSteerAngle;
    private bool isBraking;
    private bool isAccelerating;
    private bool isReversing;
    private bool save;
    private float speed;
    private Rigidbody rb;
    private CarManager carManager;

    public float initialMotorForce = 0f;

    [SerializeField] private float motorForce;
    [SerializeField] private float brakeForce;
    [SerializeField] private float maxSteeringAngle;
    private float maxSpeed;
    [SerializeField] private Image[] powersImages;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider;
    [SerializeField] private WheelCollider rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform;
    [SerializeField] private Transform rearRightWheelTransform;

    private Transform fromTrack;

    public float GetBrake()
    {
        return brakeForce;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public void SetMaxSpeed(float _speed)
    {
        maxSpeed = _speed;
    }

    public float GetTorque()
    {
        return motorForce;
    }

    public void SetTorque(float motor)
    {
        motorForce = motor;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carManager = GetComponent<CarManager>();
        initialMotorForce = motorForce;
        maxSpeed = carManager.maxSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Star")
        {
            GameManagers.serverManager.StarColected(this.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }


    // Folosesc FixedUpdate() ca nu am nevoie de atata reactie ca la Update() simplu. E mai eficient si acum nu ai nevoie de RTX 4090 ca sa iti mearga =)
    void FixedUpdate()
    {
        if(!GameManagers.isGameRunning){
            // needed so the cars will still move even if the host finishes the game
            return;
        }

        speed = rb.velocity.sqrMagnitude;

        /* SaveCar()
        * 
        * Functie facuta repede ca sa pot ridica masina. Sa nu dureze 10000 de ani sa fac debugging =)
        */
        // SaveCar();


        // Daca nu accelerezi, opreste incet masina, altfel, accelereaza (daca nu fac asta, masina accelereaza la infinit)
        // (A se schimba cat de repede, e ineficient si clunky =) )
        if (Mathf.Abs(verticalInput) < 0.01f)
        {
            StopCar();
        }
        else
        {
            HandleMotor();
        }

        /* HandleSteering()
        * De aici coteste efectiv masina, animatiile is in alta parte
        */ 
        HandleSteering();

        /* UpdateWheels()
        * Aici is animatiile pt roti, sa se roteasca si sa le vezi cum se misca cand cotesti masina
        */
        UpdateWheels();
    }

    //Get inputs from the server
    public void SetInput(float _horizontalInput, float _verticalInput, bool _isBraking)
    {
        // Fata spate stanga dreapta, the usual
        horizontalInput = _horizontalInput;
        verticalInput = _verticalInput;

        // Frana de mana
        isBraking = _isBraking;

        // Te ridica in aer cu 1 unitate (A nu se abuza de putere)
        // save = Input.GetKeyDown(KeyCode.H);

        // De aici verific daca merg inainte sau inapoi (Trebe research sa vedem de se poate face mai bine)
        isAccelerating = verticalInput > 0f;
        isReversing = verticalInput < 0f;
    }

    private void SaveCar()
    {
        // Daca apesi pe "H"
        // if (save)
        // {
        //     if(track.GetCheckpoint() == 0 && track.GetCurrentLap() == 1)
        //     {
        //         transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        //         transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        //     }
        //     else
        //     {
        //         fromTrack = track.GetCheckpointList()[track.GetCheckpoint() - 1].transform;
        //         transform.position = new Vector3(fromTrack.position.x, fromTrack.position.y, fromTrack.position.z);
        //         Vector3 checkpointDirection = fromTrack.forward;
        //         Quaternion checkpointRotation = Quaternion.LookRotation(checkpointDirection, Vector3.up);
        //         checkpointRotation *= Quaternion.Euler(0f, -90f, 0f);
        //         transform.rotation = checkpointRotation;
        //     }
        // }
    }

    private void StopCar()
    {
        // Oprim rotatiile la masina
        frontLeftWheelCollider.motorTorque = 0f;
        frontRightWheelCollider.motorTorque = 0f;

        // Si punem frana (Valoarea 30f e magica, nu se schimba!!!!!)
        ApplyBraking(brakeForce / 30f, 0.1f);
    }

    private void HandleMotor()
    {
        // Daca nu trece viteza actuala peste viteza maxima setata in Editor
        if (speed < maxSpeed)
        {
            // Adaugam un factor, care se schimba in functie de nevoie
            float motorTorqueFactor = 1f;

            // Daca am trecut peste jumate din viteza maxima a masinii
            if (speed >= maxSpeed / 2f)
            {
                // Aplicam factorul care scade potentialul masinii cu cat e mai aproape de viteza maxima
                motorTorqueFactor = 1f - (speed / maxSpeed);
            }

            /* Aici se aplica rotatiile masinii, efectiv acceleratia: Se aplica input-ul de la tastatura,
            * daca e cu + (W) atunci merge inainte, daca e cu - (S) merge inapoi, apoi totul se inmulteste
            * cu motorForce-ul setat de tine. Magie!!!!! Merge totul plug and play (Daca esti sub jumatea
            * vitezei maxime, ai acceleratie nelimitata, ca sa fie mai realistic (Research daca se poate face mai bine pls))
            */
            frontLeftWheelCollider.motorTorque = verticalInput * motorForce * motorTorqueFactor;
            frontRightWheelCollider.motorTorque = verticalInput * motorForce * motorTorqueFactor;
        }

        // Daca viteza actuala trece peste viteza maxima, oprim acceleratia
        else
        {
            frontLeftWheelCollider.motorTorque = 0f;
            frontRightWheelCollider.motorTorque = 0f;
        }

        // Daca pui frana de mana, ei bine, se pune frana de mana =)
        if (isBraking)
        {
            /* Aplicam brakeForce, dar de 100 de ori mai puternic, ca sa simulam frana de mana reala. 0.1f reprezinta viteza masinii
            * necesara ca sa dea release la frane (Trebe facut mai bine, help)
            */
            ApplyBraking(brakeForce * 100f, 0.1f);
        }

        // Daca nu pui frana, good =)
        else
        {
            // Eventual dam release la frana daca mai e pusa, sa nu avem probleme
            ReleaseBreaking();
        }
    }

    public void ApplyBraking(float brakeforce, float speedLimitBreaking)
    {
        // Aplicam frana la toate rotile (Poate schimbam sa fie doar la anumite roti mai incolo)
        rearLeftWheelCollider.brakeTorque = brakeforce;
        rearRightWheelCollider.brakeTorque = brakeforce;
        frontLeftWheelCollider.brakeTorque = brakeforce;
        frontRightWheelCollider.brakeTorque = brakeforce;

        // Daca viteza masinii este sub limita data de tine si pui frana in momentul dat
        if (speed < speedLimitBreaking && isBraking)
        {
            // Scoate franele
            ReleaseBreaking();
        }
    }
    private void ReleaseBreaking()
    {
        // Dulce eliberare a franei
        frontLeftWheelCollider.brakeTorque = 0f;
        frontRightWheelCollider.brakeTorque = 0f;
        rearLeftWheelCollider.brakeTorque = 0f;
        rearRightWheelCollider.brakeTorque = 0f;
    }

    private void HandleSteering()
    {
        // Coteste in functie de cat ti-e horizontalInput
        currentSteerAngle = maxSteeringAngle * horizontalInput;

        // Coteste efectiv masina in functie de variabila de mai sus
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        // Dam update vizual la roti, sa vedem cum se misca pentru fiecare roata in parte
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        // Facem rost de pozitia si rotatia rotii
        Vector3 pos;
        Quaternion rot;

        // Vedem unde ar trebui sa fie si stocam valorile in pos si rot
        wheelCollider.GetWorldPose(out pos, out rot);

        // Facem update-ul sa se si vada ca se coteste =)
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}
