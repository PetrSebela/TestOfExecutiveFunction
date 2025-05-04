using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sample
{
    public Vector2 point;
    public double time_stamp;
    public bool active;

    public Sample(Vector2 point, double time_stamp, bool active)
    {
        this.point = point;
        this.time_stamp = time_stamp;
        this.active = active;
    }
}