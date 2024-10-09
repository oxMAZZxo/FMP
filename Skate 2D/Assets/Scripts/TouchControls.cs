using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.VisualScripting;

public class TouchControls : MonoBehaviour
{
    [SerializeField,Range(1f,1000f)]private float deadzone = 10f;
    [SerializeField,Range(0.01f,1f)]private float minTravelMagnitude = 0.4f;
    [SerializeField]private bool showGrid = true;
    [SerializeField,Range(0.01f,20f)]private float largeStep;
    public  Color color = new Color(0f, 1f, 0f, 1f);
    public Color subColor = new Color(0f, 0.5f, 0f, 1f);
    public static event EventHandler tapEvent;
    public static event EventHandler swipeRightEvent;
    public static event EventHandler swipeDownEvent;
    public static event EventHandler swipeLeftEvent;
    private Material lineMaterial;
    private Vector2 touchStart;
    private Vector2 touchEnd;
    private bool drew;

    void Start()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);
    }

    void Update()
    {
        if(touchStart != Vector2.zero && !drew)
        {
            Vector2 mousePosWorld = Camera.main.ScreenToWorldPoint(touchStart);
            Vector3 gridStart = new Vector3(mousePosWorld.x - (deadzone / 2),mousePosWorld.y - (deadzone / 2),0);
            Vector3 gridSize = new Vector3(mousePosWorld.x + deadzone, mousePosWorld.y + deadzone,1);
            Debug.Log($"Mouse position in world space: {mousePosWorld.ToString()}" + Environment.NewLine +
            $"Grid start position: {gridStart.ToString()}" + Environment.NewLine +
            $"Grid size: {gridSize.ToString()}");
            // // DrawSquare(gridStart,gridSize);
            // drew = true;
        }
        if(Input.GetMouseButtonDown(0))
        {
            touchStart = Input.mousePosition;
        }

        if(Input.GetMouseButtonUp(0))
        {
            touchEnd = Input.mousePosition;
            DetectSwipe();
            touchStart = Vector2.zero;
            drew = false;
        }
    }

    private void DetectSwipe()
    {
        // Vector2 startPosWorldSpace = Camera.main.ScreenToWorldPoint(touchStart);
        // Vector2 endPosWorldSpace = Camera.main.ScreenToWorldPoint(touchEnd);
        // Debug.Log("Start: " + startPosWorldSpace.ToString());
        // Debug.Log("End: " + endPosWorldSpace.ToString());
        // Debug.Log("Distance X: " + Mathf.Abs(startPosWorldSpace.x - endPosWorldSpace.x));
        // Debug.Log("Distance Y: " + Mathf.Abs(startPosWorldSpace.y - endPosWorldSpace.y));
        Vector2 swipeDelta = touchEnd - touchStart;
        Debug.Log("Swipe Delta Distance X: " + Mathf.Abs(swipeDelta.x));
        Debug.Log("Distance X: " + Mathf.Abs(touchStart.x - touchEnd.x));
        if(swipeDelta.magnitude > deadzone)
        {
            SwipeDirection swipeDirection = CalculateSwipe(swipeDelta);
            switch (swipeDirection)
            {
                case SwipeDirection.RIGHT: 
                //swipeRightEvent.Invoke(this, new EventArgs());
                break;

                case SwipeDirection.LEFT:
                // swipeLeftEvent.Invoke(this, new EventArgs());
                break;

                case SwipeDirection.DOWN:
                // swipeDownEvent.Invoke(this, new EventArgs());
                break;
            }
            Debug.Log(swipeDirection.ToString());
        }else
        {
            Debug.Log("TAP");
            // tapEvent.Invoke(this, new EventArgs());
        }
    }


    private SwipeDirection CalculateSwipe(Vector2 delta)
    {

        delta.Normalize();
        if (delta.x < -minTravelMagnitude && delta.y < -minTravelMagnitude)
        {
            return SwipeDirection.DOWN_LEFT;
        }

        if (delta.x < -minTravelMagnitude && delta.y > minTravelMagnitude)
        {
            return SwipeDirection.UP_LEFT;
        }

        if (delta.x > minTravelMagnitude && delta.y < -minTravelMagnitude)
        {
            return SwipeDirection.DOWN_RIGHT;
        }

        if (delta.x > minTravelMagnitude && delta.y > minTravelMagnitude)
        {
            return SwipeDirection.UP_RIGHT;
        }

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            return delta.x > 0 ? SwipeDirection.RIGHT : SwipeDirection.LEFT;
        }
        else
        {
            return delta.y > 0 ? SwipeDirection.UP : SwipeDirection.DOWN;
        }
    }

     void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    public void DrawSquare(Vector3 gridStart,Vector3 gridSize)
    {
        GL.Begin(GL.LINES);

        if (showGrid)
        {
            GL.Color(color);

            //Layers
            for (float j = 0; j <= gridSize.y; j += largeStep)
            {
                //X axis lines
                for (float i = 0; i <= gridSize.z; i += largeStep)
                {
                    GL.Vertex3(gridStart.x, gridStart.y + j, 0);
                    GL.Vertex3(gridStart.x + gridSize.x, gridStart.y + j , 0);
                }
            }

            //Y axis lines
            for (float i = 0; i <= gridSize.z; i += largeStep)
            {
                for (float k = 0; k <= gridSize.x; k += largeStep)
                {
                    GL.Vertex3(gridStart.x + k, gridStart.y , 0);
                    GL.Vertex3(gridStart.x + k, gridStart.y + gridSize.y ,0);
                    
                }
            }
        }
        GL.End();

        Debug.DebugBreak();
    }
}

public enum SwipeDirection
{
    TAP,
    UP,
    DOWN,
    LEFT,
    RIGHT,
    UP_LEFT,
    UP_RIGHT,
    DOWN_LEFT,
    DOWN_RIGHT
}
