using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPlayerPrefab;

    [SerializeField] private GameObject lobbyNoTeamPlace;
    [SerializeField] private List<LobbyTeam> lobbyTeamsPlace;

    [SerializeField] private GameObject startGameButton;

    private Dictionary<ulong, GameObject> playersInstances = new();

    public void CreatePlayer(Player player)
    {
        var playerInstance = Instantiate(lobbyPlayerPrefab, lobbyNoTeamPlace.GetComponentInChildren<GridLayoutGroup>().transform);
        playerInstance.GetComponentInChildren<TMP_Text>().text = player.name;
        if(player.role == Player.Role.Pilot){
            lobbyTeamsPlace[player.teamId].SetAsPilot(playerInstance);
        }
        else if(player.role == Player.Role.Copilot){
            lobbyTeamsPlace[player.teamId].SetAsCopilot(playerInstance);
        }
        playersInstances.Add(player.id, playerInstance);
    }

    public void DeletePlayer(Player player)
    {
        if(playersInstances.ContainsKey(player.id)){
            Destroy(playersInstances[player.id]);
            playersInstances.Remove(player.id);
            if(player.teamId != -1 && player.role != Player.Role.None){
                if(player.role == Player.Role.Pilot){
                    lobbyTeamsPlace[player.teamId].RemovePilot();
                }
                else{
                    lobbyTeamsPlace[player.teamId].RemoveCopilot();
                }
            }
        }
    }

    public void UpdatePlayer(Player player, int previousTeamId, Player.Role previousRole)
    {
        // Would be better to have separate update methods for every parameter
        GameObject playerInstance;
        if (playersInstances.TryGetValue(player.id, out playerInstance)){
            if(player.role == Player.Role.None){
                playerInstance.transform.SetParent(lobbyNoTeamPlace.GetComponentInChildren<GridLayoutGroup>().transform);
            }
            else if(player.role == Player.Role.Pilot){
                lobbyTeamsPlace[player.teamId].SetAsPilot(playerInstance);
            }
            else{
                lobbyTeamsPlace[player.teamId].SetAsCopilot(playerInstance);
            }

            if(previousTeamId != -1 && previousRole != Player.Role.None){
                if(previousRole == Player.Role.Pilot){
                    lobbyTeamsPlace[previousTeamId].RemovePilot();
                }
                else{
                    lobbyTeamsPlace[previousTeamId].RemoveCopilot();
                }
            }
        }
    }

    public void DisconnectButton()
    {
        NetworkManager.Singleton.Shutdown();
        foreach(KeyValuePair<ulong, GameObject> ins in playersInstances){
            Destroy(ins.Value);
        }
        playersInstances.Clear();
        GameManagers.mainMenuManager.BackToMainMenu();
    }

    public void StartGameButton()
    {
        GameManagers.playerController.StartGame();
    }

    public void ActivateStartGameButton()
    {
        startGameButton.SetActive(true);
    }

    public void ChooseTeamAsPilot(int teamId)
    {
        GameManagers.playerController.PlayerSelectTeamServerRpc(
            GameManagers.playerController.OwnerClientId, Player.Role.Pilot, teamId
        );
    }

    public void ChooseTeamAsCopilot(int teamId)
    {
        GameManagers.playerController.PlayerSelectTeamServerRpc(
            GameManagers.playerController.OwnerClientId, Player.Role.Copilot, teamId
        );
    }
}
