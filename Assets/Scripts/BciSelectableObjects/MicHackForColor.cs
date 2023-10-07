using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MicHackForColor : MonoBehaviour
{
    private Image image;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        image = GetComponent<Image>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(image.color != spriteRenderer.color){
            image.color = spriteRenderer.color;
        }
    }
}
