
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

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
        Debug.LogFormat("Correctness {0}", GetCorrectness());
        Debug.LogFormat("Surness {0}", GetConfidence());
    }

    public double GetTestDuration()
    {
        return clicks[^1].time_stamp - clicks[0].time_stamp;
    }

    public double GetScore()
    {
        double score = targets.Count * 1.5f / GetTestDuration() * GetCorrectness() * GetConfidence() * 100;
        return score;
    }

    public double GetConfidence()
    {
        List<Vector2> speed = new(GetSpeedGraph());
        List<double> peaks = GetPeakApproximation(speed);

        double confidence = 0;

        for (int i = 1; i < clicks.Count; i++)
        {
            double epoch_start = clicks[i - 1].time_stamp - begin_time;
            double epoch_end = clicks[i].time_stamp- begin_time;

            int count = 0;

            foreach (double peak in peaks)
                if(peak > epoch_start && peak < epoch_end)
                    count++;

            if(count <= 1)
            {
                confidence += 1;
                continue;
            }

            confidence += 0.5f/count + 0.5f; // Confidence will never go below 0.5 (might as well be random before then, this will hopefully motivate the user to keep trying)
        }

        return confidence / (clicks.Count - 1);
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
            }
        }

        

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


    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static List<double> GetPeakApproximation(List<Vector2> data)
    {
        List<double> peaks = new();
        int FRAME_RADIUS = 25;
        int SIZE = FRAME_RADIUS * 2 + 1;

        // Peak detection
        for (int i = FRAME_RADIUS; i < data.Count - FRAME_RADIUS; i++)
        {
            float sum = 0;
            for (int j = -FRAME_RADIUS; j < FRAME_RADIUS; j++)
                sum += data[i - j].y;            
            float mean = sum / SIZE;
            

            float ss = 0;
            for (int j = -FRAME_RADIUS; j < FRAME_RADIUS; j++)
                ss += Mathf.Pow(data[i - j].y - mean, 2);

            float stdev = Mathf.Sqrt(ss / (SIZE - 1));

            float local_z_score = (data[i].y - mean) / stdev;

            if(local_z_score <= 0.5f)
                continue;

            peaks.Add(data[i].x);
        }

        // Clustering

        List<double> clusters = new();

        List<double> cluster = new();
        
        double EPSILON = 0.125f;

        for (int i = 0; i < peaks.Count; i++)
        {
            double sum = 0;
            foreach (double item in cluster)
                sum += item;

            double kernel = sum / cluster.Count;


            if(Mathf.Abs((float)(kernel - peaks[i])) < EPSILON)
            {
                cluster.Add(peaks[i]);
            }
            else
            {
                clusters.Add(kernel);
                cluster.Clear();
                cluster.Add(peaks[i]);
            }
        }

        return clusters;
    }
}