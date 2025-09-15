using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The Procedural Map class is a singleton object which will generate the endless map that the player will play on.
/// It will perform logic such as spawning the ground, the obstacles, the background and other scene environment.
/// </summary>
public class ProceduralMap : MonoBehaviour
{
    public static ProceduralMap Instance {get; private set;}
    const float ySpawnPosition = -0.75f;
    [SerializeField]private Transform player;
    [SerializeField]private GameObject groundPrefab;
    [SerializeField]private Collider2D startGround;
    [Header("Obstacle Generation")]
    [SerializeField]private bool generateObstacles;
    [SerializeField]private Obstacles[] obstacleTypes;
    [SerializeField]private LayerMask whatIsObstacle;
    [Header("Pick Ups")]
    [SerializeField] private PickUp[] pickUpPrefabs;
    [SerializeField, Range(1, 100)] private int pickUpSpawnChance;
    private PickUp[] pickUps;
    [Header("Combo Rush")]
    [SerializeField]private bool comboRush = false;
    [Header("Background Environment Generation")]
    [SerializeField]private bool generateEnvironment;
    [SerializeField]private Background[] backgroundPrefabs;
    [SerializeField]private Collider2D startBackground;
    private Collider2D previousBackground;
    private Pool<Background> backgroundPool;
    private Pool<GameObject> groundObjects;
    private Pool<Obstacle> obstacles;
    private Pool<Obstacle> grindableObstacles;
    private Collider2D previousGround;
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
        if (generateObstacles) { InitObstaclePool(); }
        InitBackgroundPool();
        if (startGround == null)
        {
            Debug.LogError("The variable Start Ground in the Procedural Map Component is null. Cannot generate ground, obstacles or environment without that");
        }
        else
        {
            previousGround = startGround;
        }
        if (startBackground == null)
        {
            Debug.LogError("The variable Start Background in the Procedural Map Component is null. Cannot generate background environment without that");
        }
        else
        {
            previousBackground = startBackground;
        }
        InitPickUps();
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
                Obstacle current = new Obstacle(obstacles.type,spawnable,CreateMainObstaclePool(spawnable.prefab),
                CreateFollowUpObjectPool(spawnable.followObjs),null);
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

    private void InitBackgroundPool()
    {
        Background[] temp = new Background[backgroundPrefabs.Length];
        for(int i = 0; i < temp.Length; i++)
        {
            temp[i] = Instantiate(backgroundPrefabs[i],new Vector2(0,100),Quaternion.identity);
        }
        backgroundPool = new Pool<Background>(temp.Length,temp);
    }

