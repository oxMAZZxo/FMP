using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The obstacle class is a runtime instantiated object, which the Procedural Map uses to generate obstacles dynamicly.
/// This class holds information which is vital to this process, such as the speed an instance of an obstacle can be spawned at, or the distance between the main obstacle and the follow up obstacles.
/// </summary>
public class Obstacle
{
    private Sprite[] skins;
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
    private List<Pool<PickUp>> pickUpPools;
    private int pickUpSpawnChances = 20;
    private Vector3 pickUpSpawnOffset;

    /// <summary>
    /// Instantiate a runtime Obstacle, which holds pools of its main obstacle, and a secondary follow up obstacle/obstacles.
    /// </summary>
    /// <param name="newObstacleType"></param>
    /// <param name="spawnable"></param>
    /// <param name="newMainObstaclePool"></param>
    /// <param name="newFollowObstaclePools"></param>
    public Obstacle(ObstacleType newObstacleType,Spawnable spawnable, Pool<GameObject> newMainObstaclePool, 
    List<Pool<GameObject>> newFollowObstaclePools, List<Pool<PickUp>> newPickUpPools)
    {
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

        if(spawnable.alternativeSkins.Length > 0)
        {
            skins = new Sprite[spawnable.alternativeSkins.Length + 1];
            for(int i = 0; i < spawnable.alternativeSkins.Length; i++)
            {
                skins[i] = spawnable.alternativeSkins[i];
            }
            skins[skins.Length -1] = spawnable.prefab.GetComponent<SpriteRenderer>().sprite;
            // Debug.Log($"Skins available for {obstacleType.ToString()} are {skins.Length}");
            // for(int i = 0; i < skins.Length; i++)
            // {
            //     Debug.Log($"{i + 1}. {skins[i].name}");
            // }
        }
        
    }

    /// <returns>Returns an obstacle instantiated from the prefab</returns>
    public GameObject GetMainObstacle()
    {
        GameObject currentObj = mainObstaclePool.GetObject();
        if(skins != null && skins.Length > 0)
        {
            SpriteRenderer currentRenderer = currentObj.GetComponent<SpriteRenderer>();
            currentRenderer.sprite = skins[Random.Range(0, skins.Length)];
        }
        return currentObj;
    }

    /// <summary>
    /// Roll back main obstacle pool to previous object
    /// </summary>
    public void RollBackMainObstacle()
    {
        mainObstaclePool.RollBack();
    }

    /// <summary>
    /// Roll back second obstacle pool to previous object
    /// </summary>
    /// <param name="index">The index of the second obstacle pool</param>
    public void RollBackFollowUpObstacle(int index)
    {
        followObstaclePools[index].RollBack();
    }

    public GameObject GetFollowUpObstacle(int index)
    {
        return followObstaclePools[index].GetObject();
    }

    /// <returns>The main obstacle pool as an array.</returns>
    public GameObject[] GetAllMainObstacles()
    {
        return mainObstaclePool.GetObjects();
    }

    /// <param name="index">The index of the item in the list of pools</param>
    /// <returns>Pool of follow up obstacles from a list using the given index</returns>
    public GameObject[] GetFollowUpObjectsAt(int index)
    {
        return followObstaclePools[index].GetObjects();
    }

    public int GetCountOfFollowUpObstacles() {return followObstaclePools.Count;}

    public void SpawnPickUp(GameObject obstacle)
    {
        if(Random.Range(0,100) > pickUpSpawnChances) {return;}
        int pickUpChoice = Random.Range(0,pickUpPools.Count);
        PickUp current = pickUpPools[pickUpChoice].GetObject();
        current.transform.position = obstacle.transform.position + pickUpSpawnOffset;
    }

    
}
