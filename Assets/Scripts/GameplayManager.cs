using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen, playerScreen, spectatorScreen;
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private CameraFollow cameraManager;
    [SerializeField] private Speedometer speedometer;
    [SerializeField] private TMP_Text starsText, lapsText;
    [SerializeField] public StarSpawner starSpawner;
    [SerializeField] public int totalLaps;
    [SerializeField] public List<CheckpointSingle> checkpointsList;
    [SerializeField] public NotificationManager notificationManager;
    [SerializeField] private Minimap minimapManager;
    [SerializeField] public LeaderboardManager leaderboardManager;
    [SerializeField] public PowerupManager powerupManager;

    private Action GetInputFunction;

    [Serializable]
    struct StartingPos{
        public Vector3 car;
        public Vector3 camera;
    }

    [SerializeField] private StartingPos[] startingPosition;

    public bool gameRuning = false;

    public void Init(Player player, bool isServer)
    {
        loadingScreen.SetActive(true);
        Time.timeScale = 0f;

        if(!isServer){
            // Only the server should know the checkpoints
            Destroy(starSpawner.gameObject);
            checkpointsList.Clear();
        }

        if(player.role == Player.Role.None){
            InitSpectator(player);
        }
        else{
            playerScreen.SetActive(true);
            cameraManager.Init(GameManagers.playerController.car, startingPosition[player.teamId].camera);
            minimapManager.Init(GameManagers.playerController.car.transform);
            UpdateLapNr(1);
            UpdateStars(0);
            if(player.role == Player.Role.Pilot){
                InitPilot(player);
            }
            else{
                InitCopilot(player);
            }
        }

        GameManagers.playerController.PlayerReadyServerRpc();
    }

    private void InitPilot(Player player)
    {
        speedometer.Init(GameManagers.playerController.car.GetComponent<CarManager>());
        GetInputFunction = this.GetPilotInput;
    }

    private void InitCopilot(Player player)
    {
        powerupManager.powerupPanel.SetActive(true);
    }

    private void InitSpectator(Player player)
    {
        spectatorScreen.SetActive(true);
    }

    public void StartGame()
    {
        loadingScreen.SetActive(false);
        Time.timeScale = 1f;
        gameRuning = true;
    }

    public void FinishGame()
    {
        leaderboardManager.leaderboard.SetActive(true);
        playerScreen.SetActive(false);
        minimapManager.gameObject.SetActive(false);
        gameRuning = false;
    }

    public GameObject SpawnCar(int teamId)
    {
        var car = Instantiate(carPrefab, startingPosition[teamId].car, Quaternion.Euler(0, -90f, 0f));
        car.GetComponent<NetworkObject>().Spawn();
        return car;
    }

    public void InitCar(GameObject car, int teamId, bool deleteController)
    {
        car.GetComponent<CarManager>().Init(GameManagers.GetTeam(teamId));
        if(deleteController){
            Destroy(car.GetComponent<Controller>());
            Destroy(car.GetComponentInChildren<WheelsScript>());
        }
    }

    private void FixedUpdate()
    {
        if(!gameRuning){
            return;
        }

        GetInputFunction?.Invoke();
    }

    public void UpdateStars(uint stars)
    {
        starsText.text = "Stars: " + stars;
    }

    public void UpdateLapNr(int lap)
    {
        lapsText.text = "Lap: " + lap + "/" + totalLaps;
    }


#region PilotInput
    // String-uri sa arate mai frumos
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";

    private float horizontalInput, verticalInput;
    private bool isBraking;
    private void GetPilotInput()
    {
        // Fata spate stanga dreapta, the usual
        horizontalInput = Input.GetAxis(HORIZONTAL);
        verticalInput = Input.GetAxis(VERTICAL);

        // Frana de mana
        isBraking = Input.GetKey(KeyCode.Space);

        GameManagers.playerController.PilotInputServerRpc(GameManagers.playerController.OwnerClientId,
            horizontalInput, verticalInput, isBraking
        );
    }
#endregion //PilotInput



}
