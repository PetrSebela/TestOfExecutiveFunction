
using System.Collections.Generic;
using UnityEngine;

public class Evaluator
{
    List<Sample> samples;
    List<Sample> clicks;
    List<Target> targets;

    double begin_time = 0;

    public Evaluator(List<Sample> samples, List<Sample> clicks,  List<Target> targets)
    {
        this.samples = samples;
        this.clicks = clicks;
        this.targets = targets;
    
        begin_time = clicks[0].time_stamp;

        Debug.LogFormat("Test duration {0}", GetTestDuration());
        Debug.LogFormat("Correctness {0}", GetCorrectness());
    }

    public double GetTestDuration()
    {
        return clicks[^1].time_stamp - clicks[0].time_stamp;
    }

    public double GetScore()
    {
        double score = targets.Count * 1.2f / GetTestDuration() * GetCorrectness() * 100;
        return score;
    }

    /// <summary>
    /// Computes how many targets were pressed in correct sequence 
    /// </summary>
    /// <returns> Percentage of how many targets were pressed in correct sequence </returns>
    public float GetCorrectness()
    {
        float correct = 0;
        int current = -1;
        foreach (Sample click in clicks)
        {
            foreach (Target target in targets)
            {
                if(Vector3.Distance(click.point, target.transform.position) >= 0.5)
                    continue;

                if(current + 1 == target.id)
                    correct++;

                current = target.id;
                Debug.LogFormat("current {0} crrect {1}", current, correct);
            }
        }

        Debug.LogFormat("Correct: {0}", correct);

        return correct / targets.Count;
    }

    public double[] GetClicks()
    {
        List<double> time_stamps = new();

        foreach (Sample click in clicks)
            time_stamps.Add(click.time_stamp - begin_time);    

        return time_stamps.ToArray();
    }

    public Vector2[] GetSpeedGraph()
    {
        List<Vector2> graph = new();
        Sample first_sample = samples[0];
        Sample previous_sample = first_sample;

        for (int i = 0; i < samples.Count; i++)
        {               
            double distance = Vector3.Distance(previous_sample.point, samples[i].point);
            double delta_time = samples[i].time_stamp - previous_sample.time_stamp;
            
            double time = samples[i].time_stamp - first_sample.time_stamp;
            double speed = distance / delta_time;

            if(delta_time == 0)
                continue;
            
            previous_sample = samples[i];

            Vector2 graph_point = new((float)time, (float)speed);
            graph.Add(graph_point);
        }
        
        graph = NormalizeData(graph);
        graph = Smooth(graph);
    
        return graph.ToArray();
    }

    public Vector2[] GetAccelerationGraph()
    {
        List<Vector2> graph = new();
        Vector2[] velocity = GetSpeedGraph();

        Vector2 first_sample = velocity[0];
        Vector2 previous_sample = first_sample;

        for (int i = 0; i < velocity.Length; i++)
        {               
            Vector2 delta = velocity[i] - previous_sample; 
            
            if(delta.x == 0)
                continue;

            double acceleration = delta.y / delta.x;
            
            previous_sample = velocity[i];

            Vector2 graph_point = new((float)velocity[i].x, (float)acceleration);
            graph.Add(graph_point);
        }
            
        return graph.ToArray();
    }


    /// <summary>
    /// Performs z normalization on y component of data in list
    /// </summary>
    /// <param name="data">raw data to be normalized</param>
    /// <returns>Normalized array</returns>
    public static List<Vector2> NormalizeData(List<Vector2> data)
    {
        // Compute mean
        float sum = 0;
        foreach (Vector2 graph_point in data)
            sum += graph_point.y;
        
        float mean = sum / data.Count;

        // Compute stdev
        float ss = 0;
        foreach (Vector2 graph_point in data)
            ss += Mathf.Pow(graph_point.y - mean, 2);

        float stdev = Mathf.Sqrt(ss / (data.Count - 1));

        // Normalize data
        for (int i = 0; i < data.Count; i++)
        {
            Vector2 original = data[i];
            data[i] = new Vector2(original.x, (original.y - mean) / stdev);
        }

        // Remove outliers
        int removed = data.RemoveAll( point => Mathf.Abs(point.y) > 3);

        Debug.LogFormat("Removed during normalization: {0}", removed);
        return data;
    }

    public static List<Vector2> Smooth(List<Vector2> data)
    {
        int SMOOTHING_FACTOR = 5;
        for (int i = 0; i < data.Count; i++)
        {
            float sum = 0;
            int used = 0;
            for (int offset = 0; offset < SMOOTHING_FACTOR; offset++)
            {

                if(i - offset < 0)
                    continue;

                sum += data[i - offset].y;
                used++;
            }

            Vector2 smoothed = data[i];
            smoothed.y = sum / used; 
            data[i] = smoothed;
        }

        return data;
    }
}