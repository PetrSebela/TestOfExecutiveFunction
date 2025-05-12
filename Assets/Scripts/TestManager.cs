using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class TestManager : MonoBehaviour
{
    [SerializeField] UIDocument start_menu_document;
    [SerializeField] UIDocument results_menu_document;

    VisualElement start_menu;
    VisualElement results_menu;

    [SerializeField] TestResultVisualization results;

    Button start_button;

    Button retry_button;
    Button btmm_button;

    [SerializeField] VisualTreeAsset history_template;

    [SerializeField] TrailRenderer trail_renderer;

    [SerializeField] TrailMakingTest trail_making_test;

    Keybinds _keybinds;

    Action<float> OnMenuOpacityChanged;
    public Action on_test_finished;

    Vector2 last_mouse_position = Vector2.zero;

    Serializer serializer = new();

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

    void Update()
    {
        trail_renderer.enabled = trail_making_test.IsReady;
        if (trail_making_test.IsReady)
            trail_renderer.transform.position = last_mouse_position;
    }

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

    void SetMenuOpactity(float value)
    {
        start_menu.style.opacity = value;

        if (value == 0)
            start_menu_document.enabled = false;
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
        {
            trail_making_test.Active = true;
            trail_making_test.OnTestBegin();
        }

        if (!trail_making_test.Active)
            return;

        Sample click = new(last_mouse_position, Time.realtimeSinceStartupAsDouble);
        trail_making_test.AddClick(click);
    }

    void ProcessInput(InputAction.CallbackContext context)
    {
        Vector2 screen_point = context.ReadValue<Vector2>();
        last_mouse_position = Camera.main.ScreenToWorldPoint(screen_point);

        if (!trail_making_test.Active)
            return;

        Sample sample = new(last_mouse_position, Time.realtimeSinceStartupAsDouble);
        trail_making_test.AddSample(sample);
    }

    void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Debug.Log("main menu");
    }

    void Repeat()
    {
        SceneManager.LoadScene("TrailMakingTest");
        Debug.Log("repeat");
    }
}
