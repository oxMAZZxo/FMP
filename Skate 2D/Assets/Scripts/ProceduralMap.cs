using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMap : MonoBehaviour
{
    private const float secondObstacleX_SpawnOffset = 1f;
    public static ProceduralMap Instance {get; private set;}
    const int groundSize = 8;
    const float ySpawnPosition = -0.5f;
    [SerializeField]private Transform player;
    [SerializeField]private GameObject groundPrefab;
    [SerializeField]private bool generateObstacles;
    [SerializeField]private Spawnable[] obstaclePrefabs;
    private Pool<GameObject> groundObjects;
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
        StartCoroutine(CreateObstacle(ground));
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
        // GameObject currentObstacle = obstaclePools[Random.Range(0,obstaclePools.Length)].GetObject();
        int obstacleType = Random.Range(0,obstaclePrefabs.Length);
        int obstacleChoice = Random.Range(0,obstaclePrefabs[obstacleType].objects.Length);
        GameObject firstObstacle = Instantiate(obstaclePrefabs[obstacleType].objects[obstacleChoice],transform.position,Quaternion.identity);
        Collider2D groundCollider = ground.GetComponent<Collider2D>();
        Collider2D firstObstacleCollider = firstObstacle.GetComponent<Collider2D>();
        // the value 'centreToBottomDistance is needed in order to place the bottom Y bounds of an obstacle,
        // onto the top Y bounds of the current ground GameObject that was activated
        float centreToBottomDistance = firstObstacleCollider.transform.position.y - firstObstacleCollider.bounds.min.y; 

        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x,groundCollider.bounds.max.y + centreToBottomDistance);

        firstObstacle.transform.position = obstacleSpawnPos;
        currentSpawnAction = obstaclePrefabs[obstacleType].spawnAction;
        if(obstaclePrefabs[obstacleType].followObjs.Length > 0 && Random.Range(0,101) > 100 - obstaclePrefabs[obstacleType].followObjectChance)
        {
            // Debug.Log($"First obstacle type is {obstaclePrefabs[obstacleType].type.ToString()}");
            int secondObstacleType = Random.Range(0,obstaclePrefabs[obstacleType].followObjs.Length);
            // Debug.Log($"Second obstacle type number is {secondObstacleType}");
            // Debug.Log($"Second obstacle type is {obstaclePrefabs[obstacleType].followObjs[secondObstacleType].type.ToString()}");
            int secondObstacleChoice = Random.Range(0,obstaclePrefabs[obstacleType].followObjs[secondObstacleType].objects.Length);
            // Debug.Log($"From that type, the object chosen is {obstaclePrefabs[obstacleType].followObjs[secondObstacleType].objects[secondObstacleChoice].name}");
            GameObject secondObstacle = Instantiate(obstaclePrefabs[obstacleType].followObjs[secondObstacleType].objects[secondObstacleChoice],transform.position,Quaternion.identity);

            Collider2D secondObstacleCollider = secondObstacle.GetComponent<Collider2D>();
            float centreToLeft = secondObstacleCollider.transform.position.x - secondObstacleCollider.bounds.min.x;
            centreToBottomDistance = secondObstacleCollider.transform.position.y - secondObstacleCollider.bounds.min.y;
            Vector2 secondObstaclePos = new Vector2(firstObstacle.transform.position.x + firstObstacleCollider.bounds.max.x + centreToLeft + secondObstacleX_SpawnOffset, groundCollider.bounds.max.y + centreToBottomDistance);
            secondObstacle.transform.position = secondObstaclePos;
            switch (obstaclePrefabs[obstacleType].followObjs[secondObstacleType].type)
            {
                case ObjectType.Bins:
                secondObstacleCollider.isTrigger = false;
                break;
                
                case ObjectType.Unavoidable: 
                currentSpawnAction = obstaclePrefabs[obstacleType].followObjs[secondObstacleType].spawnAction;
                break;
            }
            Destroy(secondObstacle,10f);
        }
        
        Destroy(firstObstacle,10f);
    }
}
