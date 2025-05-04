using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System.Security.Cryptography;

public class Target : MonoBehaviour
{
    const int PADDING = 250;

    [SerializeField] TMP_Text label;
    [SerializeField] CanvasGroup canvas_group;

    public bool CanBeClicked = true;

    public int id = -1;
    Action<float> on_alpha_change;

    void Start()
    {
        on_alpha_change += OnAlphaChange;
    }

    public void SetOrder(int id, string text)
    {
        this.id = id;
        label.text = text;
    }

    public void SetRandomPosition(Transform parent, List<Target> forbidden)
    {
        transform.SetParent(parent);

        bool invalid_position = true;
        int attempts_left = 100;
        while (invalid_position && attempts_left > 0)
        {
            transform.localPosition = new(UnityEngine.Random.Range(-1920 + PADDING, 1920 - PADDING) / 2, UnityEngine.Random.Range(-1080 + PADDING, 1080 - PADDING) / 2, 0);

            bool has_collision = false;

            foreach (Target t in forbidden)
                if(Vector3.Distance(t.transform.localPosition, transform.localPosition) < 200)
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
