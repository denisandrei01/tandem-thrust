using System;
using System.Collections.Generic;
using Gtec.UnityInterface;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] public GameObject loadingScreen, playerScreen, spectatorScreen;
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
    [SerializeField] public CountDown countDown;
    [SerializeField] private BCIManager2D bCIManager;

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

        if(!isServer){
            // Only the server should know the checkpoints
            Destroy(starSpawner.gameObject);
            checkpointsList.Clear();
        }

        if(player.role == Player.Role.None){
            InitSpectator(player);
        }
        else{
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
        playerScreen.SetActive(true);
        speedometer.Init(GameManagers.playerController.car.GetComponent<CarManager>());
        GetInputFunction = this.GetPilotInput;
    }

    private void InitCopilot(Player player)
    {
        
    }

    public void Calibration()
    {
        if(GameManagers.isCopilot){
            loadingScreen.SetActive(false);
            bCIManager.gameObject.SetActive(true);
        }
        else{
            GameManagers.playerController.PlayerReadyAfterCalibrationServerRpc();
        }
    }

    public void SkipCalibration()
    {
        GameManagers.playerController.PlayerReadyAfterCalibrationServerRpc();
        bCIManager.gameObject.SetActive(false);
    }

    public void FinishCalibration()
    {
        loadingScreen.SetActive(true);
        playerScreen.SetActive(true);
        powerupManager.powerupPanel.SetActive(true);
        GameManagers.playerController.PlayerReadyAfterCalibrationServerRpc();
    }

    private void InitSpectator(Player player)
    {
        spectatorScreen.SetActive(true);
    }

    public void StartGame()
    {
        loadingScreen.SetActive(false);
        gameRuning = true;
    }

    public void FinishGame()
    {
        leaderboardManager.leaderboard.SetActive(true);
        playerScreen.SetActive(false);
        powerupManager.powerupPanel.SetActive(false);
        minimapManager.gameObject.SetActive(false);
        gameRuning = false;
    }

    public GameObject SpawnCar(int teamId)
    {
        var car = Instantiate(carPrefab, startingPosition[teamId].car, Quaternion.Euler(0, -90f, 0f));
        car.GetComponent<NetworkObject>().Spawn();
        car.GetComponent<CarManager>().SpawnWheels(car.GetComponent<Controller>());
        return car;
    }

    public void InitCar(GameObject car, int teamId, bool deleteController)
    {
        car.GetComponent<CarManager>().Init(GameManagers.GetTeam(teamId));
        if(deleteController){
            StartCoroutine(car.GetComponent<CarManager>().FakeCar());
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
        if(Input.GetKeyDown(KeyCode.H))
        {
            GameManagers.playerController.TryToResetCheckpointServerRpc(GameManagers.playerController.OwnerClientId);
        }
    }
#endregion //PilotInput



}
