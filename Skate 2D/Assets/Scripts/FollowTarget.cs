using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField,Tooltip("The object you want this object to follow")]private Transform target;
    [SerializeField,Tooltip("Adjust based on the offset you want this object to be relative to the target")]private Vector3 offset;
    [SerializeField]private bool ignoreX;
    [SerializeField]private bool ignoreY;
    [SerializeField]private bool ignoreZ;
    private Vector3 newPosition;

    void LateUpdate()
    {
        newPosition = target.position + offset;
        if(ignoreX) {newPosition.x = transform.position.x;}
        if(ignoreY) {newPosition.y = transform.position.y;}
        if(ignoreZ) {newPosition.z = transform.position.z;}
        transform.position = newPosition;        
    }
}
