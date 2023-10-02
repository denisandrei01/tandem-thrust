using Gtec.UnityInterface;
using System;
using UnityEngine;
using static Gtec.UnityInterface.BCIManager;

public class PowerUpSelectableObject : MonoBehaviour
{
    private uint _selectedClass = 0;
    private bool _update = false;
    void Start()
    {
        //attach to class selection available event
        BCIManager.Instance.ClassSelectionAvailable += OnClassSelectionAvailable;
    }

    void OnApplicationQuit()
    {
        //detach from class selection available event
        BCIManager.Instance.ClassSelectionAvailable -= OnClassSelectionAvailable;
    }

    void Update()
    {
        //TODO ADD YOUR CODE HERE
        if(_update)
        {
            if(_selectedClass >= 1 && _selectedClass <= 3){
                GameManagers.gameplayManager.powerupManager.TryToActivatePowerUp((int)(_selectedClass - 1));
            }
            else if(_selectedClass >= 4 && _selectedClass <= 10){
                GameManagers.gameplayManager.powerupManager.minigames[1].manager.GetComponent<WindshieldCleaner>().TryToCleanInk((int)(_selectedClass - 4));
            }
            else if(_selectedClass >= 11 && _selectedClass <= 13){
                GameManagers.gameplayManager.powerupManager.minigames[2].manager.GetComponent<Bomb>().TryToCutWire((int)(_selectedClass - 11));
            }
            _update = false;
        } 
    }

    /// <summary>
    /// This event is called whenever a new class selection is available. Th
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnClassSelectionAvailable(object sender, EventArgs e)
    {
        ClassSelectionAvailableEventArgs ea = (ClassSelectionAvailableEventArgs)e;
       _selectedClass = ea.Class;
        _update = true;
        Debug.Log(string.Format("Selected class: {0}", ea.Class));
    }
}
