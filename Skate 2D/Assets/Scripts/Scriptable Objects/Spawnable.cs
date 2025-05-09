using UnityEngine;

/// <summary>
/// The spawnable scriptable object is what is used to define the different obstacles that can be spawned as primary obstacles.
/// This is what is used in the Obstacles Scriptable object to define all the types.
/// </summary>
[CreateAssetMenu(fileName = "Spawnable", menuName = "ScriptableObjects/Spawnable", order = 1)]
public class Spawnable : ScriptableObject
{
    [Header("Obstacle")]
    public GameObject prefab;
    public Sprite[] alternativeSkins;
    [Range(0.1f,8f),Tooltip("If an obstacle is within this radius, it will be moved forward on the x this amount as well.")]public float checkRadius = 1f;
    public GameSpeed minimumAcceptableSpeedForObstacle = GameSpeed.Slow;
    public SpawnAction spawnAction;
    [Header("Follow Obstacles")]
    public GameSpeed minimumAcceptableGameSpeedForFollowUp = GameSpeed.Slow;
    public SpawnAction followObjectSpawnAction;
    public GameObject[] followObjs;
    [Range(1f,100f)]public int followObjectChance = 50;
    public float followUpObjectDistance = 1f;
    [Header("Pick Ups")]
    public GameObject[] pickUps;
    [Range(1,100)]public int pickUpSpawnChances = 20;
    public Vector2 spawnOffset;
}
