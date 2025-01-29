using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public GameObject prefab {get;}
    public GameSpeed minimumAcceptableSpeedForObstacle {get;}
    public SpawnAction spawnAction {get;}
    public GameSpeed minimumAcceptableGameSpeedForFollowUp {get;}
    public GameObject[] prefabFollowObjs {get;}
    public int followObjectChance {get;}
    public float followUpObjectDistance {get;}
    public ObstacleType obstacleType {get;}
    // Holds a pool of instantiated gameobjects from the prefab obstacle
    private Pool<GameObject> mainObstaclePool;
    // Holds Pools of each follow up object instantiated from the prefab follow objects
    private List<Pool<GameObject>> followObstaclePools;

    /// <summary>
    /// Instantiates an obstacle with the given parameters
    /// </summary>
    /// <param name="newPrefab"></param>
    /// <param name="newMinAcceptSpeed"></param>
    /// <param name="newSpawnAction"></param>
    /// <param name="newMinAcceptSpeedForFollowUp"></param>
    /// <param name="newFollowObjs"></param>
    /// <param name="newFollowObjectChance"></param>
    /// <param name="newFollowUpObjectDistance"></param>
    public Obstacle(ObstacleType newObstacleType,GameObject newPrefab, GameSpeed newMinAcceptSpeed,SpawnAction newSpawnAction, GameSpeed newMinAcceptSpeedForFollowUp, GameObject[] newFollowObjs = null, int newFollowObjectChance = 0,float newFollowUpObjectDistance = 0)
    {
        prefab = newPrefab;
        minimumAcceptableSpeedForObstacle = newMinAcceptSpeed;
        spawnAction = newSpawnAction;
        minimumAcceptableGameSpeedForFollowUp = newMinAcceptSpeedForFollowUp;
        prefabFollowObjs = newFollowObjs;
        followObjectChance = newFollowObjectChance;
        followUpObjectDistance = newFollowUpObjectDistance;
        obstacleType = newObstacleType;
        InitPools();
    }

    /// <summary>
    /// Initialises Pools
    /// </summary>
    private void InitPools()
    {
        InitialiseMainObstaclePool();
        InitialiseFollowUpObstaclePools();
    }

    private void InitialiseMainObstaclePool()
    {
        List<GameObject> tempPrefabPool = new List<GameObject>();

        for(int i = 0; i < 3; i++)
        {
            GameObject temp = Instantiate(prefab, new Vector3(0,0,0),Quaternion.identity);
            temp.SetActive(false);
            tempPrefabPool.Add(temp);
        }
        mainObstaclePool = new Pool<GameObject>(tempPrefabPool);
    }

    private void InitialiseFollowUpObstaclePools()
    {
        followObstaclePools = new List<Pool<GameObject>>();
        List<GameObject> tempObstacles = new List<GameObject>();
        foreach(GameObject prefabFollowObstacle in prefabFollowObjs)
        {
            for(int i = 0; i < 3; i++)
            {
                
            }
        }
    }

    /// <returns>Returns an obstacle instantiated from the prefab</returns>
    private GameObject GetObstacle()
    {
        return mainObstaclePool.GetObject();
    }
}
