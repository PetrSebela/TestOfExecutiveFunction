using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    /// <summary>
    /// Minimal distance from screen edges in pixels ( always scaled to be 1920 x 1080 )
    /// </summary>
    const int PADDING = 180;

    /// <summary>
    /// Target label
    /// </summary>
    [SerializeField] TMP_Text label;
    
    /// <summary>
    /// Canvas group of the targer (used for hiding the target)
    /// </summary>
    [SerializeField] CanvasGroup canvas_group;

    /// <summary>
    /// Target fill sprite
    /// </summary>
    [SerializeField] Image fill;

    /// <summary>
    /// Target outline sprite
    /// </summary>
    [SerializeField] Image outline;
    
    /// <summary>
    /// Flag representing whenever the target can be clicked
    /// </summary>
    public bool CanBeClicked = true;

    /// <summary>
    /// ID of the target (by order)
    /// </summary>
    public int id = -1;

    /// <summary>
    /// LeanTween value callback (use for setting alpha smoothly)
    /// </summary>
    Action<float> on_alpha_change;

    void Start()
    {
        on_alpha_change += OnAlphaChange;
    }

    /// <summary>
    /// Initialize test target
    /// </summary>
    /// <param name="id"> order of the target </param>
    /// <param name="text"> target test </param>
    /// <param name="primary"> primary color </param>
    /// <param name="secondary"> secondary color </param>
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

    /// <summary>
    /// Sets target position without collision
    /// </summary>
    /// <param name="parent"> Canvas parent </param>
    /// <param name="forbidden"> List of other targets </param>
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

    /// <summary>
    /// Set target alpha
    /// </summary>
    /// <param name="alpha"> alpha </param>
    void OnAlphaChange(float alpha)
    {
        canvas_group.alpha = alpha;
    }

    /// <summary>
    /// On click callback
    /// </summary>
    public void OnClicked()
    {
        CanBeClicked = false;
        LeanTween.value(gameObject, on_alpha_change, 1, 0, 0.1f);
    }
}
