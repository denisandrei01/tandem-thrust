using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    /* VariBiblia 2: Camera edition
     * 
     * moveSmoothness, rotSmoothness: in cat timp (secunde) sa se miste/roteasca, camera dupa masina
     * 
     * reverseRotationSpeed: in cat timp se duce camera inainte si inapoi cand schimbi intre acceleratie si reverse
     * 
     * moveOffset, rotOffset: Offset-uri ca sa setezi cu cate unitati sa fie mai incolo camera decat masina
     * 
     * carTarget, carRigidbody: fac rost de tot ce am nevoie de la masina (pozitie + rotatie si rigidbody)
     * 
     * currMoveOffset: o sa fie nevoie de valorile initiale ale moveOffset...
     */
    public float moveSmoothness;
    public float rotSmoothness;
    public float reverseRotationSpeed;

    public Vector3 moveOffset;
    public Vector3 rotOffset;

    private Transform carTarget;
    private Rigidbody carRigidbody;

    private Vector3 currMoveOffset;

    private bool isReversing = false;

    // Initializam currMoveOffset
    void Start()
    {
        currMoveOffset = moveOffset;
    }

    public void Init(GameObject target, Vector3 position)
    {
        carTarget = target.transform;
        carRigidbody = target.GetComponent<Rigidbody>();
        //Set start position
        transform.position = position;
    }

    void FixedUpdate()
    {
        if(GameManagers.gameplayManager == null || !GameManagers.gameplayManager.gameRuning){
            return;
        }

        // Only until I figure out what the camera is supposed to do for the spectator
        if(carTarget == null){
            return;
        }

        // Du-te dupa el!!
        FollowTarget();
    }

    void FollowTarget()
    {
        // Se intelege care ce face, mai explic la fiecare in parte =)))
        HandleMovement();
        HandleRotation();
        HandleReverse();
    }

    void HandleMovement()
    {
        // Facem rost de pozitia masinii si o setam ca target
        Vector3 targetPos = carTarget.TransformPoint(moveOffset);

        /* Miscam camera utilizand Lerp (folosit sa interpolezi intre 2 puncte, ca sa fie mai smooth tranzitia), incepand de
         * la camera, spre masina, folosind variabila de moveSmoothness ca sa personalizam timpul in care se face miscarea
         */
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);
    }

    void HandleRotation()
    {
        // Daca nu dai Reverse (Am facut sa difere modul de utilizare a camerei in functie de directia masinii)
        if (!isReversing)
        {
            // Tinem minte directia camerei in functie de diferenta dintre ea si masina
            var direction = carTarget.position - transform.position;

            // Setam target-ul rotatiei prin LookRotation (cauta pe net, nu scriu aici toata explicatia functiei...) si a offset-ului ales de tine
            var rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);

            // Facem tranzitia efectiva, utilizand target-ul dinainte (rotation)
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSmoothness * Time.deltaTime);
        }
    }

    void HandleReverse()
    {
        // Verificam daca mergem inainte sau inapoi
        float verticalInput = Input.GetAxis("Vertical");

        // Daca masina e aproape oprita
        if (Mathf.Abs(carRigidbody.velocity.magnitude) < 0.1f)
        {
            // Si daca apasam pe butonul de Reverse (S)
            if (verticalInput < 0)
            {
                // Inseamna ca dam reverse =)
                isReversing = true;
            }
        }

        // Daca nu dam reverse
        if (!isReversing)
        {
            // Handle-uim miscarea/rotatia ca inainte si resetam valorile la moveOffset (o sa vezi imediat de ce)
            moveOffset = currMoveOffset;
            var rotation = Quaternion.LookRotation(carTarget.forward + rotOffset, Vector3.up);

            // Ce e diferit fata de inainte la echivalentul din HandleRotation(), e viteza cu care miscam camera, pentru ca vrem sa fie diferit, nu??
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, reverseRotationSpeed * Time.deltaTime);
        }

        // Daca dam reverse
        else
        {
            // Rotim camera la 180 de grade in functie de valorile initiale
            moveOffset = new Vector3(currMoveOffset.x, currMoveOffset.y, -currMoveOffset.z);

            // In rest e la fel ca inainte tot, doar ca folosim spatele masinii in loc de fata ca si tinta (-carTarget.forward)
            var reverseRotation = Quaternion.LookRotation(-carTarget.forward + rotOffset, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, reverseRotation, reverseRotationSpeed * Time.deltaTime);

            // Daca nu mai dam reverse
            if (verticalInput >= 0)
            {
                // Nu o mai facem =)
                isReversing = false;
            }
        }
    }

}