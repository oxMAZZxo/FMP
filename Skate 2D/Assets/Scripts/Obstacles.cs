using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Obstacles", menuName = "ScriptableObjects/Obstacles", order = 1)]
public class Obstacles : ScriptableObject
{
    public Spawnable[] spawnables;
    public ObstacleType type;
}

public enum ObstacleType
{
    Curb,
    Bench,
    Rail,
    Kicker,
    Pyramid,
    Unavoidable,
    Bins,
    Other
}

public enum SpawnAction
{
    Spawn,
    Skip
}
