using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Obstacle", menuName = "ScriptableObjects/Obstacle", order = 1)]
public class Obstacle : ScriptableObject
{
    public GameObject[] objects;
    public ObstacleType type;
    public SpawnAction spawnAction;
}

public enum ObstacleType
{
    Curb,
    Bench,
    Rail,
    Kicker,
    Unavoidable
}

public enum SpawnAction
{
    Spawn,
    Skip
}
