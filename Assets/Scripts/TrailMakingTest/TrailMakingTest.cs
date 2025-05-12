using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Component for data collection during testing
/// </summary>
public class TrailMakingTest : MonoBehaviour
{
    /// <summary>
    /// Test target prefab
    /// </summary>
    [SerializeField] GameObject _targetPrefab;
    
    /// <summary>
    /// Reference to test manager
    /// </summary>
    [SerializeField] TestManager _test_manager;
    
    /// <summary>
    /// Parent of test targets
    /// </summary>
    [SerializeField] Transform _targetParent;

    [Header("Test settings")]

    /// <summary>
    /// Count of test targets
    /// </summary>
    public int TargetCount = 6;

    /// <summary>
    /// Flag signalizing use of hidden test variant
    /// </summary>
    public bool HiddenVariant = false;

    /// <summary>
    /// Flag signalizing use of aplha test variant
    /// </summary>
    public bool AlphaVariant = false;

    /// <summary>
    /// Flag signalizing if the test is ready to be started
    /// </summary>
    public bool IsReady = false;

    /// <summary>
    /// Flag signalizing that the test in in progress
    /// </summary>
    public bool Active = false;

    /// <summary>
    /// List of captured clicks
    /// </summary>
    public List<Sample> Clicks = new();

    /// <summary>
    /// List of captured mouse positions
    /// </summary>
    public List<Sample> Samples = new();

    /// <summary>
    /// List of all test targets
    /// </summary>
    public List<Target> Targets = new();

    /// <summary>
    /// On test begin callback
    /// </summary>
    public void OnTestBegin()
    {
        if(!HiddenVariant)
            return;

        foreach (Target target in Targets)
            target.HideText();
    }

    /// <summary>
    /// Generates and initializes all test targets
    /// </summary>
    public void GenerateTargets()
    {
        IsReady = true;

        if(AlphaVariant)
            TargetCount *= 2;

        for (int i = 0; i < TargetCount; i++)
        {
            // Position
            GameObject target_object = Instantiate(_targetPrefab);
            Target target = target_object.GetComponent<Target>();
            target.SetRandomPosition(_targetParent, Targets);

            // Target colors
            float color_hue = Random.value;
            Color primary_color = Color.HSVToRGB( color_hue, 0.3f, 1);
            Color secondary_color = Color.HSVToRGB( color_hue, 0.4f, 0.6f);

            // Init targets
            if(AlphaVariant)
            {
                string target_label = "";

                if(i % 2 == 0)
                    target_label = ((i + 2) / 2).ToString();
                else
                    target_label = ((char)('A' + i / 2)).ToString();

                target.Init(i, target_label, primary_color, secondary_color);

            }
            else
            {
                target.Init(i, (i + 1).ToString(), primary_color, secondary_color);
            }

            Targets.Add(target);
        }
    }

    /// <summary>
    /// Add click to test samples
    /// </summary>
    /// <param name="click"> click sample </param>
    public void AddClick(Sample click)
    {
        Clicks.Add(click);

        bool anything_active = false;
        foreach (Target target in Targets)
        {
            if (CollidesWithTarget(click.point, target.id) && target.CanBeClicked)
                target.OnClicked();

            if (target.CanBeClicked)
                anything_active = true;
        }

        if (!anything_active && Active)
            _test_manager.on_test_finished.Invoke();
    }

    /// <summary>
    /// Add mouse position to test samples
    /// </summary>
    /// <param name="sample"> mouse sample </param>
    public void AddSample(Sample sample)
    {
        Samples.Add(sample);
    }

    /// <summary>
    /// Collision detection with test targets
    /// </summary>
    /// <param name="position"> Mouse position </param>
    /// <param name="target_index"> Target index </param>
    /// <returns> If mouse collides with given test target </returns>
    public bool CollidesWithTarget(Vector3 position, int target_index)
    {
        if (target_index < 0 || target_index >= TargetCount)
            return false;
        position.z = 0;
        return Vector3.Distance(position, Targets[target_index].transform.position) < 0.5;
    }
}