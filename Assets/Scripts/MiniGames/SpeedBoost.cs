using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : IMinigame
{
    [SerializeField] private GameObject activeIndicator;

    public override void Activate(int patern)
    {
        activeIndicator.SetActive(true);
    }

    public void Finished()
    {
        activeIndicator.SetActive(false);
    }
}
