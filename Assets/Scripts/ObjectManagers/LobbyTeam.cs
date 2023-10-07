using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTeam : MonoBehaviour
{
    [SerializeField] private Team team;
    [SerializeField] private GameObject pilotSlot, copilotSlot;

    private void Start() 
    {
        this.GetComponent<Image>().color = team.color;
        pilotSlot.GetComponentInChildren<Image>().color = team.color;
        copilotSlot.GetComponentInChildren<Image>().color = team.color;
    }

    public void SetAsPilot(GameObject playerInstance)
    {
        playerInstance.transform.SetParent(this.transform);
        playerInstance.transform.position = pilotSlot.transform.position;
        pilotSlot.SetActive(false);
    }

    public void SetAsCopilot(GameObject playerInstance)
    {
        playerInstance.transform.SetParent(this.transform);
        playerInstance.transform.position = copilotSlot.transform.position;
        copilotSlot.SetActive(false);
    }

    public void RemovePilot()
    {
        pilotSlot.SetActive(true);
    }

    public void RemoveCopilot()
    {
        copilotSlot.SetActive(true);
    }
}
