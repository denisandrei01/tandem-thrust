using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
public class PlayerController : NetworkBehaviour
{

    public Player.Role playerRole = Player.Role.None;

    public override void OnNetworkSpawn()
    {
        if(!IsOwner){
            return;
        }
        GameManagers.playerController = this;

        if(IsServer){
            GameManagers.serverManager = GameObject.Find("ServerManager").GetComponent<ServerManager>();
            GameManagers.serverManager.Init();
            NetworkManager.Singleton.OnClientDisconnectCallback += GameManagers.serverManager.RemovePlayer;
        }

        PlayerJoinLobbyServerRpc(OwnerClientId, GameManagers.mainMenuManager.playerName);
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        GameManagers.gameplayManager = GameObject.Find("GameplayManager").GetComponent<GameplayManager>();
        InitGameSceneServerRpc();
    }

    public void StartGame()
    {
        NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        var status =  NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        if(status != SceneEventProgressStatus.Started){
            Debug.LogError("Error starting the scene");
        }
    }

#region LobbyUiRPCs

    [ServerRpc]
    private void PlayerJoinLobbyServerRpc(ulong clientId, string playerName)
    {
        // Sync the data with the new client
        ClientRpcParams clientRpcParams = new ClientRpcParams{
            Send = new ClientRpcSendParams{
            TargetClientIds = new ulong[]{ clientId }
            }
        };
        foreach (var player in GameManagers.serverManager.players)
        {
            CreatePlayerInLobbyClientRpc(player, clientRpcParams);
        }
        var newPlayer = GameManagers.serverManager.NewPlayer(clientId, playerName);
        CreatePlayerInLobbyClientRpc(newPlayer);
    #if UNITY_EDITOR
            GameManagers.lobbyManager.ActivateStartGameButton();
    #else
        if(GameManagers.serverManager.players.Count >= 2){ 
            //ToDo Should appear only if each player is in a team and have a role
            GameManagers.lobbyManager.ActivateStartGameButton();
        }
    #endif
    }

    [ClientRpc]
    private void CreatePlayerInLobbyClientRpc(Player player, ClientRpcParams clientRpcParams = default)
    {
        GameManagers.lobbyManager.CreatePlayer(player);
    }

    [ClientRpc]
    public void DeletePlayerFromLobbyClientRpc(Player player)
    {
        GameManagers.lobbyManager.DeletePlayer(player);
    }

    [ClientRpc]
    public void UpdatePlayerTeamInLobbyClientRpc(Player player, int previousTeamId, Player.Role previousRole)
    {
        GameManagers.lobbyManager.UpdatePlayer(player, previousTeamId, previousRole);
    }

    [ServerRpc]
    public void PlayerSelectTeamServerRpc(ulong playerId, Player.Role role, int teamId)
    {
        var player = GameManagers.serverManager.GetPlayer(playerId);
        Player.Role previousRole = player.role;
        int previousTeamId = player.teamId;
        if(GameManagers.serverManager.SetPlayerTeam(playerId, role, teamId)){
            UpdatePlayerTeamInLobbyClientRpc(player, previousTeamId, previousRole);
        }
    }

#endregion // LobbyUiRPCs

#region Gameplay

#region init
    [ServerRpc]
    private void InitGameSceneServerRpc()
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams{
            Send = new ClientRpcSendParams{}
        };
        GameManagers.serverManager.playersReady = 0;

        //Spawn cars
        List<NetworkObject> cars = new(GameManagers.teamsNumber);
        for(int teamId=0; teamId<GameManagers.teamsNumber; teamId++){
            if(GameManagers.serverManager.HasPilot(teamId)){
                cars.Add(GameManagers.gameplayManager.SpawnCar(teamId).GetComponent<NetworkObject>());
            }
            else{
                cars.Add(null);
            }
        }

        GameManagers.gameplayManager.starSpawner.SpawnStars();

        foreach(var player in GameManagers.serverManager.players){
            // Give each player its role and car object
            clientRpcParams.Send.TargetClientIds = new ulong[] { player.id };
            if(player.role == Player.Role.None){
                InitPlayerSpectatorClientRpc(player, clientRpcParams);
            }
            else{
               InitPlayerClientRpc(player, cars[player.teamId], clientRpcParams);
               if(player.role == Player.Role.Pilot){
                GameManagers.serverManager.cars.Add(player.id, cars[player.teamId]);
               }
            }
        }

