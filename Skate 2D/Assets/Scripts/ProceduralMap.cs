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
    [SerializeField]private GameObject[] obstaclePrefabs;
    private Pool<GameObject> groundObjects;
    private Pool<GameObject>[] obstaclePools;
    private Vector2 previousSpawnPosition;

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
            grounds[i] = Instantiate(groundPrefab,new Vector2(0,1000), Quaternion.identity);
        }
        
        groundObjects = new Pool<GameObject>(5,grounds);
        obstaclePools = new Pool<GameObject>[obstaclePrefabs.Length];
        for(int i = 0; i < obstaclePrefabs.Length; i++)
        {
            GameObject one = Instantiate(obstaclePrefabs[i],new Vector2(0,1000),Quaternion.identity);
            GameObject two = Instantiate(obstaclePrefabs[i],new Vector2(0,1000),Quaternion.identity);
            GameObject three = Instantiate(obstaclePrefabs[i],new Vector2(0,1000),Quaternion.identity);
            obstaclePools[i] = new Pool<GameObject>(2,one,two,three);
        }
        

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

    void Update()
    {
        
    }

    /// <summary>
    /// Generates Terrain and Obstacles
    /// </summary>
    public void GenerateMap()
    {
        GameObject ground = CreateGround();
        CreateObstacle(ground);
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
    private void CreateObstacle(GameObject ground)
    {
        GameObject currentObstacle = obstaclePools[Random.Range(0,obstaclePools.Length)].GetObject();
        Collider2D groundCollider = ground.GetComponent<Collider2D>();
        Collider2D obstacleCollider = currentObstacle.GetComponent<Collider2D>();
        // the value 'centreToBottomDistance is needed in order to place the bottom Y bounds of an obstacle,
        // onto the top Y bounds of the current ground GameObject that was activated
        float centreToBottomDistance = obstacleCollider.transform.position.y - obstacleCollider.bounds.min.y; 

        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x,groundCollider.bounds.max.y + centreToBottomDistance);
        currentObstacle.transform.position = obstacleSpawnPos;
    }
}
