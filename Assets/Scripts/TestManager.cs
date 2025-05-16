using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Class containing all UI logic and input processing.
/// </summary>
public class TestManager : MonoBehaviour
{
    /// <summary>
    /// Start menu UI document
    /// </summary>
    [SerializeField] UIDocument start_menu_document;
    
    /// <summary>
    /// Results menu UI document
    /// </summary>
    [SerializeField] UIDocument results_menu_document;
    
    /// <summary>
    /// Start menu UI element reference
    /// </summary>
    VisualElement start_menu;

    /// <summary>
    /// Results menu UI element reference
    /// </summary>
    VisualElement results_menu;

    /// <summary>
    /// SGO reference for result visualization
    /// </summary>
    [SerializeField] TestResultVisualization results;

    /// <summary>
    /// Start button reference
    /// </summary>
    Button start_button;

    /// <summary>
    /// Retry button reference
    /// </summary>
    Button retry_button;

    /// <summary>
    /// Back to main menu button reference
    /// </summary>
    Button btmm_button;

    /// <summary>
    /// Template for history object
    /// </summary>
    [SerializeField] VisualTreeAsset history_template;

    /// <summary>
    /// Cursor train object
    /// </summary>
    [SerializeField] TrailRenderer trail_renderer;

    /// <summary>
    /// TMT instance
    /// </summary>
    [SerializeField] TrailMakingTest trail_making_test;

    /// <summary>
    /// Input system link
    /// </summary>
    Keybinds _keybinds;

    /// <summary>
    /// MenuOpacityChange callback
    /// </summary>
    Action<float> OnMenuOpacityChanged;
    
    /// <summary>
    /// OnTestFinished callback
    /// </summary>
    public Action on_test_finished;

    /// <summary>
    /// Last mouse position
    /// </summary>
    Vector2 last_mouse_position = Vector2.zero;

    /// <summary>
    /// Instance of serializer
    /// </summary>
    Serializer serializer = new();

    /// <summary>
    /// Override of Monobehavior method
    /// </summary>
    void Start()
    {
        // Link start menu
        start_menu = start_menu_document.rootVisualElement.Q<VisualElement>("StartMenuContainer");
        start_button = start_menu.Q<Button>("StartButton");
        start_button.clicked += OnTestBegin;

        // Link results menu
        results_menu = results_menu_document.rootVisualElement;
                
        retry_button = results_menu.Q<Button>("TryAgain");
        retry_button.clicked += Repeat;
        results_menu.visible = false;   // This does not flush all binded events

        OnMenuOpacityChanged += SetMenuOpactity;
        on_test_finished += OnTestFinished;

        ListView history = start_menu_document.rootVisualElement.Q<ListView>("History");
        history.reorderable = false;
        history.selectionType = SelectionType.None;
        history.makeItem = () => 
        {
            var history_item = history_template.Instantiate();
            return history_item;
        };

        var results = serializer.GetAllResults();
        history.bindItem = (element, index) =>
        {
            string[] label_ids = {
                "Score",
                "Duration",
                "Size",
                "Correct",
                "Sureness",
                "Modifier"
                };

            foreach(string id in label_ids)
                element.Q<Label>(id).dataSource = results[index];

            if (float.TryParse(results[index].Score, out float score))
            {
                float hue = score / 1000 * 0.5f;
                hue = Mathf.Clamp(hue, 0, 0.5f);
                element.Q<VisualElement>("CTNR").style.backgroundColor = new(Color.HSVToRGB(hue, 0.3f, 1));
            }
            element.MarkDirtyRepaint();
        };
        history.itemsSource = results;
        history.RefreshItems();
    }

    /// <summary>
    /// Override of Monobehavior method
    /// </summary>
    void Update()
    {
        trail_renderer.enabled = trail_making_test.IsReady;
        if (trail_making_test.IsReady)
            trail_renderer.transform.position = last_mouse_position;
    }

