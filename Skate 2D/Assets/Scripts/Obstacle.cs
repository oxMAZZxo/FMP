using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Obstacle : MonoBehaviour
{
    Collider2D myCollider;
    public bool drawGizmos;
    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<Collider2D>();
    }

    void OnDrawGizmos()
    {
        if(myCollider == null || !drawGizmos) {return;}
        Gizmos.color = Color.green;
        Vector2 topLeft = new Vector2(myCollider.bounds.min.x,myCollider.bounds.max.y);
        Vector2 topRight = myCollider.bounds.max;
        Vector2 bottomLeft = myCollider.bounds.min;
        Vector2 bottomRight = new Vector2(myCollider.bounds.max.x,myCollider.bounds.min.y);
        Gizmos.DrawWireSphere(topLeft,0.01f);
        Gizmos.DrawWireSphere(topRight,0.01f);
        Gizmos.DrawWireSphere(bottomLeft,0.01f);
        Gizmos.DrawWireSphere(bottomRight,0.01f);
        float centreToBottomDistance = myCollider.transform.position.y - myCollider.bounds.min.y;
        Debug.Log("The distance from the bottom Y bounds to top Y bounds of the obstacle is:" + centreToBottomDistance);

    }
}
