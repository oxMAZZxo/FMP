using System;
using UnityEngine;

/// <summary>
/// The UI Trail Renderer component allows developers to create their own custom trails for the Canvas, as the Unity Engine does not have a built-in Trail Renderer for UI.
/// This version of the class is designed specifically for City Skaters.
/// </summary>
public class UITrailRenderer : MonoBehaviour
{
    [Header("UI Element")]
    [SerializeField]private UITrailPoint trailPoint;
    [Header("Trail Settings")]
    [SerializeField,Range(10,500),Tooltip("The amount of trail points this renderer should create. This number depends on the type of trail you want to create; For example: if you want a long trail, with long trail times, this should be a number closer to the max.")]
    private int trailPoolSize = 5;
    [SerializeField,Range(0.1f,10f),Tooltip("How long each trail point is visible")]private float trailTime = 1f;
    [SerializeField,Range(0.1f,100f),Tooltip("The distance between each trail point. This depends on the type of sprite you use for the trail.")]
    private float minDistance = 0.5f;
    [SerializeField]private bool emitting;
    private Pool<UITrailPoint> trailObjects;
    private Vector2? lastPosition;

    void Start()
    {
        InitPool();
    }

    void InitPool()
    {
        UITrailPoint[] tempObj = new UITrailPoint[trailPoolSize];

        for(int i = 0; i < trailPoolSize; i++)
        {
            tempObj[i] = Instantiate(trailPoint,transform);
            tempObj[i].Init(trailTime);
        }

        trailObjects = new Pool<UITrailPoint>(trailPoolSize,tempObj);
    }

    void Update()
    {
        // If we're not emitting, reset tracking
        if(!emitting) 
        {
            lastPosition = null;
            return;
        }
        Vector2 currentPos = TouchControls.Instance.currentTouchPosition;
        if(currentPos == Vector2.zero || currentPos == lastPosition) {return;}
        // If this is the first point, spawn it immediately
        if(lastPosition == null)
        {
            lastPosition = currentPos;
            SpawnTrail(currentPos);
            return;
        }

        Vector2 lastPos = lastPosition.Value;
        float distance = Vector2.Distance(lastPos,currentPos);

        // If the distance moved is large, interpolate multiple trail points
        if(distance > minDistance)
        {
            int numPoints = Mathf.FloorToInt(distance / minDistance);
            for(int i = 1; i <= numPoints; i++)
            {
                Vector2 interpolated = Vector2.Lerp(lastPos, currentPos, (float)i / numPoints);
                SpawnTrail(interpolated);
            }
        }else
        {
            // Otherwise just place one point
            SpawnTrail(currentPos);
        }

        // Save current position for next frame
        lastPosition = currentPos;
    }


    private void SpawnTrail(Vector2 position)
    {
        //Get trail from the pool
        UITrailPoint current = trailObjects.GetObject();
        //Change its position to given position
        current.transform.position = position;
        //Render it
        current.Render();
    }

    void OnTouchStarted(object sender, Vector2 position) {
        emitting = true;
    }

    void OnTouchEnded(object sender, Vector2 position)
    {
        emitting = false;
    }

    void OnEnable()
    {
        TouchControls.touchStarted += OnTouchStarted;
        TouchControls.touchEnded += OnTouchEnded;
    }

    void OnDisable()
    {
        TouchControls.touchStarted -= OnTouchStarted;
        TouchControls.touchEnded -= OnTouchEnded;
    }
}
