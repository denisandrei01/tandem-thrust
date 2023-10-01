using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [SerializeField] private GameObject body, spoiler, blimp;
    [SerializeField] private GameObject blimpPrefab;

    private float blimpYpos = 300f;

    public float maxSpeed;

    public float speed;

    public void Init(Team team)
    {
        body.GetComponent<MeshRenderer>().material.mainTexture = team.carTexture;

        blimp = Instantiate(blimpPrefab, new Vector3(transform.position.x, blimpYpos, transform.position.z), Quaternion.Euler(90f, transform.eulerAngles.y, 0f));
        blimp.GetComponent<MeshRenderer>().material.color = team.color;
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
