using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Class used for serializing test data
/// </summary>
public class Serializer
{
    /// <summary>
    /// Path wehere the results will be stored
    /// </summary>
    string path = "";

    /// <summary>
    /// Initialize test result serializer
    /// </summary>
    public Serializer()
    {
        path = Path.Combine(Application.dataPath, "results");
        if(!Directory.Exists(path))
            Directory.CreateDirectory(path); 
    }

    /// <summary>
    /// Saves results to common director at app root
    /// </summary>
    /// <param name="result"> Test result to be serialized </param>
    public void SaveResult(TestResult result)
    {
        string json = JsonUtility.ToJson(result, true);
        string name = result.GetHashCode().ToString() + ".result";
        string full_path = Path.Combine(path, name);
        File.WriteAllText(full_path, json);
    }

    /// <summary>
    /// Loads all stored results
    /// </summary>
    /// <returns> List of all deserialized test results from common directory </returns>
    public List<TestResult> GetAllResults()
    {
        List<TestResult> results = new();

        string[] result_files = Directory.GetFiles(path, "*.result");

        foreach (string full_path in result_files)
        {
            Debug.Log(full_path);
            string json = File.ReadAllText(full_path);
            TestResult result = JsonUtility.FromJson<TestResult>(json);
            results.Add(result);
        }

        results.Sort( (a, b) => a.Score.CompareTo(b.Score));

        return results;
    }
}

/// <summary>
/// Struct for serialization of test results
/// </summary>
[System.Serializable]
public struct TestResult
{
    public string Score;
    public string Duration;
    public string Correct;
    public string Sureness;
    public string Size;
    public string Modifier;
}