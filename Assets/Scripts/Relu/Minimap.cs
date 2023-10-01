using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    private Transform player;

    [SerializeField] private GameObject uiMinimap;

    public void Init(Transform _player)
    {
        player = _player;
        uiMinimap.SetActive(true);
        this.gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        if(GameManagers.gameplayManager == null || !GameManagers.gameplayManager.gameRuning){
            return;
        }

        Vector3 newPos = player.position;
        newPos.y = transform.position.y;
        
        transform.SetPositionAndRotation(newPos, Quaternion.Euler(90f, player.eulerAngles.y, 0f));
    }
}
