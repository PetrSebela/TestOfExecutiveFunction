using UnityEngine;

public class Utils
{
    public static Vector2 RandomPoint()
    {
        Random.InitState((int)(Time.timeAsDouble * 1000000));
        return new Vector2(Random.value, Random.value);
    }
}
