using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountDown : MonoBehaviour
{
    [SerializeField] private TMP_Text PanelCountdown;    
    public void StartCountDown()
    {
        Debug.Log("in start countdown");
        PanelCountdown.text = "3";
        StartCoroutine(CountDownRoutine());
    }

    IEnumerator CountDownRoutine()
    {
        int counter = 3;
        PanelCountdown.gameObject.SetActive(true);
        while(counter > 0) {
            PanelCountdown.text = counter.ToString();

            yield return new WaitForSeconds(1);
            counter--;
        }
        PanelCountdown.gameObject.SetActive(false);
        GameManagers.gameplayManager.StartGame();
        this.gameObject.SetActive(false);
    }
}
