using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PowerupManager : MonoBehaviour
{
    public Controller car;

    public void HandleButtonClick(TextMeshProUGUI starThresholdText)
    {
        int starThreshold = int.Parse(starThresholdText.text.Replace(" Stars", ""));
        // int currentStars = car.GetStars();

        // if (starThreshold <= currentStars)
        // {
        //     car.SetStars(currentStars - starThreshold);
        // }
        // else
        // {
        //     Debug.Log("Not enough stars!");
        // }
    }
}
