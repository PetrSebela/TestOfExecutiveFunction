using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TestResult", menuName = "Tests/TestResult", order = 1)]
public class TestResult : ScriptableObject
{
    List<Sample> samples;

    public Vector2[] SpeedGraph = new Vector2[0];

    public void Evaluate(List<Sample> samples)
    {
        this.samples = samples;
        Debug.LogFormat("Performing analysis on {0} samples", samples.Count);
        ShowSpeedGraph();
    }

    public void ShowSpeedGraph()
    {
        List<Vector2> graph = new();

        Sample first_sample = samples[0];
        Sample previous_sample = first_sample;
        for (int i = 0; i < samples.Count; i++)
        {
            double distance = Vector3.Distance(previous_sample.point, samples[i].point);
            double delta_time = samples[i].time_stamp - previous_sample.time_stamp;
            
            Debug.LogFormat("D = {0} | DT = {1}", distance, delta_time);

            if(distance < 0.0025f || delta_time < 0.0025f)
                continue;

            double speed = distance / delta_time;

            double time = samples[i].time_stamp - first_sample.time_stamp;

            Vector2 graph_point = new((float)time, (float)speed);
            graph.Add(graph_point);

            previous_sample = samples[i];
        }

        SpeedGraph = graph.ToArray();
    }
}