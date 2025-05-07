using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    const int PADDING = 250;

    [SerializeField] TMP_Text label;
    [SerializeField] CanvasGroup canvas_group;

    [SerializeField] Image fill;
    [SerializeField] Image outline;
    
    public bool CanBeClicked = true;

    public int id = -1;
    Action<float> on_alpha_change;

    void Start()
    {
        on_alpha_change += OnAlphaChange;
    }

    public void Init(int id, string text, Color primary, Color secondary)
    {
        this.id = id;
        label.text = text;
        fill.color = primary;
        outline.color = secondary;
    }

    public void HideText()
    {
        label.text = "";
    }

    public void SetRandomPosition(Transform parent, List<Target> forbidden)
    {
        transform.SetParent(parent);

        bool invalid_position = true;
        int attempts_left = 200;
        while (invalid_position && attempts_left > 0)
        {
            transform.localPosition = new(UnityEngine.Random.Range(-1920 + PADDING, 1920 - PADDING) / 2, UnityEngine.Random.Range(-1080 + PADDING, 1080 - PADDING) / 2, 0);

            bool has_collision = false;

            foreach (Target t in forbidden)
                if(Vector3.Distance(t.transform.localPosition, transform.localPosition) < 225)
                    has_collision = true;

            attempts_left--;
            invalid_position = has_collision;
        }
    }

    void OnAlphaChange(float alpha)
    {
        canvas_group.alpha = alpha;
    }

    public void OnClicked()
    {
        CanBeClicked = false;
        LeanTween.value(gameObject, on_alpha_change, 1, 0, 0.1f);
    }
}
