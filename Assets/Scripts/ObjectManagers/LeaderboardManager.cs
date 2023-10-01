using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] public GameObject leaderboard;
    [SerializeField] private Transform table;
    [SerializeField] private GameObject entryPrefab;

    private Dictionary<int, LeaderboardEntry> entries = new();

    public void AddEntry(int teamId, float time)
    {
        var team = GameManagers.GetTeam(teamId);
        var newEntry = Instantiate(entryPrefab, table).GetComponent<LeaderboardEntry>();
        newEntry.place.text = "#" + (entries.Count + 1).ToString();
        newEntry.team_name.text = team.teamName;
        newEntry.time.text = time.ToString();
        entries.Add(teamId, newEntry);
    } 
    
    public void SetOwnerForEntry(int teamId)
    {
        if(entries.TryGetValue(teamId, out LeaderboardEntry entry)){
            entry.place.color = entry.team_name.color = entry.time.color = Color.yellow;
            entry.place.fontStyle = entry.team_name.fontStyle = entry.time.fontStyle = TMPro.FontStyles.Bold;
        }
    }
}
