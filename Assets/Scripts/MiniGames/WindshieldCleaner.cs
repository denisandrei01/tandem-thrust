using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WindshieldCleaner : IMinigame
{
    [SerializeField] private List<GameObject> paterns;
    private int currentPattern;

    private int needsToClean, cleaned;

    private bool isActive = false;

    public override void Activate(int patern)
    {
        paterns[patern].SetActive(true);
        foreach (var ink in paterns[patern].GetComponent<InkPattern>().inks){
            ink.SetActive(true);
            if(!GameManagers.isCopilot){
                ink.GetComponent<Button>().enabled = false;
            }
        }

        if(GameManagers.playerController.playerRole == Player.Role.Copilot){
            GameManagers.gameplayManager.powerupManager.powerupPanel.SetActive(false);
        }

        currentPattern = patern;
        needsToClean = paterns[patern].GetComponent<InkPattern>().inks.Count;
        cleaned = 0;
        isActive = true;
    }

    public void TryToCleanInk(int inkIndex)
    {
        if(!isActive){
            return;
        }
        Debug.Log("Try to clean: " + inkIndex);
        if(paterns[currentPattern].GetComponent<InkPattern>().inks[inkIndex].activeSelf){ // Only if it wasn't already cleaned
            GameManagers.playerController.CleanInkServerRpc(GameManagers.playerController.OwnerClientId, inkIndex);
        }
    }

    public void CleanInk(int inkIndex)
    {
        Debug.Log("Clean: " + inkIndex);
        paterns[currentPattern].GetComponent<InkPattern>().inks[inkIndex].SetActive(false);
        if(++cleaned >= needsToClean){
            isActive = false;
            paterns[currentPattern].SetActive(false);
            if(GameManagers.isCopilot){
                GameManagers.gameplayManager.powerupManager.powerupPanel.SetActive(true);
            }
        }
    }

}
