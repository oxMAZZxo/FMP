using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unlike the Obstacle Class, this class is a scriptable object which allows the developers to setup the obstacle chains visually, in the inspector.
/// When the game runs, the procedural map, uses theses scriptable objects, to create the different Obstacle instances.
/// </summary>
[CreateAssetMenu(fileName = "Obstacles", menuName = "ScriptableObjects/Obstacles", order = 1)]
public class Obstacles : ScriptableObject
{
    public Spawnable[] spawnables;
    public ObstacleType type;
}

public enum ObstacleType
{
    Bench,
    Rail,
    ManualPad,
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
