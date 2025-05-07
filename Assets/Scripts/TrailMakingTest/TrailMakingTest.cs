using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
public class TrailMakingTest : MonoBehaviour
{
    [SerializeField] GameObject _targetPrefab;
    [SerializeField] TestManager _test_manager;
    [SerializeField] Transform _targetParent;

    [Header("Test settings")]
    public int TargetCount = 6;
    public bool HiddenVariant = false;

    public bool IsReady = false;
    public bool Active = false;

    public List<Sample> Clicks = new();
    public List<Sample> Samples = new();
    public List<Target> Targets = new();

    public void OnTestBegin()
    {

        if(!HiddenVariant)
            return;

        foreach (Target target in Targets)
            target.HideText();
    }

    public void GenerateTargets()
    {
        IsReady = true;

        for (int i = 0; i < TargetCount; i++)
        {
            // Position
            GameObject target_object = Instantiate(_targetPrefab);
            Target target = target_object.GetComponent<Target>();
            target.SetRandomPosition(_targetParent, Targets);

            float color_hue = Random.value;

            // Target colors
            Color primary_color = Color.HSVToRGB( color_hue, 0.3f, 1);
            Color secondary_color = Color.HSVToRGB( color_hue, 0.4f, 0.6f);

            // Init target
            target.Init(i, (i + 1).ToString(), primary_color, secondary_color);

            Targets.Add(target);
        }
    }

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
    public void AddSample(Sample sample)
    {
        Samples.Add(sample);
    }

    public bool CollidesWithTarget(Vector3 position, int target_index)
    {
        if (target_index < 0 || target_index >= TargetCount)
            return false;
        position.z = 0;
        return Vector3.Distance(position, Targets[target_index].transform.position) < 0.5;
    }
}