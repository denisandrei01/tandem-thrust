using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagers : MonoBehaviour
{
    private static GameManagers instance; 

    [SerializeField] private List<Team> _teamsData;

    [SerializeField] private MainMenuManager _mainMenuManager;
    [SerializeField] private LobbyManager _lobbyManager;
    [SerializeField] private ServerManager _serverManager;
    [SerializeField] private GameplayManager _gameplayManager;

    private PlayerController _currentPlayer;

    public static MainMenuManager mainMenuManager
    {
        get => instance._mainMenuManager;
    }

    public static LobbyManager lobbyManager
    {
        get => instance._lobbyManager;
    }

    public static PlayerController playerController
    {
        get => instance._currentPlayer;
        set => instance._currentPlayer = value;
    }

    public static ServerManager serverManager
    {
        get => instance._serverManager;
        set => instance._serverManager = value;
    }

    public static GameplayManager gameplayManager
    {
        get => instance._gameplayManager;
        set => instance._gameplayManager = value;
    }

    public static Team GetTeam(int teamId)
    {
        return instance._teamsData[teamId];
    }

    public static int teamsNumber
    {
        get => instance._teamsData.Count;
    }

    public static bool isGameRunning
    {
        get => ((instance._gameplayManager != null && instance._gameplayManager.gameRuning) || (instance._serverManager && instance._serverManager.trackTime));
    }

    public static bool isCopilot
    {
        get => (instance._currentPlayer.playerRole == Player.Role.Copilot);
    }

    private void Awake() {
        if(instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }
}