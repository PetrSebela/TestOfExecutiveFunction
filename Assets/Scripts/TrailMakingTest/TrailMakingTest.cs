using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEngine;
public class TrailMakingTest : MonoBehaviour
{
    [SerializeField] GameObject _targetPrefab;
    [SerializeField] TestManager _test_manager;
    [SerializeField] Transform _targetParent;
        
    const int TARGET_COUNT = 2; 

    public bool IsReady = false;
    public bool Active = false;

    public List<Sample> samples = new();
    List<Target> _targets = new();
    
    public void GenerateTargets()
    {
        IsReady = true;
        for (int i = 0; i < TARGET_COUNT; i++)
        {
            GameObject target_object = Instantiate(_targetPrefab);
            Target target = target_object.GetComponent<Target>();
            target.SetRandomPosition(_targetParent, _targets);
            
            target.SetOrder(i, (i + 1).ToString());
            
            _targets.Add(target);
        };
    }

    public void AddSample(Sample sample)
    {
        samples.Add(sample);

        if(!sample.active)
            return;

        bool anything_active = false;
        foreach (Target target in _targets)
        {
            if(CollidesWithTarget(sample.point, target.id) && target.CanBeClicked)
                target.OnClicked();      

            if(target.CanBeClicked)
                anything_active = true;
        }

        if(!anything_active && Active)
            _test_manager.on_test_finished.Invoke();
    }

    public Vector3[] GetLinePoints(){
        Vector3[] points = new Vector3[samples.Count];
        
        for (int i = 0; i < samples.Count; i++)
        {
            points[i] = samples[i].point;
            points[i].z = 0;
        }

        return points;
    } 

    public bool CollidesWithTarget(Vector3 position, int target_index)
    {
        if(target_index < 0 || target_index >= TARGET_COUNT)
            return false;
        position.z = 0;
        return Vector3.Distance(position, _targets[target_index].transform.position) < 0.5;
    }
}