using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITrailPoint : MonoBehaviour
{
    private float pointAliveTime;

    public void Init(float newPointAliveTime){
        pointAliveTime = newPointAliveTime;
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(DisableMe());
    }

    IEnumerator DisableMe()
    {
        yield return new WaitForSeconds(pointAliveTime);
        gameObject.SetActive(false);
    }
}
