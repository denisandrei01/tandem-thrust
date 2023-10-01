using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Netcode.Transports.UTP;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel, selectServerPanel, lobbyPanel;
    [SerializeField] private TMP_InputField serverAddressInputField, playerNameInputField;
    public string playerName
    {
        get => playerNameInputField.text;
    }

    public void CreateServerButton()
    {
        mainMenuPanel.SetActive(false);
        NetworkManager.Singleton.StartHost();
        lobbyPanel.SetActive(true);
    }

    public void SelectServerButton()
    {
        selectServerPanel.SetActive(true);
    }

    private string validateAddress(string address)
    {
        if(address.Length == 0){
            return "127.0.0.1";
        }
        //ToDo more validation
        return address;
    }

    public void JoinServerButton()
    {
        selectServerPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
        string validAddress = validateAddress(serverAddressInputField.text);
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = validAddress;
        NetworkManager.Singleton.StartClient();
        lobbyPanel.SetActive(true);
    }

    public void CancelButton()
    {
        selectServerPanel.SetActive(false);
        serverAddressInputField.text = "localhost";
    }

    public void BackToMainMenu()
    {
        CancelButton();
        lobbyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
