using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralMap : MonoBehaviour
{
    public static ProceduralMap Instance {get; private set;}
    const float ySpawnPosition = -0.5f;
    [SerializeField]private Transform player;
    [SerializeField]private GameObject groundPrefab;
    [SerializeField]private bool generateObstacles;
    [SerializeField]private Obstacles[] obstacleTypes;
    [SerializeField]private LayerMask whatIsObstacle;
    private Pool<GameObject> groundObjects;
    private Pool<Obstacle> obstacles;
    private Collider2D previousGround;
    private SpawnAction currentSpawnAction = SpawnAction.Spawn;
    private int lastSlowObstacleIndex;
    private int lastMediumObstacleIndex;
    private int lastFastObstacleIndex;

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
        GameObject[] grounds = new GameObject[5];
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
                temp.Add(new Obstacle(obstacles.type,spawnable,CreateMainObstaclePool(spawnable.prefab),CreateFollowUpObjectPool(spawnable.followObjs)));
            }
        }
        temp = temp.OrderBy(o => o.minimumAcceptableSpeedForObstacle).ToList();
        lastSlowObstacleIndex = temp.FindLastIndex(o => o.minimumAcceptableSpeedForObstacle == GameSpeed.Slow);
        lastMediumObstacleIndex = temp.FindLastIndex(o => o.minimumAcceptableSpeedForObstacle == GameSpeed.Medium);
        lastFastObstacleIndex = temp.FindLastIndex(o => o.minimumAcceptableSpeedForObstacle == GameSpeed.Fast);
        obstacles = new Pool<Obstacle>(temp);
    }

    private Pool<GameObject> CreateMainObstaclePool(GameObject prefab)
    {
        List<GameObject> tempPrefabPool = new List<GameObject>();
        for(int i = 0; i < 3; i++)
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

        placePosition = new Vector2(middleGround.transform.position.x - middleGround.GetComponent<Collider2D>().bounds.extents.x, ySpawnPosition);
        GameObject leftGround = groundObjects.GetObject();
        leftGround.transform.position = placePosition;

        placePosition = new Vector2(middleGround.transform.position.x + middleGround.GetComponent<Collider2D>().bounds.extents.x,ySpawnPosition);
        GameObject rightGround = groundObjects.GetObject();
        rightGround.transform.position = placePosition;
        
        previousGround = rightGround.GetComponent<Collider2D>();
        Physics2D.SyncTransforms();
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
        Collider2D currentGourndCollider = currentGround.GetComponent<Collider2D>();
        float previousGroundRightBounds = previousGround.gameObject.transform.position.x + previousGround.bounds.extents.x;
        Vector2 spawnPos = new Vector2(previousGroundRightBounds + currentGourndCollider.bounds.extents.x, ySpawnPosition);
        currentGround.transform.position = spawnPos;
        Physics2D.SyncTransforms();
        previousGround = currentGourndCollider;
        return currentGround;
    }

    /// <summary>
    /// Creates a random obstacle from an array. THIS FUNCTION IS UNOPTIMISED
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
        int obstacleType = Random.Range(0,obstacleTypes.Length);
        int obstacleChoice = Random.Range(0,obstacleTypes[obstacleType].spawnables.Length);
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
            int secondObstacleChoice = Random.Range(0,obstacleTypes[obstacleType].spawnables[obstacleChoice].followObjs.Length);

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

    /// <summary>
    /// Creates a random obstacle from the pools.
    /// </summary>
    /// <param name="ground">The current ground</param>
    /// <returns></returns>
    IEnumerator CreateObstacleOptimised(GameObject ground)
    {
        if(currentSpawnAction != SpawnAction.Spawn) 
        {
            currentSpawnAction = SpawnAction.Spawn;
            yield break;
        }
        yield return new WaitForSeconds(0.3f);
        
        int maxIndex;
        switch (GameManager.Instance.currentGameSpeed)
        {
            case GameSpeed.Slow: maxIndex = lastSlowObstacleIndex; break;
            case GameSpeed.Medium: maxIndex = lastMediumObstacleIndex; break;
            case GameSpeed.Fast: maxIndex = lastFastObstacleIndex; break;
            default: maxIndex = obstacles.length - 1; break;
        }

        Obstacle currentObstacleTypeChoice = obstacles.GetRandomObject(maxIndex + 1);
        GameObject mainObstacle = currentObstacleTypeChoice.GetMainObstacle();
        
        Collider2D mainObstacleCollider = mainObstacle.GetComponent<Collider2D>();
        mainObstacle.transform.position = Vector3.zero;
        Physics2D.SyncTransforms();

        Collider2D groundCollider = ground.GetComponent<Collider2D>();

        float obstacleBottomBoundsPosition = mainObstacleCollider.bounds.center.y - mainObstacleCollider.bounds.extents.y; 

        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x,groundCollider.bounds.center.y + groundCollider.bounds.extents.y - obstacleBottomBoundsPosition);

        mainObstacle.transform.position = obstacleSpawnPos;
        Physics2D.SyncTransforms();
        
        if(CheckForPreviousObjectNear(mainObstacleCollider,currentObstacleTypeChoice.checkRadius))
        {
            MoveObstacle(currentObstacleTypeChoice,mainObstacle,groundCollider,obstacleBottomBoundsPosition);
        }

        if(GameManager.Instance.currentGameSpeed != GameSpeed.Slow)
        {
            currentSpawnAction = currentObstacleTypeChoice.spawnAction;
        }
        if(currentObstacleTypeChoice.noOfFollowObstacleObjs > 0 && GameManager.Instance.currentGameSpeed >= currentObstacleTypeChoice.minimumAcceptableGameSpeedForFollowUp
        && Random.Range(0,100) <= currentObstacleTypeChoice.followObjectChance)
        {
            GameObject secondObstacle = currentObstacleTypeChoice.GetFollowUpObstacle(Random.Range(0,currentObstacleTypeChoice.noOfFollowObstacleObjs));
            Collider2D secondObstacleCollider = secondObstacle.GetComponent<Collider2D>();

            secondObstacle.transform.position = Vector3.zero;
            Physics2D.SyncTransforms();

            obstacleBottomBoundsPosition = secondObstacleCollider.bounds.center.y - secondObstacleCollider.bounds.extents.y; 
            
            float mainToSecondSideToSideDistance = mainObstacleCollider.bounds.extents.x + secondObstacleCollider.bounds.extents.x;
            
            Vector2 secondObstaclePos = new Vector2(mainObstacle.transform.position.x + mainToSecondSideToSideDistance + currentObstacleTypeChoice.followUpObjectDistance, groundCollider.bounds.center.y + groundCollider.bounds.extents.y - obstacleBottomBoundsPosition);
            secondObstacle.transform.position = secondObstaclePos;
            Physics2D.SyncTransforms();
            currentSpawnAction = currentObstacleTypeChoice.followObjectSpawnAction;
        }
    }

    private bool CheckForPreviousObjectNear(Collider2D obstacle,float checkRadius)
    {
        Vector3 checkPos = new Vector3(obstacle.transform.position.x - obstacle.bounds.extents.x,obstacle.transform.position.y,obstacle.transform.position.z);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPos, checkRadius, whatIsObstacle);
        
        foreach(Collider2D collider in colliders)
        {
            if(collider != obstacle)
            {
                return true;
            }
        }

        return false;
    }  

    private void MoveObstacle(Obstacle currentObstacle, GameObject mainObstacle, Collider2D ground, float obstacleBottomBoundsPosition)
    {
        if(currentObstacle.obstacleType == ObstacleType.Unavoidable)
        {
            mainObstacle.transform.position = Vector3.zero;
            Physics2D.SyncTransforms();
        }else
        {
            
            mainObstacle.transform.position = new Vector2(ground.gameObject.transform.position.x + currentObstacle.checkRadius -1,ground.bounds.center.y + ground.bounds.extents.y - obstacleBottomBoundsPosition);;
            Physics2D.SyncTransforms();

        }
    }
}   
