using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    private bool isActive = false;
    
    // This should support multiple nottifications at once
    // Create a queue to store the notifications. When One is finish the next one Pops up
    // For now, for simplicity, will support only one notification at a time

    public void ShowNotification(string message) // add also a severity level
    {
        text.text = message;
        InvokeRepeating(nameof(ToggleObjectState), 1.0f, 1.0f);
    }

    private void ToggleObjectState()
    {
        isActive = !isActive;
        text.transform.parent.gameObject.SetActive(isActive);
    }

    public void DiscardNotification(string message) // add also a severity level
    {
        CancelInvoke(nameof(ToggleObjectState));
        isActive = false;
        text.transform.parent.gameObject.SetActive(false);
    }
}
