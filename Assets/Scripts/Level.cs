using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData_", menuName = "ScriptableObjects/LevelData", order = 1)]
public class Level : ScriptableObject
{
    public int levelID;
    [Tooltip("Width x Length")]
    public Vector2 startingFloorSize;
    public int puzzlesPerWave;
    public int[] waveSize = new int[4];

}
