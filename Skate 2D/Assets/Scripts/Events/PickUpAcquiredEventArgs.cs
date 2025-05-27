
using UnityEngine;

public class PickUpAcquiredEventArgs
{
    public float multiplier { get; }
    public Color colour { get; }

    public PickUpAcquiredEventArgs(float multiplier, Color colour)
    {
        this.multiplier = multiplier;
        this.colour = colour;
    }
}
