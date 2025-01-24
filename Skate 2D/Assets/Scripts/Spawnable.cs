using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawnable", menuName = "ScriptableObjects/Spawnable", order = 1)]
public class Spawnable : ScriptableObject
{
    public GameObject prefab;
    public GameObject[] followObjs;
    [Range(1f,100f)]public int followObjectChance = 50;
    public float followUpObjectDistance = 1f;
}
