using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMap : MonoBehaviour
{
    public static ProceduralMap Instance {get; private set;}
    const int groundSize = 8;
    const float ySpawnPosition = -0.5f;
    [SerializeField]private Transform player;
    [SerializeField]private GameObject groundPrefab;
    [SerializeField]private bool generateObstacles;
    [SerializeField]private Obstacles[] obstacleTypes;
    private Pool<GameObject> groundObjects;
    private Pool<Obstacle> obstacles;
    private Vector2 previousSpawnPosition;
    private SpawnAction currentSpawnAction = SpawnAction.Spawn;

    void Awake()
    {
        if(Instance == null && Instance != this)
        {
            Instance = this;
        }else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitPools();
        InitGround();
        InitObstacles();
    }

    /// <summary>
    /// Initialises a pool of ground GameObjects with a fixed amount
    /// </summary>
    private void InitPools()
    {
        GameObject[] grounds = new GameObject[5];
        for(int i = 0; i < grounds.Length; i++)
        {
            grounds[i] = Instantiate(groundPrefab,new Vector2(0,100), Quaternion.identity);
        }
        
        groundObjects = new Pool<GameObject>(5,grounds);
    }

    private void InitObstacles()
    {
        List<Obstacle> temp = new List<Obstacle>();
        foreach(Obstacles obstacles in obstacleTypes)
        {
            foreach(Spawnable spawnable in obstacles.spawnables)
            {
                Debug.Log($"Creating a pool of obstacle of type {obstacles.type}");
                temp.Add(new Obstacle(obstacles.type,spawnable,CreateMainObstaclePool(spawnable.prefab),CreateFollowUpObjectPool(spawnable.followObjs)));
            }
        }
        obstacles = new Pool<Obstacle>(temp);
    }

    private Pool<GameObject> CreateMainObstaclePool(GameObject prefab)
    {
        List<GameObject> tempPrefabPool = new List<GameObject>();
        for(int i = 0; i < 3; i++)
        {
            GameObject temp = Instantiate(prefab, transform.position, Quaternion.identity);
            tempPrefabPool.Add(temp);
        }
        return new Pool<GameObject>(tempPrefabPool);
    }

    private List<Pool<GameObject>> CreateFollowUpObjectPool(GameObject[] followUpObjects)
    {
        List<Pool<GameObject>> followObjs = new List<Pool<GameObject>>();
        List<GameObject> tempObstacles = new List<GameObject>();
        foreach(GameObject prefabFollowObstacle in followUpObjects)
        {
            for(int i = 0; i < 3; i++)
            {
                GameObject currentFollowUpObstacle = Instantiate(prefabFollowObstacle, transform.position, Quaternion.identity);
                tempObstacles.Add(currentFollowUpObstacle);
            }
            followObjs.Add(new Pool<GameObject>(tempObstacles));
            tempObstacles.Clear();
        }
        return followObjs;
    }


    /// <summary>
    /// Spawns the Initial ground GameObjects the player is on
    /// </summary>
    private void InitGround()
    {
        GameObject middleGround = groundObjects.GetObject();
        Vector2 placePosition = new Vector2(player.position.x,ySpawnPosition);
        middleGround.transform.position = placePosition;

        placePosition = new Vector2(middleGround.transform.position.x - groundSize, ySpawnPosition);
        GameObject leftGround = groundObjects.GetObject();
        leftGround.transform.position = placePosition;

        placePosition = new Vector2(middleGround.transform.position.x + groundSize,ySpawnPosition);
        GameObject rightGround = groundObjects.GetObject();
        rightGround.transform.position = placePosition;
        
        previousSpawnPosition = rightGround.transform.position;
    }

    /// <summary>
    /// Generates Terrain and Obstacles
    /// </summary>
    public void GenerateMap()
    {
        GameObject ground = CreateGround();
        if(!generateObstacles) {return;}
        StartCoroutine(CreateObstacleOptimised(ground));
    }

    /// <summary>
    /// Creates a new ground based on the position of where the previous ground was created
    /// </summary>
    private GameObject CreateGround()
    {
        GameObject currentGround = groundObjects.GetObject();
        Vector2 spawnPos = new Vector2(previousSpawnPosition.x + groundSize, ySpawnPosition);
        currentGround.transform.position = spawnPos;
        previousSpawnPosition = spawnPos;
        return currentGround;
    }

    /// <summary>
    /// Creates a random obstacle from an array
    /// </summary>
    /// <param name="ground"></param>   
    IEnumerator CreateObstacle(GameObject ground)
    {
        if(currentSpawnAction != SpawnAction.Spawn) 
        {
            currentSpawnAction = SpawnAction.Spawn;
            yield break;
        }
        yield return new WaitForSeconds(0.3f);
        int obstacleType = UnityEngine.Random.Range(0,obstacleTypes.Length);
        int obstacleChoice = UnityEngine.Random.Range(0,obstacleTypes[obstacleType].spawnables.Length);
        GameObject firstObstacle = Instantiate(obstacleTypes[obstacleType].spawnables[obstacleChoice].prefab,transform.position,Quaternion.identity);
        Collider2D groundCollider = ground.GetComponent<Collider2D>();
        Collider2D firstObstacleCollider = firstObstacle.GetComponent<Collider2D>();
        // // the value 'centreToBottomDistance is needed in order to place the bottom Y bounds of an obstacle,
        // // onto the top Y bounds of the current ground GameObject that was activated
        float centreToBottomDistance = firstObstacleCollider.transform.position.y - firstObstacleCollider.bounds.min.y; 

        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x,groundCollider.bounds.max.y + centreToBottomDistance);

        firstObstacle.transform.position = obstacleSpawnPos;
        currentSpawnAction = obstacleTypes[obstacleType].spawnables[obstacleChoice].spawnAction;

        if(obstacleTypes[obstacleType].spawnables[obstacleChoice].followObjs.Length > 0 
        && GameManager.Instance.currentGameSpeed >= obstacleTypes[obstacleType].spawnables[obstacleChoice].minimumAcceptableGameSpeedForFollowUp
        && UnityEngine.Random.Range(0,100) <= obstacleTypes[obstacleType].spawnables[obstacleChoice].followObjectChance)
        {
            int secondObstacleChoice = UnityEngine.Random.Range(0,obstacleTypes[obstacleType].spawnables[obstacleChoice].followObjs.Length);
        //     // Debug.Log($"From that type, the object chosen is {obstaclePrefabs[obstacleType].followObjs[secondObstacleType].objects[secondObstacleChoice].name}");
            GameObject secondObstacle = Instantiate(obstacleTypes[obstacleType].spawnables[obstacleChoice].followObjs[secondObstacleChoice],transform.position,Quaternion.identity);

            Collider2D secondObstacleCollider = secondObstacle.GetComponent<Collider2D>();
            float centreToLeft = secondObstacleCollider.transform.position.x - secondObstacleCollider.bounds.min.x;
            centreToBottomDistance = secondObstacleCollider.transform.position.y - secondObstacleCollider.bounds.min.y;
            Vector2 secondObstaclePos = new Vector2(firstObstacle.transform.position.x + firstObstacleCollider.bounds.max.x + centreToLeft + obstacleTypes[obstacleType].spawnables[obstacleChoice].followUpObjectDistance, groundCollider.bounds.max.y + centreToBottomDistance);
            secondObstacle.transform.position = secondObstaclePos;
            Destroy(secondObstacle,10f);
            switch(obstacleTypes[obstacleType].type)
            {
                case ObstacleType.Curb:
                currentSpawnAction = SpawnAction.Skip;
                break;
            }
        }
        
        Destroy(firstObstacle,10f);
    }

    IEnumerator CreateObstacleOptimised(GameObject ground)
    {
        if(currentSpawnAction != SpawnAction.Spawn) 
        {
            currentSpawnAction = SpawnAction.Spawn;
            yield break;
        }
        yield return new WaitForSeconds(0.3f);
        Obstacle current = obstacles.GetObject();
        GameObject mainObstacle = current.GetMainObstacle();
        Collider2D mainObstacleCollider = mainObstacle.GetComponent<Collider2D>();
        Collider2D groundCollider = ground.GetComponent<Collider2D>();

        float centreToBottomDistance = mainObstacleCollider.transform.position.y - mainObstacleCollider.bounds.min.y; 
        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x,groundCollider.bounds.max.y + centreToBottomDistance);

        mainObstacle.transform.position = obstacleSpawnPos;

        if(current.noOfFollowObstacleObjs > 0 && GameManager.Instance.currentGameSpeed >= current.minimumAcceptableGameSpeedForFollowUp
        && UnityEngine.Random.Range(0,100) <= current.followObjectChance)
        {
            GameObject secondObstacle = current.GetFollowUpObstacle(UnityEngine.Random.Range(0,current.noOfFollowObstacleObjs));
            
            Collider2D secondObstacleCollider = secondObstacle.GetComponent<Collider2D>();
            Debug.Log($"Spawning second obstacle {secondObstacle.name}. Initial cordinates: {secondObstacle.transform.position.ToString("F1")}");

            float centreToLeft = secondObstacleCollider.transform.position.x - secondObstacleCollider.bounds.min.x;
            Debug.Log($"From centre to left bounds parameter is {centreToLeft}");
            centreToBottomDistance = secondObstacleCollider.transform.position.y - secondObstacleCollider.bounds.min.y;
            Debug.Log($"The max x bounds of the main obstacle is {mainObstacleCollider.bounds.max.x}");
            Debug.Log($"The distance this follow up object must be from the main obstacle is {current.followUpObjectDistance}");
            Debug.Log($"The main obstacles X position is {mainObstacle.transform.position.x}");
            Debug.Log($"Therefore the position the second obstacle will be spawned at is {mainObstacle.transform.position.x + mainObstacleCollider.bounds.max.x + centreToLeft + current.followUpObjectDistance}");
            Vector2 secondObstaclePos = new Vector2(mainObstacle.transform.position.x + mainObstacleCollider.bounds.max.x + centreToLeft + current.followUpObjectDistance, groundCollider.bounds.max.y + centreToBottomDistance);
            secondObstacle.transform.position = secondObstaclePos;
        }
        Debug.Break();
    }

    /// <summary>
    /// Returns all obstacles from list that can be spawned based on a given game speed.
    /// </summary>
    /// <param name="gameSpeed"></param>
    /// <returns></returns>
    private List<Obstacles> GetObstaclesForCurrentGameSpeed(GameSpeed gameSpeed)
    {
        throw new NotImplementedException();
    }
    
}   
