using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    
    [SerializeField]private Renderer lastRendered;
    public bool isBeingRendered{
        get{
            return lastRendered.isVisible;
        }
    }
    
}