    /// <summary>
    /// OnTestBegin callback
    /// </summary>
    void OnTestBegin()
    {
        Debug.Log("Starting TMT");
        LeanTween.value(gameObject, OnMenuOpacityChanged, 1, 0, 0.25f);
        trail_making_test.TargetCount = start_menu.Q<SliderInt>("TestSize").value;
        trail_making_test.HiddenVariant = start_menu.Q<Toggle>("Hidden").value;
        trail_making_test.AlphaVariant = start_menu.Q<Toggle>("AlphaVariant").value;
        
        trail_making_test.GenerateTargets();
        trail_making_test.IsReady = true;
    }

    /// <summary>
    /// Show results window and set results
    /// </summary>
    public void OnTestFinished()
    {
        Debug.Log("Finishing TMT");

        // Disable test
        trail_making_test.Active = false;
        trail_making_test.IsReady = false;

        // Compute results
        ShowResults();

        // Show results
        results_menu.visible = true;
    }

    /// <summary>
    /// Sets results window
    /// </summary>
    void ShowResults()
    {
        Evaluator evaluator = new(trail_making_test);

        TestResult result = evaluator.GetResult();
      
        results.SpeedGraph = evaluator.GetSpeedGraph();
        results.Clicks = evaluator.GetClicks();

        results.Data = Evaluator.GetPeakApproximation(new System.Collections.Generic.List<Vector2>(results.SpeedGraph)).ToArray();

        results.Duration = result.Duration;
        results.Correct = result.Correct;
        results.Score = result.Score;
        results.Sureness = result.Sureness; 
        results.TestSize = result.Size;
        results.ActiveModifier = result.Modifier;

        serializer.SaveResult(result);
    }

    /// <summary>
    /// Sets opacity of the start menu window
    /// </summary>
    void SetMenuOpactity(float value)
    {
        start_menu.style.opacity = value;

        if (value == 0)
            start_menu_document.enabled = false;
    }

    // === Input management ===

    /// <summary>
    /// Override of Monobehavior method
    /// </summary>
    void Awake()
    {
        _keybinds = new();
    }

    /// <summary>
    /// Override of Monobehavior method
    /// </summary>
    void OnEnable()
    {
        _keybinds.Enable();
        SubscribeEvents();
    }

    /// <summary>
    /// Override of Monobehavior method
    /// </summary>
    void OnDisable()
    {
        _keybinds.Disable();
        UnsubscribeEvents();
    }

    /// <summary>
    /// Add callbacks from input system
    /// </summary>
    void SubscribeEvents()
    {
        _keybinds.TrailMakingTest.MousePosition.performed += ProcessInput;
        _keybinds.TrailMakingTest.MouseButtonDown.performed += OnFirePerformed;
    }

    /// <summary>
    /// Remove callbacks from input system
    /// </summary>
    void UnsubscribeEvents()
    {
        _keybinds.TrailMakingTest.MousePosition.performed -= ProcessInput;
        _keybinds.TrailMakingTest.MouseButtonDown.performed -= OnFirePerformed;
    }

    /// <summary>
    /// Mouse click processing callback
    /// </summary>
    void OnFirePerformed(InputAction.CallbackContext context)
    {
        if (trail_making_test.IsReady && !trail_making_test.Active)
        {
            trail_making_test.Active = true;
            trail_making_test.OnTestBegin();
        }

        if (!trail_making_test.Active)
            return;

        Sample click = new(last_mouse_position, Time.realtimeSinceStartupAsDouble);
        trail_making_test.AddClick(click);
    }

    /// <summary>
    /// Mouse input processing callback
    /// </summary>
    void ProcessInput(InputAction.CallbackContext context)
    {
        Vector2 screen_point = context.ReadValue<Vector2>();
        last_mouse_position = Camera.main.ScreenToWorldPoint(screen_point);

        if (!trail_making_test.Active)
            return;

        Sample sample = new(last_mouse_position, Time.realtimeSinceStartupAsDouble);
        trail_making_test.AddSample(sample);
    }

    /// <summary>
    /// Realoads the test scene 
    /// </summary>
    void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("main menu");
    }

    /// <summary>
    /// Reloads the test scene
    /// </summary>
    void Repeat()
    {
        SceneManager.LoadScene("TrailMakingTest");
        Debug.Log("repeat");
    }
}
