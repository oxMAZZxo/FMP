using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides easy access to the background city object last rendered sprite, for the procedural map generation.
/// </summary>
public class Background : MonoBehaviour
{
    [SerializeField]private Renderer lastRendered;
    public bool isBeingRendered{
        get{
            return lastRendered.isVisible;
        }
    }
    
}