        // We do this separately because it takes time to spawn all the object, and they need to be spawned when call Init
        for(int teamId=0; teamId<GameManagers.teamsNumber; teamId++){
            if(cars[teamId] != null){
                //Car attribute needs to be initialized on every client, as unity sync only the transform component
                InitCarClientRpc(teamId, cars[teamId]);
            }
        }
    }

    [ServerRpc]
    public void PlayerReadyServerRpc()
    {
        if(++GameManagers.serverManager.playersReady == GameManagers.serverManager.players.Count){
            // ToDo Start countdown
            StartGameClientRpc();
            GameManagers.serverManager.gameTime = 0;
            GameManagers.serverManager.trackTime = true;
            GameManagers.serverManager.teamsFinished = 0;
        }
        Debug.Log("Ready: " + GameManagers.serverManager.playersReady + "\\" + GameManagers.serverManager.players.Count);
    }

    [ClientRpc]
    private void InitPlayerClientRpc(Player player, NetworkObjectReference carReference, ClientRpcParams clientRpcParams)
    {
        if(GameManagers.gameplayManager == null){
            // In case it wasn't ready on OnSceneLoaded
            GameManagers.gameplayManager = GameObject.Find("GameplayManager").GetComponent<GameplayManager>();
        }
        if (carReference.TryGet(out NetworkObject carNetworkObject)){
            GameManagers.playerController.car = carNetworkObject.gameObject;
        }
        GameManagers.playerController.playerRole = player.role;
        GameManagers.gameplayManager.Init(player, this.IsServer);
    }

    [ClientRpc]
    private void InitPlayerSpectatorClientRpc(Player player, ClientRpcParams clientRpcParams)
    {
        if(GameManagers.gameplayManager == null){
            // In case it wasn't ready on OnSceneLoaded
            GameManagers.gameplayManager = GameObject.Find("GameplayManager").GetComponent<GameplayManager>();
        }
        GameManagers.gameplayManager.Init(player, this.IsServer);
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        GameManagers.gameplayManager.StartGame();
    }

    [ClientRpc]
    private void InitCarClientRpc(int teamId, NetworkObjectReference carReference)
    {
        if (carReference.TryGet(out NetworkObject carNetworkObject)){
            // Delete the controller on the cars if it is not the server
            GameManagers.gameplayManager.InitCar(carNetworkObject.gameObject, teamId, !IsServer);
        }
    }

#endregion //init

#region Pilot

    public GameObject car;

    [ServerRpc]
    public void PilotInputServerRpc(ulong pilotId, float horizontalInput, float verticalInput, bool isBraking)
    {
        var carController = GameManagers.serverManager.GetCar(pilotId).GetComponent<Controller>();
        carController.SetInput(horizontalInput, verticalInput, isBraking);

        ClientRpcParams clientRpcParams = new ClientRpcParams{
            Send = new ClientRpcSendParams{
            TargetClientIds = new ulong[]{ pilotId }
            }
        };
        UpdateSpeedClientRpc(carController.GetSpeed(), clientRpcParams);
    }

    [ClientRpc]
    public void UpdateSpeedClientRpc(float speed, ClientRpcParams clientRpcParams)
    {
        // We need to update it manually as the rigidbody speed is 0 on the client side
        GameManagers.playerController.car.GetComponent<CarManager>().speed = speed;
    }

#endregion //Pilot

    [ClientRpc]
    public void UpdateStarsClientRpc(uint stars, ClientRpcParams clientRpcParams)
    {
        // We need to update it manually as the rigidbody speed is 0 on the client side
        GameManagers.gameplayManager.UpdateStars(stars);
    }

    [ClientRpc]
    public void UpdateLapNrClientRpc(int lap, ClientRpcParams clientRpcParams)
    {
        GameManagers.gameplayManager.UpdateLapNr(lap);
    }

    [ClientRpc]
    public void FinishedClientRpc(int teamId, ClientRpcParams clientRpcParams)
    {
        GameManagers.gameplayManager.leaderboardManager.SetOwnerForEntry(teamId);
        GameManagers.gameplayManager.FinishGame();
    }

    [ClientRpc]
    public void AddTeamToLeaderboardClientRpc(int teamId, float time)
    {
        GameManagers.gameplayManager.leaderboardManager.AddEntry(teamId, time);
    }

    [ClientRpc]
    public void WrongDirectionClientRpc(ClientRpcParams clientRpcParams)
    {
        GameManagers.gameplayManager.notificationManager.ShowNotification("WrongDirection");
    }

    [ClientRpc]
    public void CorrectDirectionClientRpc(ClientRpcParams clientRpcParams)
    {
        GameManagers.gameplayManager.notificationManager.DiscardNotification("WrongDirection");
    }

    [ServerRpc]
    public void TryToActivatePowerUpServerRpc(ulong playerId, int powerUpID)
    {
        GameManagers.serverManager.ActivatePowerUp(playerId, powerUpID);
    }

    [ClientRpc]
    public void ActivatePowerClientRpc(int powerUpID, int powerUpPatern, ClientRpcParams clientRpcParams)
    {
        GameManagers.gameplayManager.powerupManager.Activate(powerUpID, powerUpPatern);
        if(GameManagers.playerController.playerRole == Player.Role.Copilot){
            GameManagers.gameplayManager.powerupManager.powerupPanel.SetActive(false);
        }
    }

    [ServerRpc]
    public void CleanInkServerRpc(ulong copilodId, int inkId)
    {
        GameManagers.serverManager.CleanInk(copilodId, inkId);
    }

    [ClientRpc]
    public void CleanInkClientRpc(int inkId, ClientRpcParams clientRpcParams)
    {
        // ToDo: rewrite linia asta 
        GameManagers.gameplayManager.powerupManager.minigames[0].manager.gameObject.GetComponent<WindshieldCleaner>().CleanInk(inkId);
    }

#endregion //Gameplay

}
