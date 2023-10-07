using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Powers : MonoBehaviour
{
    [SerializeField] private Controller car;
    [SerializeField] private TextMeshProUGUI text;
    private int stars;
    private int starsRequirement;

    void Start()
    {
        starsRequirement = int.Parse(text.text.Replace(" Stars", ""));
    }

    void ChangeColor(Image img, float alpha)
    {
        var coloare = img.color;
        coloare.a = alpha;
        img.color = coloare;
    }

    void UpdateGraphicsPowerups()
    {
        var img = gameObject.GetComponent<Image>();

        if(stars < starsRequirement)
        {
            ChangeColor(img, 0.3f);
        }
        else
        {
            ChangeColor(img, 0.8f);
        }
    }
}
