using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class IMinigame : MonoBehaviour{
    public abstract void Activate(int patern);
}

public class PowerupManager : MonoBehaviour
{
    public GameObject powerupPanel;

    [Serializable]
    public struct MinigameStruct{
        public string name;
        public IMinigame manager;
        public uint cost;
    }

    [SerializeField] public List<MinigameStruct> minigames;

    public void TryToActivatePowerUp(int powerUpID)
    {
        GameManagers.playerController.TryToActivatePowerUpServerRpc(GameManagers.playerController.OwnerClientId, powerUpID);
    }

    public void Activate(int powerUpID, int powerUpPatern)
    {
        minigames[powerUpID].manager.Activate(powerUpPatern);
    }
}
