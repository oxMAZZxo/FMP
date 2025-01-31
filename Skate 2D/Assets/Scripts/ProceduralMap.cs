using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    [SerializeField]private LayerMask whatIsObstacle;
    [SerializeField,Range(0.1f,3f)]private float obstacleCheckRadius = 1f;
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
        InitObstaclePool();
        InitGround();
    }

    /// <summary>
    /// Initialises a pool of ground GameObjects with a fixed amount
    /// </summary>
    private void InitPools()
    {
        GameObject[] grounds = new GameObject[4];
        for(int i = 0; i < grounds.Length; i++)
        {
            grounds[i] = Instantiate(groundPrefab,new Vector2(0,100), Quaternion.identity);
        }
        
        groundObjects = new Pool<GameObject>(4,grounds);
    }

    private void InitObstaclePool()
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
        for(int i = 0; i < 2; i++)
        {
            GameObject temp = Instantiate(prefab, new Vector2(0,100), Quaternion.identity);
            temp.name += i.ToString();
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
            for(int i = 0; i < 2; i++)
            {
                GameObject currentFollowUpObstacle = Instantiate(prefabFollowObstacle, new Vector2(0,100), Quaternion.identity);
                currentFollowUpObstacle.name += i.ToString();
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
        ClearConsole();

        StartCoroutine(CreateObstacleOptimised(ground));
    }

    void ClearConsole()
    {
        Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor");
        MethodInfo clearMethod = logEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
        clearMethod?.Invoke(null, null);
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

        float centreToBottomDistance = firstObstacleCollider.transform.position.y - firstObstacleCollider.bounds.min.y; 

        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x,groundCollider.bounds.max.y + centreToBottomDistance);

        firstObstacle.transform.position = obstacleSpawnPos;
        currentSpawnAction = obstacleTypes[obstacleType].spawnables[obstacleChoice].spawnAction;

        if(obstacleTypes[obstacleType].spawnables[obstacleChoice].followObjs.Length > 0 
        && GameManager.Instance.currentGameSpeed >= obstacleTypes[obstacleType].spawnables[obstacleChoice].minimumAcceptableGameSpeedForFollowUp
        && UnityEngine.Random.Range(0,100) <= obstacleTypes[obstacleType].spawnables[obstacleChoice].followObjectChance)
        {
            int secondObstacleChoice = UnityEngine.Random.Range(0,obstacleTypes[obstacleType].spawnables[obstacleChoice].followObjs.Length);

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

        Obstacle currentObstacleTypeChoice = obstacles.GetObject();
        GameObject mainObstacle = currentObstacleTypeChoice.GetMainObstacle();
        
        Collider2D mainObstacleCollider = mainObstacle.GetComponent<Collider2D>();
        mainObstacle.transform.position = Vector3.zero;
        Physics2D.SyncTransforms();

        Collider2D groundCollider = ground.GetComponent<Collider2D>();

        float obstacleBottomBoundsPosition = mainObstacleCollider.bounds.center.y - mainObstacleCollider.bounds.extents.y; 

        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x,groundCollider.bounds.center.y + groundCollider.bounds.extents.y - obstacleBottomBoundsPosition);

        mainObstacle.transform.position = obstacleSpawnPos;
        Physics2D.SyncTransforms();
        if(CheckForPreviousObjectNear(mainObstacle))
        {
            Debug.Log($"Moving {mainObstacle.name} 3 meters to the right");
            obstacleSpawnPos = new Vector2(ground.transform.position.x + 3,groundCollider.bounds.center.y + groundCollider.bounds.extents.y - obstacleBottomBoundsPosition);
            mainObstacle.transform.position = obstacleSpawnPos;
            Physics2D.SyncTransforms();
        }
        if(GameManager.Instance.currentGameSpeed != GameSpeed.Slow)
        {
            currentSpawnAction = currentObstacleTypeChoice.spawnAction;
        }
        if(currentObstacleTypeChoice.noOfFollowObstacleObjs > 0 && GameManager.Instance.currentGameSpeed >= currentObstacleTypeChoice.minimumAcceptableGameSpeedForFollowUp
        && UnityEngine.Random.Range(0,100) <= currentObstacleTypeChoice.followObjectChance)
        {
            GameObject secondObstacle = currentObstacleTypeChoice.GetFollowUpObstacle(UnityEngine.Random.Range(0,currentObstacleTypeChoice.noOfFollowObstacleObjs));
            Collider2D secondObstacleCollider = secondObstacle.GetComponent<Collider2D>();

            secondObstacle.transform.position = Vector3.zero;
            Physics2D.SyncTransforms();

            obstacleBottomBoundsPosition = secondObstacleCollider.bounds.center.y - secondObstacleCollider.bounds.extents.y; 
            
            float mainToSecondSideToSideDistance = mainObstacleCollider.bounds.extents.x + secondObstacleCollider.bounds.extents.x;
            
            Vector2 secondObstaclePos = new Vector2(mainObstacle.transform.position.x + mainToSecondSideToSideDistance + currentObstacleTypeChoice.followUpObjectDistance, groundCollider.bounds.center.y + groundCollider.bounds.extents.y - obstacleBottomBoundsPosition);
            secondObstacle.transform.position = secondObstaclePos;
            Physics2D.SyncTransforms();
        }
    }

    private bool CheckForPreviousObjectNear(GameObject obstacle)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(obstacle.transform.position, obstacleCheckRadius, whatIsObstacle);
        
        foreach(Collider2D collider in colliders)
        {
            if(collider.gameObject != obstacle)
            {
                Debug.Log("There's an obstacle near the current obstacle position");
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position,obstacleCheckRadius);
    }    
}   
