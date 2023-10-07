using System.Collections;
using UnityEngine;

public class StarIdle : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 60.0f;
    [SerializeField] private float starsRespawnTimer;

    private float initialPositionY;

    void Start()
    {
        initialPositionY = transform.position.y;
    }

    void Update()
    {
        RotateStar();
    }

    void RotateStar()
    {
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.x = 180f;
        currentRotation.z = 0f;
        currentRotation.y += rotationSpeed * Time.deltaTime;
        transform.eulerAngles = currentRotation;

        transform.position = new Vector3(transform.position.x, Mathf.Sin(Time.time) / 2 + initialPositionY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            StartCoroutine(ResetStar());
        }
    }

    private IEnumerator ResetStar()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        gameObject.GetComponent<MeshCollider>().enabled = false;

        yield return new WaitForSeconds(starsRespawnTimer);

        gameObject.GetComponent<MeshRenderer>().enabled = true;
        gameObject.GetComponent<MeshCollider>().enabled = true;
    }
}
