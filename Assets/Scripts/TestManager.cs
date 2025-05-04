using System;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
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

    [SerializeField] TestResult results;

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
        if(trail_making_test.IsReady)
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
        results.Evaluate(trail_making_test.samples);
    }



    void SetMenuOpactity(float value)
    {
        start_menu.style.opacity = value;

        if(value == 0)
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
        _keybinds.TrailMakingTest.MouseButtonDown.canceled += OnFireCancelled;
    }

    void UnsubscribeEvents()
    {
        _keybinds.TrailMakingTest.MousePosition.performed -= ProcessInput;
        _keybinds.TrailMakingTest.MouseButtonDown.performed -= OnFirePerformed;
        _keybinds.TrailMakingTest.MouseButtonDown.canceled -= OnFireCancelled;
    }
    
    void OnFirePerformed(InputAction.CallbackContext context)
    {
        fire_down = true;

        if(trail_making_test.IsReady && !trail_making_test.Active)
            trail_making_test.Active = true;

        if(!trail_making_test.Active)
            return;

        Sample sample = new(last_mouse_position, context.startTime, fire_down);
        trail_making_test.AddSample(sample);
    }

    void OnFireCancelled(InputAction.CallbackContext context)
    {
        fire_down = false;
    }
    
    void ProcessInput(InputAction.CallbackContext context)
    {

        Vector2 screen_point = context.ReadValue<Vector2>();
        last_mouse_position = Camera.main.ScreenToWorldPoint(screen_point);

        if(!trail_making_test.Active)
            return;

        Sample sample = new(last_mouse_position, context.startTime, fire_down);
        trail_making_test.AddSample(sample);
    }
}
