using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle
{
    public GameObject prefab {get;}
    public float checkRadius {get;}
    public GameSpeed minimumAcceptableSpeedForObstacle {get;}
    public SpawnAction spawnAction {get;}
    public GameSpeed minimumAcceptableGameSpeedForFollowUp {get;}
    public SpawnAction followObjectSpawnAction {get;}
    public int noOfFollowObstacleObjs {get;}
    public int followObjectChance {get;}
    public float followUpObjectDistance {get;}
    public ObstacleType obstacleType {get;}
    // Holds a pool of instantiated gameobjects from the prefab obstacle
    private Pool<GameObject> mainObstaclePool;
    // Holds Pools of each follow up object instantiated from the prefab follow objects
    private List<Pool<GameObject>> followObstaclePools;

    public Obstacle(ObstacleType newObstacleType,Spawnable spawnable, Pool<GameObject> newMainObstaclePool, List<Pool<GameObject>> newFollowObstaclePools)
    {
        prefab = spawnable.prefab;
        minimumAcceptableSpeedForObstacle = spawnable.minimumAcceptableSpeedForObstacle;
        minimumAcceptableGameSpeedForFollowUp = spawnable.minimumAcceptableGameSpeedForFollowUp;
        spawnAction = spawnable.spawnAction;
        noOfFollowObstacleObjs = spawnable.followObjs.Length;
        followUpObjectDistance = spawnable.followUpObjectDistance;
        followObjectChance = spawnable.followObjectChance;
        obstacleType = newObstacleType;
        mainObstaclePool = newMainObstaclePool;
        followObstaclePools = newFollowObstaclePools;
        checkRadius = spawnable.checkRadius;
        followObjectSpawnAction = spawnable.followObjectSpawnAction;
    }

    /// <returns>Returns an obstacle instantiated from the prefab</returns>
    public GameObject GetMainObstacle()
    {
        return mainObstaclePool.GetObject();
    }

    public GameObject GetFollowUpObstacle(int index)
    {
        return followObstaclePools[index].GetObject();
    }
}
