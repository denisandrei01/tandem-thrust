using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MicHack : MonoBehaviour
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
        if(image.sprite != spriteRenderer.sprite){
            image.sprite = spriteRenderer.sprite;
        }
    }
}
