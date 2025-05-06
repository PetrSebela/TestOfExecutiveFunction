using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}