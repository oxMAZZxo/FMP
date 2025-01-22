using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Obstacle", menuName = "ScriptableObjects/Obstacle", order = 1)]
public class Spawnable : ScriptableObject
{
    public GameObject[] objects;
    public ObjectType type;
    public SpawnAction spawnAction;
    [Range(1f,100f)]public int followObjectChance = 50;
    public Spawnable[] followObjs;
}

public enum ObjectType
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
