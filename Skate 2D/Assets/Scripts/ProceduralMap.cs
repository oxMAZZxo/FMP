using System;
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
    [SerializeField]private Collider2D startGround;
    [Header("Obstacle Generation")]
    [SerializeField]private bool generateObstacles;
    [SerializeField]private Obstacles[] obstacleTypes;
    [SerializeField]private LayerMask whatIsObstacle;
    [Header("Combo Rush")]
    [SerializeField]private bool comboRush = false;
    [SerializeField,Range(1f,10f),Tooltip("How long can the comboRushLast")]private float comboRushDuration;
    [Header("Background Environment Generation")]
    [SerializeField]private bool generateEnvironment;
    [SerializeField]private GameObject[] environmentPrefabs;
    private Pool<GameObject> groundObjects;
    private Pool<Obstacle> obstacles;
    private Pool<Obstacle> grindableObstacles;
    private Collider2D previousGround;
    private Collider2D previousEnvironment;
    private SpawnAction currentSpawnAction = SpawnAction.Spawn;
    private int lastSlowObstacleIndex;
    private int lastMediumObstacleIndex;
    private int lastFastObstacleIndex;
    private float distanceToAddToFollowUp;

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
        // InitGround();
        if(startGround == null)
        {
            Debug.LogError("The variable Start Ground in the Procedural Map Component is null. Cannot generate ground, obstacles or environment without that");
        }else
        {
            previousGround = startGround;
        }
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
        List<Obstacle> grindableTemp = new List<Obstacle>();
        foreach(Obstacles obstacles in obstacleTypes)
        {
            foreach(Spawnable spawnable in obstacles.spawnables)
            {
                Obstacle current = new Obstacle(obstacles.type,spawnable,CreateMainObstaclePool(spawnable.prefab),CreateFollowUpObjectPool(spawnable.followObjs));
                temp.Add(current);
                if(current.obstacleType == ObstacleType.Bench || current.obstacleType == ObstacleType.Rail)
                {
                    grindableTemp.Add(current);
                }
            }
        }
        temp = temp.OrderBy(o => o.minimumAcceptableSpeedForObstacle).ToList();
        lastSlowObstacleIndex = temp.FindLastIndex(o => o.minimumAcceptableSpeedForObstacle == GameSpeed.Slow);
        lastMediumObstacleIndex = temp.FindLastIndex(o => o.minimumAcceptableSpeedForObstacle == GameSpeed.Medium);
        lastFastObstacleIndex = temp.FindLastIndex(o => o.minimumAcceptableSpeedForObstacle == GameSpeed.Fast);
        obstacles = new Pool<Obstacle>(temp);
        grindableObstacles = new Pool<Obstacle>(grindableTemp);
    }

    private Pool<GameObject> CreateMainObstaclePool(GameObject prefab)
    {
        List<GameObject> tempPrefabPool = new List<GameObject>();
        for(int i = 0; i < 3; i++)
        {
            GameObject temp = Instantiate(prefab, new Vector2(0,100), Quaternion.identity);
            temp.name += i.ToString();
            tempPrefabPool.Add(temp);
            temp.SetActive(false);
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
                currentFollowUpObstacle.SetActive(false);

            }
            followObjs.Add(new Pool<GameObject>(tempObstacles));
            tempObstacles.Clear();
        }
        return followObjs;
    }

    /// <summary>
    /// Generates Terrain and Obstacles
    /// </summary>
    public void GenerateMap()
    {
        GameObject ground = CreateGround();
        if(generateObstacles) {StartCoroutine(CreateObstacle(ground));}
        
        if(generateEnvironment) {CreateBuildings(ground.GetComponent<Collider2D>());}

    }
    
    /// <summary>
    /// Creates a new ground based on the position of where the previous ground was created
    /// </summary>
    private GameObject CreateGround()
    {
        GameObject currentGround = groundObjects.GetObject();
        currentGround.GetComponentInChildren<GroundTrigger>().Reset();
        Collider2D currentGourndCollider = currentGround.GetComponent<Collider2D>();
        float previousGroundRightBounds = previousGround.gameObject.transform.position.x + previousGround.bounds.extents.x;
        Vector2 spawnPos = new Vector2(previousGroundRightBounds + currentGourndCollider.bounds.extents.x, ySpawnPosition);
        currentGround.transform.position = spawnPos;
        Physics2D.SyncTransforms();
        previousGround = currentGourndCollider;
        return currentGround;
    }

    private void CreateBuildings(Collider2D ground)
    {
        Collider2D currentEnvironemnt = Instantiate(environmentPrefabs[UnityEngine.Random.Range(0,environmentPrefabs.Length)],transform.position,Quaternion.identity).GetComponent<Collider2D>();
        currentEnvironemnt.transform.position = Vector3.zero;
        Physics2D.SyncTransforms();

        float centreToBottomDistance = currentEnvironemnt.bounds.center.y - currentEnvironemnt.bounds.extents.y; 
        float mainToSecondSideToSideDistance = 0;
        float currentXPosition = ground.transform.position.x;
        if(previousEnvironment != null)
        {
            mainToSecondSideToSideDistance = previousEnvironment.bounds.extents.x + currentEnvironemnt.bounds.extents.x;
            currentXPosition = previousEnvironment.transform.position.x;
        }

        Vector2 newPos = new Vector2(currentXPosition + mainToSecondSideToSideDistance,ground.bounds.center.y + ground.bounds.extents.y - centreToBottomDistance + 0.28f);
        currentEnvironemnt.transform.position = newPos;
        Physics2D.SyncTransforms();
        previousEnvironment = currentEnvironemnt;
    }

    /// <summary>
    /// Creates a random obstacle from an array. THIS FUNCTION IS UNOPTIMISED
    /// </summary>
    /// <param name="ground"></param>   
    IEnumerator CreateObstacleNonOptimised(GameObject ground)
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
                case ObstacleType.ManualPad:
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
    IEnumerator CreateObstacle(GameObject ground)
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
        Pool<Obstacle> obstaclePoolToChooseFrom = obstacles;
        if(comboRush) {obstaclePoolToChooseFrom = grindableObstacles; maxIndex = grindableObstacles.length - 1;}

        Obstacle currentObstacleTypeChoice = obstaclePoolToChooseFrom.GetRandomObject(maxIndex + 1);
        GameObject mainObstacle = currentObstacleTypeChoice.GetMainObstacle();
        
        Collider2D mainObstacleCollider = mainObstacle.GetComponent<Collider2D>();
        mainObstacle.transform.position = Vector3.zero;
        Physics2D.SyncTransforms();

        if(!mainObstacle.activeInHierarchy) {mainObstacle.SetActive(true);}
        
        Collider2D groundCollider = ground.GetComponent<Collider2D>();

        float obstacleBottomBoundsPosition = mainObstacleCollider.bounds.center.y - mainObstacleCollider.bounds.extents.y; 

        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x,groundCollider.bounds.center.y + groundCollider.bounds.extents.y - obstacleBottomBoundsPosition);

        mainObstacle.transform.position = obstacleSpawnPos;
        Physics2D.SyncTransforms();
        
        bool obstacleInTheWay = CheckForPreviousObjectNear(mainObstacleCollider,currentObstacleTypeChoice.checkRadius);
        bool secondObstacleElligible = true;
        if(obstacleInTheWay)
        {
            secondObstacleElligible = HandleObstacle(currentObstacleTypeChoice,mainObstacle,groundCollider,obstacleBottomBoundsPosition);
        }

        currentSpawnAction = currentObstacleTypeChoice.spawnAction;
        if(currentObstacleTypeChoice.obstacleType == ObstacleType.Unavoidable && GameManager.Instance.currentGameSpeed == GameSpeed.Slow) {currentSpawnAction = SpawnAction.Spawn;}
        
        if(currentObstacleTypeChoice.noOfFollowObstacleObjs > 0 && secondObstacleElligible && ((GameManager.Instance.currentGameSpeed >= currentObstacleTypeChoice.minimumAcceptableGameSpeedForFollowUp
        && UnityEngine.Random.Range(0,100) <= currentObstacleTypeChoice.followObjectChance) || comboRush))
        {
            CreateSecondObstacle(currentObstacleTypeChoice,mainObstacleCollider,groundCollider);
        }
    }

    private void CreateSecondObstacle(Obstacle currentObstacleTypeChoice,Collider2D mainObstacleCollider, Collider2D groundCollider)
    {
        GameObject secondObstacle = currentObstacleTypeChoice.GetFollowUpObstacle(UnityEngine.Random.Range(0,currentObstacleTypeChoice.noOfFollowObstacleObjs));
        Collider2D secondObstacleCollider = secondObstacle.GetComponent<Collider2D>();

        secondObstacle.transform.position = Vector3.zero;
        Physics2D.SyncTransforms();
        if(!secondObstacle.activeInHierarchy) {secondObstacle.SetActive(true);}

        float obstacleBottomBoundsPosition = secondObstacleCollider.bounds.center.y - secondObstacleCollider.bounds.extents.y; 
        
        float mainToSecondSideToSideDistance = mainObstacleCollider.bounds.extents.x + secondObstacleCollider.bounds.extents.x;
        
        Vector2 secondObstaclePos = new Vector2(mainObstacleCollider.transform.position.x + mainToSecondSideToSideDistance + currentObstacleTypeChoice.followUpObjectDistance + distanceToAddToFollowUp, groundCollider.bounds.center.y + groundCollider.bounds.extents.y - obstacleBottomBoundsPosition);
        secondObstacle.transform.position = secondObstaclePos;
        Physics2D.SyncTransforms();
        currentSpawnAction = currentObstacleTypeChoice.followObjectSpawnAction;
    }

    private bool CheckForPreviousObjectNear(Collider2D obstacle,float checkRadius)
    {
        Vector3 checkPos = new Vector3(obstacle.transform.position.x - obstacle.bounds.extents.x,obstacle.transform.position.y,obstacle.transform.position.z);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPos, checkRadius, whatIsObstacle);
        
        foreach(Collider2D collider in colliders)
        {
            if(collider != obstacle)
            {
                Debug.Log($"{obstacle.name} collides with {collider.name}");
                return true;
            }
        }

        return false;
    }  

    /// <summary>
    /// Function handles obstacle in terms of moving it or removing it.
    /// </summary>
    /// <param name="currentObstacle"></param>
    /// <param name="mainObstacle"></param>
    /// <param name="ground"></param>
    /// <param name="obstacleBottomBoundsPosition"></param>
    /// <returns>Returns true if obstacle moved forward, false if obstacle was reset</returns>
    private bool HandleObstacle(Obstacle currentObstacle, GameObject mainObstacle, Collider2D ground, float obstacleBottomBoundsPosition)
    {
        if(currentObstacle.obstacleType == ObstacleType.ManualPad || currentObstacle.obstacleType == ObstacleType.Unavoidable)
        {
            mainObstacle.transform.position = Vector3.zero;
            Physics2D.SyncTransforms();
            Debug.Log($"Removing {mainObstacle.name} obstacle");
            return false;
        }
        Debug.Log($"Moving {mainObstacle.name} obstacle");
        MoveObstacle(currentObstacle.checkRadius,mainObstacle,ground,obstacleBottomBoundsPosition);
        return true;
    }

    private void MoveObstacle(float checkRadius, GameObject mainObstacle, Collider2D ground, float obstacleBottomBoundsPosition)
    {        
        mainObstacle.transform.position = new Vector2(ground.gameObject.transform.position.x + checkRadius -1,ground.bounds.center.y + ground.bounds.extents.y - obstacleBottomBoundsPosition);
        Physics2D.SyncTransforms();
    }

    private void ResetMap(object sender, EventArgs e)
    {
        previousGround = startGround;
        DisableObstacles();
        StopCoroutine(DisableComboRush());
        comboRush = false;
        distanceToAddToFollowUp = 0;
    }

    private void DisableObstacles()
    {
        foreach(Obstacle obstacle in obstacles.GetObjects())
        {
            DisableGameObjects(obstacle.GetAllMainObstacles());
            for(int i = 0; i < obstacle.GetCountOfFollowUpObstacles(); i++)
            {
                DisableGameObjects(obstacle.GetFollowUpObjectsAt(i));
            }
        }
    }

    private void DisableGameObjects(GameObject[] gameObjects)
    {
        foreach(GameObject gameObject in gameObjects)
        {
            if(gameObject.activeInHierarchy) {gameObject.SetActive(false);}
        }
    }

    public void StartComboRush()
    {
        if(comboRush) {return;}
        Debug.Log("Starting combo rush");
        comboRush = true;
        StartCoroutine(DisableComboRush());
    }

    /// <summary>
    /// Disables Combo Rush after the provided duration from the inspector
    /// </summary>
    /// <returns></returns>
    private IEnumerator DisableComboRush()
    {
        yield return new WaitForSeconds(comboRushDuration);
        comboRush = false;
    }

    private void OnGameSpeedChanged(object sender, EventArgs e)
    {
        switch(GameManager.Instance.currentGameSpeed)
        {
            case GameSpeed.Medium: distanceToAddToFollowUp = 0.25f; break;
            case GameSpeed.Fast: distanceToAddToFollowUp = 0.5f; break;
            case GameSpeed.SuperFast: distanceToAddToFollowUp = 1f; break;
        }
    }

    void OnEnable()
    {
        GameManager.reset += ResetMap;
        GameManager.gamespeedChanged += OnGameSpeedChanged;
    }

    void OnDisable()
    {
        GameManager.reset -= ResetMap;
        GameManager.gamespeedChanged -= OnGameSpeedChanged;
    }
}   
