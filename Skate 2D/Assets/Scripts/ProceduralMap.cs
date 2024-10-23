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
    [SerializeField]private GameObject[] obstacles;
    private Pool<GameObject> groundObjects;
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
        InitPool();
        InitGround();
    }


    /// <summary>
    /// Initialises a pool of ground GameObjects with a fixed amount
    /// </summary>
    private void InitPool()
    {
        GameObject[] grounds = new GameObject[5];
        for(int i = 0; i < grounds.Length; i++)
        {
            grounds[i] = Instantiate(groundPrefab,transform.position, Quaternion.identity);
            grounds[i].SetActive(false);
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
        middleGround.SetActive(true);        

        placePosition = new Vector2(middleGround.transform.position.x - groundSize, ySpawnPosition);
        GameObject leftGround = groundObjects.GetObject();
        leftGround.transform.position = placePosition;
        leftGround.SetActive(true);

        placePosition = new Vector2(middleGround.transform.position.x + groundSize,ySpawnPosition);
        GameObject rightGround = groundObjects.GetObject();
        rightGround.transform.position = placePosition;
        rightGround.SetActive(true);
        
        previousSpawnPosition = rightGround.transform.position;
    }

    void Update()
    {
        
    }

    /// <summary>
    /// Generates Terrain and Obstacles
    /// </summary>
    public void Generate()
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
        currentGround.SetActive(true);
        return currentGround;
    }

    /// <summary>
    /// Creates a random obstacle from an array
    /// </summary>
    /// <param name="ground"></param>
    private void CreateObstacle(GameObject ground)
    {
        GameObject currentObstacle = Instantiate(obstacles[Random.Range(0,4)],transform.position,Quaternion.identity);
        
        Collider2D groundCollider = ground.GetComponent<Collider2D>();
        Collider2D obstacleCollider = currentObstacle.GetComponent<Collider2D>();
        // the value 'centreToBottomDistance is needed in order to place the bottom Y bounds of an obstacle,
        // onto the top Y bounds of the current ground GameObject that was activated
        float centreToBottomDistance = obstacleCollider.transform.position.y - obstacleCollider.bounds.min.y; 
        Vector2 obstacleSpawnPos = new Vector2(ground.transform.position.x,groundCollider.bounds.max.y + centreToBottomDistance);
        currentObstacle.transform.position = obstacleSpawnPos;
    }
}
