using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Team", menuName = "Data/Team", order = 0)]
public class Team : ScriptableObject 
{
    public string teamName;
    public Color color;
    public Texture2D carTexture;
}
