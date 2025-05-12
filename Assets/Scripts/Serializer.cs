using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Serializer
{
    string path = "";

    public Serializer()
    {
        path = Path.Combine(Application.dataPath, "results");

        if(!Directory.Exists(path))
            Directory.CreateDirectory(path); 
    }

    public void SaveResult(TestResult result)
    {
        string json = JsonUtility.ToJson(result, true);

        string name = result.GetHashCode().ToString() + ".result";
        string full_path = Path.Combine(path, name);
        File.WriteAllText(full_path, json);
    }

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