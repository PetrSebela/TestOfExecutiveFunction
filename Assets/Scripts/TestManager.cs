using System;
using UnityEngine;
using UnityEngine.InputSystem;
// using UnityEngine.UI;
using UnityEngine.UIElements;

public class TestManager : MonoBehaviour
{
    [SerializeField] UIDocument start_menu_document;
    [SerializeField] UIDocument results_menu_document;

    VisualElement start_menu;
    VisualElement results_menu;

    [SerializeField] TestResultVisualization results;

    Button start_button;

    [SerializeField] TrailRenderer trail_renderer;

    [SerializeField] TrailMakingTest trail_making_test;

    Keybinds _keybinds;

    Action<float> OnMenuOpacityChanged;
    Action<float> OnResultsOpacityChanged;

    public Action on_test_finished;

    bool fire_down = false;

    Vector2 last_mouse_position = Vector2.zero;

    void Start()
    {
        // Link start menu
        start_menu = start_menu_document.rootVisualElement.Q<VisualElement>("StartMenuContainer");
        start_button = start_menu.Q<Button>("StartButton");
        start_button.clicked += OnTestBegin;

        // Link results menu
        results_menu = results_menu_document.rootVisualElement;
        results_menu.style.opacity = 0;
        results_menu_document.enabled = false;

        OnMenuOpacityChanged += SetMenuOpactity;
        OnResultsOpacityChanged += SetResultsOpacity;

        on_test_finished += OnTestFinished;
    }

    void Update()
    {
        trail_renderer.enabled = trail_making_test.IsReady;
        if (trail_making_test.IsReady)
        {
            trail_renderer.transform.position = last_mouse_position;
        }
    }

    void OnTestBegin()
    {
        Debug.Log("Starting TMT");
        LeanTween.value(gameObject, OnMenuOpacityChanged, 1, 0, 0.25f);
        trail_making_test.GenerateTargets();
        trail_making_test.IsReady = true;
    }

    public void OnTestFinished()
    {
        Debug.Log("Finishing TMT");

        // Disable test
        trail_making_test.Active = false;
        trail_making_test.IsReady = false;

        // Compute results
        ShowResults();

        // Show results
        LeanTween.value(gameObject, OnResultsOpacityChanged, 0, 1, 0.25f);
    }

    void ShowResults()
    {
        Evaluator evaluator = new(trail_making_test.Samples, trail_making_test.Clicks, trail_making_test.Targets);
        results.SpeedGraph = evaluator.GetSpeedGraph();
        results.Clicks = evaluator.GetClicks();
        // debug

        results.Data = Evaluator.GetPeakApproximation(new System.Collections.Generic.List<Vector2>(results.SpeedGraph)).ToArray();

        results.Duration = (((int)(evaluator.GetTestDuration() * 100)) / 100f).ToString() + " s";
        results.Correct = (((int)(evaluator.GetCorrectness() * 10000)) / 100f).ToString() + "%";
        results.Score = (((int)(evaluator.GetScore() * 100)) / 100f).ToString();
        results.AccelerationGraph = evaluator.GetAccelerationGraph();
        results.Sureness = ((int)(evaluator.GetConfidence() * 100)).ToString() + "%";
    }

    void SetMenuOpactity(float value)
    {
        start_menu.style.opacity = value;

        if (value == 0)
            start_menu_document.enabled = false;
    }

    void SetResultsOpacity(float value)
    {
        results_menu_document.enabled = true;
        results_menu.style.opacity = value;
    }

    // === Input management ===

    void Awake()
    {
        _keybinds = new();
    }

    void OnEnable()
    {
        _keybinds.Enable();
        SubscribeEvents();
    }

    void OnDisable()
    {
        _keybinds.Disable();
        UnsubscribeEvents();
    }

    void SubscribeEvents()
    {
        _keybinds.TrailMakingTest.MousePosition.performed += ProcessInput;
        _keybinds.TrailMakingTest.MouseButtonDown.performed += OnFirePerformed;
    }

    void UnsubscribeEvents()
    {
        _keybinds.TrailMakingTest.MousePosition.performed -= ProcessInput;
        _keybinds.TrailMakingTest.MouseButtonDown.performed -= OnFirePerformed;
    }

    void OnFirePerformed(InputAction.CallbackContext context)
    {
        if (trail_making_test.IsReady && !trail_making_test.Active)
            trail_making_test.Active = true;

        if (!trail_making_test.Active)
            return;

        Sample click = new(last_mouse_position, Time.realtimeSinceStartupAsDouble, true);
        trail_making_test.AddClick(click);
    }

    void ProcessInput(InputAction.CallbackContext context)
    {
        Vector2 screen_point = context.ReadValue<Vector2>();
        last_mouse_position = Camera.main.ScreenToWorldPoint(screen_point);

        if (!trail_making_test.Active)
            return;

        Sample sample = new(last_mouse_position, Time.realtimeSinceStartupAsDouble, false);
        trail_making_test.AddSample(sample);
    }
}
