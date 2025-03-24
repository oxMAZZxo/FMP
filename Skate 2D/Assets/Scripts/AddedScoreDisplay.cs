using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The added scpre display is a gameobject with a TMProUGUI which displays the points added on the players score.
/// </summary>
public class AddedScoreDisplay : DisableMe
{
    private bool run;
    
    private IEnumerator MoveUp()
    {
        while(run)
        {
            yield return new WaitForEndOfFrame();
            Vector3 newPosition = transform.position;
            newPosition.y += 1;
            transform.position = newPosition;
        }
    }

    void OnEnable()
    {
        run = true;
        StartCoroutine(MoveUp());
    }

    void OnDisable()
    {
        run = false;
    }
}
