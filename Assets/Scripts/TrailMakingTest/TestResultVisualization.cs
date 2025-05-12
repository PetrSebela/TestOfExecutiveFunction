using UnityEngine;

/// <summary>
/// SGO for connecting with Unity UI Toolkit
/// </summary>
[CreateAssetMenu(fileName = "TRV", menuName = "Tests/TRV", order = 1)]
public class TestResultVisualization : ScriptableObject
{
    public Vector2[] SpeedGraph = new Vector2[0];
    public Vector2[] AccelerationGraph = new Vector2[0];

    public double[] Clicks = new double[0]; 

    public double[] Data = new double[0]; 

    public string Score = "0";
    public string Duration = "0ms";
    public string Correct = "0/0";
    public string Sureness = "0";

    public string TestSize = "0";
    public string ActiveModifier = "1x";
}