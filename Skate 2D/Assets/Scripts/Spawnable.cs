using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The spawnable scriptable object is what is used to define the different obstacles that can be spawned as primary obstacles.
/// This is what is used in the Obstacles Scriptable object to define all the types.
/// </summary>
[CreateAssetMenu(fileName = "Spawnable", menuName = "ScriptableObjects/Spawnable", order = 1)]
public class Spawnable : ScriptableObject
{
    public GameObject prefab;
    [Range(0.1f,8f),Tooltip("If an obstacle is within this radius, it will be moved forward on the x this amount as well.")]public float checkRadius = 1f;
    public GameSpeed minimumAcceptableSpeedForObstacle = GameSpeed.Slow;
    public SpawnAction spawnAction;
    public GameSpeed minimumAcceptableGameSpeedForFollowUp = GameSpeed.Slow;
    public SpawnAction followObjectSpawnAction;
    public GameObject[] followObjs;
    [Range(1f,100f)]public int followObjectChance = 50;
    public float followUpObjectDistance = 1f;
}
