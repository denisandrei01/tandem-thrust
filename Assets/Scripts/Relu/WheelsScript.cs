using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelsScript : MonoBehaviour
{
    // controller e referinta la scriptul masinii si cu massCoef setezi cat de "punishing" sa fie mersul off-road
    [SerializeField] private Controller controller;
    [SerializeField] private float massCoef;

    // Nu recomand sa umblati aici =))))
    private float raycastDistance = 1f;

    // Verificare daca esti pe drum sau nu
    private bool road = true;

    void FixedUpdate()
    {
        // Ray sub roata ca sa vezi pe ce suprafata te aflii
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            // Daca esti pe iarba
            if (hit.collider.tag == "Grass" && road)
            {
                // Crestem masa =) sa mearga mai incet
                controller.GetComponent<Rigidbody>().mass *= massCoef;

                // Si nu te mai aflii pe drum =(
                road = false;
            }
            if (hit.collider.tag == "Road" && !road)
            {
                // Revenim la masa initiala. Totul e ok acum
                controller.GetComponent<Rigidbody>().mass /= massCoef;
                
                // Esti inapoi pe drum =)
                road = true;
            }
        }
    }
}
