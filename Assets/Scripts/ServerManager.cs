using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class ServerManager : MonoBehaviour
{
    public List<Player> players = new();

    private class TeamData{
        public ulong pilotId, copilotId;
        public bool hasPilot, hasCopilot;

        public uint stars;

        public int currentCheckpoint;
        public int currentLap;
        public bool wrongDirection;
        public float activeBoostTimer;

        public TeamData(){
            hasPilot = hasCopilot = wrongDirection = false;
            pilotId = copilotId = stars = 0;
            currentCheckpoint = 0;
            currentLap = 1;
        }
    }

    private List<TeamData> teamsWithActiveSpeedBoost = new();

    // map teamId to TeamData
    private List<TeamData> teams = new();

    // map pilotId to car
    public Dictionary<ulong, NetworkObject> cars = new();

    public int playersReady;
    public int playersCalibrated;

    public float gameTime = 0;
    public bool trackTime = false;
    
    public int teamsFinished = 0;

    public void Init()
    {
        for(int teamId=0; teamId<GameManagers.teamsNumber; teamId++){
            teams.Add(new TeamData());
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public Player NewPlayer(ulong playerId, string playerName)
    {
        Player newPlayer = new(playerId, playerName);
        players.Add(newPlayer);
        return newPlayer;
    }

    public void RemovePlayer(ulong playerId)
    {
        GameManagers.playerController.DeletePlayerFromLobbyClientRpc(GetPlayer(playerId));
        players.RemoveAll(player => player.id == playerId);
    }

    public bool SetPlayerTeam(ulong playerId, Player.Role role, int teamId)
    {
        try {
            var player = GetPlayer(playerId);
            player.role = role;
            player.teamId = teamId;
            if (role == Player.Role.Pilot)
            {
                teams[teamId].pilotId = player.id;
                teams[teamId].hasPilot = true;
            }
            else if (role == Player.Role.Copilot)
            {
                teams[teamId].copilotId = player.id;
                teams[teamId].hasCopilot = true;
            }
        }
        catch (Exception e){
            Debug.LogWarning(e.ToString());
            return false;
        }

        return true;
    }

    public Player GetPlayer(ulong id)
    {
        foreach(var player in players){
            if(player.id == id){
                return player;
            }
        }

        // Can't return null because Client inherits INetworkSerializable which is non null
        throw new Exception("Player " + id + " doesn't exist");
    }

    public bool HasPilot(int teamId)
    {
        return teams[teamId].hasPilot;
    }

    public ulong GetPilotId(int teamId){
        return teams[teamId].pilotId;
    }

    public GameObject GetCar(ulong pilotId){
        if (cars.TryGetValue(pilotId, out NetworkObject car)){
            return car.gameObject;
        }

        throw new Exception("Pilot " + pilotId + " doesn't have a car");
    }

    public NetworkObject GetCarById(ulong carId){
        return cars.FirstOrDefault(x => x.Value.NetworkObjectId == carId).Value;

        throw new Exception("Car " + carId + " doesn't exist");
    }

    private TeamData GetTeamByCar(ulong carId)
    {
        // if performance issue, we should consider to have another dict to map cars to TeamData
        ulong pilotId = cars.FirstOrDefault(x => x.Value.NetworkObjectId == carId).Key;

        foreach(var team in teams){
            if(team.hasPilot && team.pilotId == pilotId){
                return team;
            }
        }

        return null;
    }

    private TeamData GetTeamByCopilot(ulong copilotId)
    {
        foreach(var team in teams){
            if(team.copilotId == copilotId){
                return team;
            }
        }

        return null;
    }


    private TeamData GetTeamByPilot(ulong pilotId)
    {
        foreach(var team in teams){
            if(team.pilotId == pilotId){
                return team;
            }
        }

        return null;
    }
    public void StarColected(ulong carId)
    {
        TeamData team = GetTeamByCar(carId);
        team.stars++;

        GameManagers.playerController.UpdateStarsClientRpc(team.stars, ClientRpcParamsForTeam(team));
    }

    public void CarThroughCheckpoint(ulong carId, CheckpointSingle checkpoint)
    {
        TeamData team = GetTeamByCar(carId);

        int thisCheckpointIndex = GameManagers.gameplayManager.checkpointsList.IndexOf(checkpoint);

        if(thisCheckpointIndex == team.currentCheckpoint){
            // Skip if this is triggered multiple times for the same checkpoint
            return;
        }

        if(thisCheckpointIndex == team.currentCheckpoint + 1){
            // Correct Checkpoint
            team.currentCheckpoint++;
            if(team.wrongDirection){
                // Call correct dirrection only if was previosly going wrong
                GameManagers.playerController.CorrectDirectionClientRpc(ClientRpcParamsForTeam(team));
                team.wrongDirection = false;
            }
        }
        else if(thisCheckpointIndex == 0 && team.currentCheckpoint == GameManagers.gameplayManager.checkpointsList.Count - 1){
            if(team.wrongDirection){
                // Call correct dirrection only if was previosly going wrong
                GameManagers.playerController.CorrectDirectionClientRpc(ClientRpcParamsForTeam(team));
                team.wrongDirection = false;
            }
            // Finish Lap
            team.currentCheckpoint = 0;
            team.currentLap++;
            if(team.currentLap > GameManagers.gameplayManager.totalLaps){
                // Player Finish
                // Get the time
                float teamTime = gameTime;
                int teamId = teams.IndexOf(team);
                GameManagers.playerController.AddTeamToLeaderboardClientRpc(teamId, teamTime);
                GameManagers.playerController.FinishedClientRpc(teamId, ClientRpcParamsForTeam(team));
                if(++teamsFinished >= teams.Count){
                    //All the teams finished
                    trackTime = false;
                    // Do more
                }
                //Destroy the car
                GetCarById(carId).Despawn();
            }
            else{
                GameManagers.playerController.UpdateLapNrClientRpc(team.currentLap, ClientRpcParamsForTeam(team));
            }
        }
        else{
            // Wrong Checkpoint
            GameManagers.playerController.WrongDirectionClientRpc(ClientRpcParamsForTeam(team));
            team.wrongDirection = true;
        }
    }

    private ClientRpcParams ClientRpcParamsForTeam(TeamData team)
    {
        return new ClientRpcParams{
            Send = new ClientRpcSendParams{
            TargetClientIds = new ulong[]{ team.pilotId, team.copilotId}
            }
        };
    }

    private ClientRpcParams ClientRpcParamsAllTeamsExceptOne(TeamData team)
    {
        List<ulong> allOtherPlayers = new();

        Debug.Log("Nr of teams: " + teams.Count);

        foreach (var t in teams){
            if(t.pilotId == team.pilotId || !t.hasPilot){
                continue;
            }

            allOtherPlayers.Add(t.pilotId);
            allOtherPlayers.Add(t.copilotId);
        }

        var players = new ulong[allOtherPlayers.Count];

        for(int i=0;i<players.Length; i++){
            players[i] = allOtherPlayers[i];
            Debug.Log("To player id: " + players[i]);
        }

        return new ClientRpcParams{
            Send = new ClientRpcSendParams{
            TargetClientIds = players
            }
        };
    }

    public void ActivatePowerUp(ulong copilotId, int powerUpID)
    {
        TeamData team = GetTeamByCopilot(copilotId);

        uint powerUpCost = GameManagers.gameplayManager.powerupManager.minigames[powerUpID].cost;
        if(powerUpCost <= team.stars){
            team.stars -= powerUpCost;
            GameManagers.playerController.UpdateStarsClientRpc(team.stars, ClientRpcParamsForTeam(team));
            if(powerUpID != 0){
                int powerUpPatern = Random.Range(0, 3);
                // Activate powerUp for all the other players
                GameManagers.playerController.ActivatePowerClientRpc(powerUpID, powerUpPatern, ClientRpcParamsAllTeamsExceptOne(team));
            }
            else{
                // Speed Boost powerUp is handled on the server
                var car = GetCar(team.pilotId);
                car.GetComponent<Rigidbody>().mass = 1000;
                teamsWithActiveSpeedBoost.Add(team);
                team.activeBoostTimer = 15f;
                GameManagers.playerController.ActivatePowerClientRpc(powerUpID, 0, ClientRpcParamsForTeam(team));
            }
        }
    }

    private void DeactivateBoost(TeamData team)
    {
        var car = GetCar(team.pilotId);
        car.GetComponent<Rigidbody>().mass = 1500;
        teamsWithActiveSpeedBoost.Remove(team);
        GameManagers.playerController.SpeedBoostFinishedClientRpc(ClientRpcParamsForTeam(team));
    }

    public void CleanInk(ulong copilotId, int inkId)
    {
        TeamData team = GetTeamByCopilot(copilotId);
        GameManagers.playerController.CleanInkClientRpc(inkId, ClientRpcParamsForTeam(team));
    }


     public void TryToResetCheckpoint(ulong playerId)
    {
        TeamData team = GetTeamByPilot(playerId);
    
        Vector3 checkpointPos = GameManagers.gameplayManager.checkpointsList[team.currentCheckpoint].transform.position;
        Transform carTransform = GetCar(playerId).transform;           
        carTransform.position = new Vector3(checkpointPos.x, checkpointPos.y, checkpointPos.z);
        carTransform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Transform fromtrack = GameManagers.gameplayManager.checkpointsList[team.currentCheckpoint].transform;
        Quaternion checkpointRotation = Quaternion.LookRotation(fromtrack.forward, Vector3.up);
        checkpointRotation *= Quaternion.Euler(0f, checkpointPos.y + 90f, 0f);
        carTransform.rotation = checkpointRotation;
        GameManagers.playerController.CorrectDirectionClientRpc(ClientRpcParamsForTeam(team));
        team.wrongDirection = false;
    }
    public void PlayerCutWire(ulong copilodId, bool correct)
    {
        TeamData team = GetTeamByCopilot(copilodId);
        if(!correct){
            GameObject car = GetCar(team.pilotId);
            car.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        GameManagers.playerController.RemoveBombWarningClientRpc(ClientRpcParamsForTeam(team));
    }

    private void Update()
    {
        if(trackTime){
            gameTime += Time.deltaTime;
        }

        // We need to iterate in reverse because we remove items from the list while iterate
        for(int i=teamsWithActiveSpeedBoost.Count - 1; i>=0; i--){
            TeamData team = teamsWithActiveSpeedBoost[i];
            if(team.activeBoostTimer > 0){
                team.activeBoostTimer -= Time.deltaTime;
            }
            else{
                DeactivateBoost(team);
            }
        }
    }
}
