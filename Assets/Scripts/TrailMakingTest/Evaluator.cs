
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for evaluation of test data captured during TMT
/// </summary>
public class Evaluator
{
    /// <summary>
    /// List of all captures mouse positions, in order they were captured
    /// </summary>
    List<Sample> samples; 

    /// <summary>
    /// List of all captured clicks, in order they were captured
    /// </summary>
    List<Sample> clicks;

    /// <summary>
    /// List of all targets used during the test
    /// </summary>
    List<Target> targets;

    /// <summary>
    /// Flag signalizing the use of hidden test variant
    /// </summary>
    bool hidden_variant;

    /// <summary>
    /// Flag signalizing the use of alpha test variant
    /// </summary>
    bool alpha_variant;


    /// <summary>
    /// Start time of the test (timestamp of the first click)
    /// </summary>
    double begin_time = 0;

    /// <summary>
    /// Constructor for Evaluator class, returns instance of evaluator 
    /// </summary>
    /// <param name="samples">          Captured mouse positions            </param>
    /// <param name="clicks">           Captured clicks                     </param>
    /// <param name="targets">          Used targets                        </param>
    /// <param name="hidden_variant">   Was the hidden test variant used    </param>
    public Evaluator(TrailMakingTest test)
    {
        this.samples = test.Samples;
        this.clicks = test.Clicks;
        this.targets = test.Targets;
        this.hidden_variant = test.HiddenVariant;
        this.alpha_variant = test.AlphaVariant;
        
        begin_time = clicks[0].time_stamp;  // Test is stated by first clicks

        Debug.LogFormat("Test duration {0}", GetTestDuration());
        Debug.LogFormat("Correctness {0}", GetCorrectness());
        Debug.LogFormat("Correctness {0}", GetCorrectness());
        Debug.LogFormat("Surness {0}", GetConfidence());
    }

    /// <returns> Duration of evaluated test </returns>
    public double GetTestDuration()
    {
        return clicks[^1].time_stamp - clicks[0].time_stamp;
    }

    /// <summary>
    /// Computes test score
    /// </summary>
    /// <returns> Computed test score </returns>
    public double GetScore()
    {
        double score = targets.Count * 1.5f / GetTestDuration() * GetCorrectness() * GetConfidence() * GetModifiers() * 100;
        return score;
    }

    /// <summary>
    /// Fills out struct respresenting test result
    /// </summary>
    /// <returns> Fillded out test result struct </returns>
    public TestResult GetResult()
    {
        TestResult result;
        
        result.Duration =   (((int) (GetTestDuration()  * 100))     / 100f  ).ToString() + " s" ;
        result.Correct =    (((int) (GetCorrectness()   * 10000))   / 100f  ).ToString() + "%"  ;
        result.Score =      (((int) (GetScore()         * 100))     / 100f  ).ToString()        ;
        result.Sureness =   ((int)  (GetConfidence()    * 100)              ).ToString() + "%"  ;
        
        result.Size = targets.Count.ToString();
        result.Modifier = GetModifiers().ToString() + "x";

        return result;
    }

    /// <summary>
    /// Computes all used modifiers
    /// </summary>
    /// <returns> Sum of all used modifiers </returns>
    public double GetModifiers()
    {
        float modifier = 1;
        modifier *= hidden_variant ? Mathf.Pow(1.1f, targets.Count) : 1; 
        modifier *= alpha_variant ? Mathf.Pow(1.05f, targets.Count) : 1; 

        modifier = (int)(modifier * 100)/100f;
        return modifier;
    }

    /// <summary>
    /// Function predicting subject confidence by counting velocity peaks in between clicks 
    /// </summary>
    /// <returns> Subject confidence modifier </returns>
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

            confidence += count <= 1 ? 1 : 0.5f / count + 0.5f; // Confidence will never go below 0.5 (might as well be random before then, this will hopefully motivate the user to keep trying)
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
                // Click collision detection
                if(Vector3.Distance(click.point, target.transform.position) >= 0.5)
                    continue;

                if(current + 1 == target.id)
                    correct++;

                current = target.id;
            }
        }

        return correct / targets.Count;
    }

    /// <summary>
    /// Get click with timestamp beginning at 0s, relative sample distance is kept same
    /// </summary>
    /// <returns> Array of time normalized clicks </returns>
    public double[] GetClicks()
    {
        List<double> time_stamps = new();

        foreach (Sample click in clicks)
            time_stamps.Add(click.time_stamp - begin_time);    

        return time_stamps.ToArray();
    }

    /// <summary>
    /// Returns points of graph reprezenting velocity over time
    /// </summary>
    /// <returns> Array of velocity graph points </returns>
    public Vector2[] GetSpeedGraph()
    {
        List<Vector2> graph = new();
        Sample first_sample = samples[0];
        Sample previous_sample = first_sample;

        for (int i = 0; i < samples.Count; i++)
        {             
            // Computed derivative  
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
        
        // Perform z normalization (cut out outliers)
        graph = NormalizeData(graph);

        // Apply smoothing (just for visualization)
        graph = Smooth(graph);
    
        return graph.ToArray();
    }

    /// <summary>
    /// Performs z normalization on y component of data in list
    /// </summary>
    /// <param name="data"> raw data to be normalized </param>
    /// <returns> Normalized array </returns>
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

        return data;
    }

    /// <summary>
    /// Performs smoothing by averaging samples withing moving frame
    /// </summary>
    /// <param name="data"> Data </param>
    /// <returns> Smoothed data </returns>
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
    /// Performs "custom" peak detection 
    /// </summary>
    /// <param name="data"> graph points (x = time, y = value) </param>
    /// <returns>List of peak timestamps (not normalized) </returns>
    public static List<double> GetPeakApproximation(List<Vector2> data)
    {
        List<double> peaks = new();
        int FRAME_RADIUS = 25;
        int SIZE = FRAME_RADIUS * 2 + 1;

        // Peak detection by using z score normalization to detect outliers
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

        // Perform clustering of raw peaks
        List<double> clusters = new();
        List<double> cluster = new();
        
        double EPSILON_MAX = 0.125f;

        for (int i = 0; i < peaks.Count; i++)
        {
            double cluster_mean = 0;
            foreach (double item in cluster)
                cluster_mean += item;

            cluster_mean /= cluster.Count;

            if(Mathf.Abs((float)(cluster_mean - peaks[i])) < EPSILON_MAX)
            {
                cluster.Add(peaks[i]);
            }
            else
            {
                clusters.Add(cluster_mean);
                cluster.Clear();
                cluster.Add(peaks[i]);
            }
        }

        return clusters;
    }
}