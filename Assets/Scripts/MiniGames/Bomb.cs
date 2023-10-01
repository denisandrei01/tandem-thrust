using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : IMinigame
{
    [SerializeField] private GameObject warning;
    [SerializeField] private List<GameObject> paterns;

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
    }

    public void TryToCutWire(int wire)
    {
        GameManagers.playerController.PlayerCutWireServerRpc(GameManagers.playerController.OwnerClientId,
            paterns[currentPattern].GetComponent<BombPattern>().correctWire == wire
        );
    }

    public void Finished()
    {
        warning.SetActive(false);
        if(GameManagers.isCopilot){
            GameManagers.gameplayManager.powerupManager.powerupPanel.SetActive(true);
            paterns[currentPattern].SetActive(false);
        }
    }
}
