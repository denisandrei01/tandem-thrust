using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : IMinigame
{
    [SerializeField] private GameObject warning;
    [SerializeField] private List<GameObject> paterns;

    private bool isActive = false;

    private int currentPattern;

    public override void Activate(int patern)
    {
        if(GameManagers.isCopilot){
            GameManagers.gameplayManager.powerupManager.powerupPanel.SetActive(false);
            paterns[patern].SetActive(true);
        }
        else{
            warning.SetActive(true);
        }

        currentPattern = patern;
        isActive = true;
    }

    public void TryToCutWire(int wire)
    {
        if(!isActive){
            return;
        }

        GameManagers.playerController.PlayerCutWireServerRpc(GameManagers.playerController.OwnerClientId,
            paterns[currentPattern].GetComponent<BombPattern>().correctWire == wire
        );
    }

    public void Finished()
    {
        isActive = false;
        warning.SetActive(false);
        if(GameManagers.isCopilot){
            GameManagers.gameplayManager.powerupManager.powerupPanel.SetActive(true);
            paterns[currentPattern].SetActive(false);
        }
    }
}
