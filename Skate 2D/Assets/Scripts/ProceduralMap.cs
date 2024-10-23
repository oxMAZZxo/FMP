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
    /// Creates a new ground based on the position of where the previous ground was created
    /// </summary>
    public void CreateGround()
    {
        GameObject currentGround = groundObjects.GetObject();
        Vector2 spawnPos = new Vector2(previousSpawnPosition.x + groundSize, ySpawnPosition);
        currentGround.transform.position = spawnPos;
        previousSpawnPosition = spawnPos;
        currentGround.SetActive(true);
    }
}
