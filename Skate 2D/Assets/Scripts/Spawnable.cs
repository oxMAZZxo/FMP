using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawnable", menuName = "ScriptableObjects/Spawnable", order = 1)]
public class Spawnable : ScriptableObject
{
    public GameObject prefab;
    public GameSpeed minimumAcceptableSpeedForObstacle = GameSpeed.Slow;
    public SpawnAction spawnAction;
    public GameSpeed minimumAcceptableGameSpeedForFollowUp = GameSpeed.Slow;
    public GameObject[] followObjs;
    [Range(1f,100f)]public int followObjectChance = 50;
    public float followUpObjectDistance = 1f;
}
