using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindshieldCleaner : IMinigame
{
    [SerializeField] private List<GameObject> paterns;
    private int currentPattern;

    private int needsToClean, cleaned;

    public override void Activate(int patern)
    {
        paterns[patern].SetActive(true);
        foreach (var ink in paterns[patern].GetComponent<InkPattern>().inks){
            ink.SetActive(true);
            if(!GameManagers.isCopilot){
                ink.GetComponent<Button>().enabled = false;
            }
        }

        currentPattern = patern;
        needsToClean = paterns[patern].GetComponent<InkPattern>().inks.Count;
        cleaned = 0;
    }

    public void TryToCleanInk(int inkIndex)
    {
        Debug.Log("Try to clean: " + inkIndex);
        GameManagers.playerController.CleanInkServerRpc(GameManagers.playerController.OwnerClientId, inkIndex);
    }

    public void CleanInk(int inkIndex)
    {
        Debug.Log("Clean: " + inkIndex);
        paterns[currentPattern].GetComponent<InkPattern>().inks[inkIndex].SetActive(false);
        if(++cleaned >= needsToClean){
            if(GameManagers.isCopilot){
                GameManagers.gameplayManager.powerupManager.powerupPanel.SetActive(true);
            }
        }
    }

}
