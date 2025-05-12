using UnityEngine;

/// <summary>
/// Class representing captures test sample ( could be a struct )
/// </summary>
public class Sample
{
    /// <summary>
    /// Sample position (in world space)
    /// </summary>
    public Vector2 point;

    /// <summary>
    /// Sample time stamp
    /// </summary>
    public double time_stamp;

    public Sample(Vector2 point, double time_stamp)
    {
        this.point = point;
        this.time_stamp = time_stamp;
    }
}