    private Pool<GameObject> CreateMainObstaclePool(GameObject prefab)
    {
        List<GameObject> tempPrefabPool = new List<GameObject>();
        for(int i = 0; i < 2; i++)
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

    private void InitPickUps()
    {
        pickUps = new PickUp[4];
        for (int i = 0; i < pickUpPrefabs.Length; i++)
        {
            PickUp current = Instantiate(pickUpPrefabs[i], new Vector3(0,100,0), Quaternion.identity);
            pickUps[i] = current;
        }
    }

    /// <summary>
    /// Generates Terrain and Obstacles
    /// </summary>
    public void GenerateMap()
    {
        GameObject ground = CreateGround();
        if (generateObstacles) { StartCoroutine(CreateObstacles(ground)); }

        if (generateEnvironment) { CreateBackgroundEnvironment(); }
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

    private void CreateBackgroundEnvironment()
    {
        Background currentBackground = backgroundPool.GetObject();
        if(currentBackground.isBeingRendered) {
            backgroundPool.RollBack();
            return;
        }
        Collider2D currentEnvironementCollider = currentBackground.GetComponent<Collider2D>();
        currentEnvironementCollider.transform.position = Vector3.zero;
        Physics2D.SyncTransforms();

        float mainToSecondSideToSideDistance = previousBackground.bounds.extents.x + currentEnvironementCollider.bounds.extents.x;
        float previousXPosition = previousBackground.transform.position.x;

        Vector3 newPos = new Vector3(previousXPosition + mainToSecondSideToSideDistance,previousBackground.transform.position.y);
        currentEnvironementCollider.transform.position = newPos;
        Physics2D.SyncTransforms();
        previousBackground = currentEnvironementCollider;
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
    IEnumerator CreateObstacles(GameObject ground)
    {
        if (currentSpawnAction != SpawnAction.Spawn)
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
        if (comboRush)
        {
            obstaclePoolToChooseFrom = grindableObstacles;
            maxIndex = grindableObstacles.length - 1;
        }

        Obstacle currentObstacleType = obstaclePoolToChooseFrom.GetRandomObject(maxIndex + 1);
        GameObject mainObstacle = currentObstacleType.GetMainObstacle();
        if (GameobjectInSight(mainObstacle))
        {
            currentObstacleType.RollBackMainObstacle();
            Debug.Log($"Main Obstacle {mainObstacle.name} is in sight, therefore I cannot use it. SKIPPING.");
            yield break;
        }
        bool secondObstacleElligible = PositionFirst(ground, mainObstacle, currentObstacleType);
        GameObject secondObstacle = null;

        if (currentObstacleType.noOfFollowObstacleObjs > 0 && secondObstacleElligible &&
        ((GameManager.Instance.currentGameSpeed >= currentObstacleType.minimumAcceptableGameSpeedForFollowUp
        && UnityEngine.Random.Range(0, 100) <= currentObstacleType.followObjectChance) || comboRush))
        {
            secondObstacle = CreateSecondObstacle(currentObstacleType, mainObstacle.GetComponent<Collider2D>(), ground.GetComponent<Collider2D>());
        }
        if (currentObstacleType.obstacleType == ObstacleType.Bench || currentObstacleType.obstacleType == ObstacleType.Rail || currentObstacleType.obstacleType == ObstacleType.Kicker)
        {
            CreatePickUp(currentObstacleType.obstacleType,mainObstacle, secondObstacle);
        }
    }

    /// <summary>
    /// Positions the first obstacle in the current obstacle choice.
    /// </summary>
    /// <param name="ground"></param>
    /// <param name="mainObstacle"></param>
    /// <param name="currentObstacleType"></param>
    /// <returns>Returns true if a follow up obstacle can be created.</returns>
    private bool PositionFirst(GameObject ground, GameObject mainObstacle, Obstacle currentObstacleType)
    {
        Collider2D mainObstacleCollider = mainObstacle.GetComponent<Collider2D>();
        mainObstacle.transform.position = Vector3.zero;
        Physics2D.SyncTransforms();

        if (!mainObstacle.activeInHierarchy) { mainObstacle.SetActive(true); }

        Collider2D groundCollider = ground.GetComponent<Collider2D>();

        float obstacleBottomBoundsPosition = mainObstacleCollider.bounds.center.y - mainObstacleCollider.bounds.extents.y;

        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x, groundCollider.bounds.center.y + groundCollider.bounds.extents.y - obstacleBottomBoundsPosition);

        mainObstacle.transform.position = obstacleSpawnPos;
        Physics2D.SyncTransforms();

        bool obstaclePositioningSuccess = true;
        if (CheckForPreviousObjectNear(mainObstacleCollider, currentObstacleType.checkRadius))
        {
            obstaclePositioningSuccess = HandleObstacle(currentObstacleType, mainObstacle, groundCollider, obstacleBottomBoundsPosition);
        }

        currentSpawnAction = currentObstacleType.spawnAction;
        if (currentObstacleType.obstacleType == ObstacleType.Unavoidable && GameManager.Instance.currentGameSpeed == GameSpeed.Slow) { currentSpawnAction = SpawnAction.Spawn; }
        return obstaclePositioningSuccess;
    }

    /// <summary>
    /// Created a Second Obstacle (Follow Up obstacle) based on the main obstacle spawned.
    /// </summary>
    /// <param name="currentObstacleTypeChoice">The current obstacle choice in terms of type</param>
    /// <param name="mainObstacleCollider">The main obstacles collider</param>
    /// <param name="groundCollider">The current grounds collider</param>
    private GameObject CreateSecondObstacle(Obstacle currentObstacleTypeChoice, Collider2D mainObstacleCollider, Collider2D groundCollider)
    {
        int choice = UnityEngine.Random.Range(0, currentObstacleTypeChoice.noOfFollowObstacleObjs);
        GameObject secondObstacle = currentObstacleTypeChoice.GetFollowUpObstacle(choice);
        if (GameobjectInSight(secondObstacle))
        {
            Debug.Log($"Second obstacle {secondObstacle.name} of main obstacle {mainObstacleCollider.name} is visible, therefore cannot use it.");
            currentObstacleTypeChoice.RollBackFollowUpObstacle(choice);
            return null;
        }
        Collider2D secondObstacleCollider = secondObstacle.GetComponent<Collider2D>();
        secondObstacle.transform.position = Vector3.zero;
        Physics2D.SyncTransforms();
        if (!secondObstacle.activeInHierarchy) { secondObstacle.SetActive(true); }

        float obstacleBottomBoundsPosition = secondObstacleCollider.bounds.center.y - secondObstacleCollider.bounds.extents.y;

        float mainToSecondSideToSideDistance = mainObstacleCollider.bounds.extents.x + secondObstacleCollider.bounds.extents.x;

        Vector2 secondObstaclePos = new Vector2(mainObstacleCollider.transform.position.x + mainToSecondSideToSideDistance + currentObstacleTypeChoice.followUpObjectDistance + distanceToAddToFollowUp, groundCollider.bounds.center.y + groundCollider.bounds.extents.y - obstacleBottomBoundsPosition);
        secondObstacle.transform.position = secondObstaclePos;
        Physics2D.SyncTransforms();
        currentSpawnAction = currentObstacleTypeChoice.followObjectSpawnAction;

        return secondObstacle;
    }

    private void CreatePickUp(ObstacleType obstacleType, GameObject mainObstacle, GameObject secondObstacle)
    {
        if (UnityEngine.Random.Range(1, 100) > pickUpSpawnChance) { return; }

        Collider2D obstacleSpawnChoice = mainObstacle.GetComponent<Collider2D>();
        
        if (secondObstacle != null && obstacleType == ObstacleType.Bench && UnityEngine.Random.Range(1, 100) > 50 )
        {
            obstacleSpawnChoice = secondObstacle.GetComponent<Collider2D>();
        }

        PickUp currentPickUp;
        Vector3 spawnPosition;
        
        if (obstacleType == ObstacleType.Kicker)
        {
            currentPickUp = pickUps[0];
            currentPickUp.gameObject.SetActive(true);
            currentPickUp.transform.position = Vector3.zero;
            Physics2D.SyncTransforms();
            spawnPosition = new Vector3(2, 2.2f, 0) + obstacleSpawnChoice.transform.position;
        }
        else
        {
            currentPickUp = SelectRandomPickUp();
            currentPickUp.gameObject.SetActive(true);
            currentPickUp.transform.position = Vector3.zero;
            Physics2D.SyncTransforms();
            Collider2D currentPickUpCollider = currentPickUp.GetComponent<Collider2D>();
            float obstacleTopBounds = obstacleSpawnChoice.bounds.center.y + obstacleSpawnChoice.bounds.extents.y;
            float pickUpBottomBounds = currentPickUpCollider.bounds.center.y + currentPickUpCollider.bounds.extents.y;
            spawnPosition = new Vector3(obstacleSpawnChoice.transform.position.x, obstacleTopBounds + pickUpBottomBounds);
        }

        currentPickUp.AssignMultiplier();
        currentPickUp.transform.position = spawnPosition;
        Physics2D.SyncTransforms();
    }

    private PickUp SelectRandomPickUp()
    {
        PickUp current;
        do
        {
            current = pickUps[UnityEngine.Random.Range(1, 4)];
            SpriteRenderer spriteRenderer = current.GetComponent<SpriteRenderer>();
            if (spriteRenderer.isVisible) { current = null; }
        } while (current == null);

        return current;
    }

    private bool CheckForPreviousObjectNear(Collider2D obstacle, float checkRadius)
    {
        Vector3 checkPos = new Vector3(obstacle.transform.position.x - obstacle.bounds.extents.x, obstacle.transform.position.y, obstacle.transform.position.z);
        Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPos, checkRadius, whatIsObstacle);

        foreach (Collider2D collider in colliders)
        {
            if (collider != obstacle)
            {
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
        if (currentObstacle.obstacleType == ObstacleType.ManualPad || currentObstacle.obstacleType == ObstacleType.Unavoidable)
        {
            mainObstacle.transform.position = Vector3.zero;
            Physics2D.SyncTransforms();
            return false;
        }
        MoveObstacle(currentObstacle.checkRadius,mainObstacle,ground,obstacleBottomBoundsPosition);
        return true;
    }

    private void MoveObstacle(float checkRadius, GameObject mainObstacle, Collider2D ground, float obstacleBottomBoundsPosition)
    {        
        mainObstacle.transform.position = new Vector2(ground.gameObject.transform.position.x + checkRadius,ground.bounds.center.y + ground.bounds.extents.y - obstacleBottomBoundsPosition);
        Physics2D.SyncTransforms();
    }

    private bool GameobjectInSight(GameObject current)
    {
        Renderer renderer = current.GetComponent<Renderer>();
        if(renderer == null) {renderer = GetComponentInChildren<Renderer>();}
        if(renderer == null) {return false;}
        return renderer.isVisible;
    }

    private void ResetMap(object sender, EventArgs e)
    {
        previousGround = startGround;
        previousBackground = startBackground;
        DisableObstacles();
        StopCoroutine(DisableComboRush(0));
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

    public void StartComboRush(float comboRushDuration)
    {
        if(comboRush) {return;}
        Debug.Log("Starting combo rush");
        comboRush = true;
        StartCoroutine(DisableComboRush(comboRushDuration));
    }

    /// <summary>
    /// Disables Combo Rush after the provided duration from the inspector
    /// </summary>
    /// <returns></returns>
    private IEnumerator DisableComboRush(float comboRushDuration)
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
