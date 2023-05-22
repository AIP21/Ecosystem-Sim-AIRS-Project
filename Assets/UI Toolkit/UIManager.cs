using System.Collections;
using System.Collections.Generic;
using DayTime;
using Managers;
using SimDataStructure;
using TreeGrowth;
using UnityEngine;
using UnityEngine.UIElements;
using WaterSim;
using Weather;
using UnityEngine.SceneManagement;
using Managers.Interfaces;
using System.Linq;
using Graphing;

public class UIManager : MonoBehaviour, ITickableSystem
{
    public UIDocument UIDocument;

    public TimeManager timeManager;
    public DataStructure dataStructure;
    public DataStructureDebug dataStructureDebug;
    public SystemsManager systemsManager;
    public WaterSimulation waterSim;
    public WaterSimDebug waterSimDebug;
    public TreeManager treeManager;
    public WeatherSim weatherSim;

    [Space(10)]
    public GameObject TreeRender;
    public GameObject TreeNoRender;

    [Space(10)]
    private LineChart ageChart;
    private LineChart ageChartNonzero;
    private LineChart maxAgeChart;

    private VisualElement controlPanel;
    private VisualElement metricsPanel;
    private VisualElement timePanel;
    private VisualElement weatherGraphsPanel;
    private VisualElement treeGraphsPanel;

    public float TickPriority { get { return 5; } }
    public int TickInterval { get { return 5; } }
    public int ticksSinceLastTick { get; set; }
    public bool willTickNow { get; set; }
    public bool shouldTick { get { return this.isActiveAndEnabled; } }

    private void Start()
    {
        // ageChart = UIDocument.rootVisualElement.Q<LineChart>("AvgAge");
        // ageChart.EnableGraph(100);
        // ageChart.AddLine("age", Color.red);

        // ageChartNonzero = UIDocument.rootVisualElement.Q<LineChart>("AvgAgeNonzero");
        // ageChartNonzero.EnableGraph(100);
        // ageChartNonzero.AddLine("age", Color.red);

        // maxAgeChart = UIDocument.rootVisualElement.Q<LineChart>("MaxAge");
        // maxAgeChart.EnableGraph(100);
        // maxAgeChart.AddLine("age", Color.red);

        controlPanel = UIDocument.rootVisualElement.Q<VisualElement>("ControlPanel");
        metricsPanel = UIDocument.rootVisualElement.Q<VisualElement>("Metrics");
        timePanel = UIDocument.rootVisualElement.Q<VisualElement>("Time");
        weatherGraphsPanel = UIDocument.rootVisualElement.Q<VisualElement>("Graphs");
        treeGraphsPanel = UIDocument.rootVisualElement.Q<VisualElement>("TreeGraphs");

        this.setupButtonActions();

        this.setupTimePanel();

        Toggle waterToggle = this.controlPanel.Q<Toggle>("WaterToggle");
        waterToggle.RegisterValueChangedCallback((evt) => this.toggleWaterSim(evt.newValue));

        Toggle weatherToggle = this.controlPanel.Q<Toggle>("WeatherToggle");
        weatherToggle.RegisterValueChangedCallback((evt) => this.toggleWeatherSim(evt.newValue));

        Toggle treeToggle = this.controlPanel.Q<Toggle>("TreeToggle");
        treeToggle.RegisterValueChangedCallback((evt) => this.toggleTreeSim(evt.newValue));

        Toggle rendTreeToggle = this.controlPanel.Q<Toggle>("RendTreeToggle");
        rendTreeToggle.RegisterValueChangedCallback((evt) => this.toggleRenderTree(evt.newValue));

        Toggle drawWaterToggle = this.controlPanel.Q<Toggle>("DrawWaterToggle");
        drawWaterToggle.RegisterValueChangedCallback((evt) => this.toggleDrawWater(evt.newValue));
    }

    private void setupButtonActions()
    {
        var simBox = this.controlPanel.Q<GroupBox>("SimButtons");
        Button haltResume = simBox.Q<Button>("HaltResume");
        haltResume.clicked += () => this.haltResume();

        Button resetButton = simBox.Q<Button>("Reset");
        resetButton.clicked += () => this.reset();

        var timeBox = this.controlPanel.Q<GroupBox>("TimeButtons");
        Button newDay = timeBox.Q<Button>("NewDay");
        newDay.clicked += () => timeManager.NewDay();

        Button playPause = timeBox.Q<Button>("PlayPause");
        playPause.clicked += () => this.playPause();
    }

    private void setupTimePanel()
    {
        Slider timeOfDaySlider = this.timePanel.Q<Slider>("DaySlider");
        timeOfDaySlider.RegisterValueChangedCallback((evt) => this.timeManager.SetTimeOfDay(evt.newValue));

        SliderInt dayOfYearSlider = this.timePanel.Q<SliderInt>("YearSlider");
        dayOfYearSlider.RegisterValueChangedCallback((evt) => this.timeManager.SetDayOfYear(evt.newValue));

        TextField dayLengthInput = this.timePanel.Q<TextField>("DayLength");
        dayLengthInput.RegisterValueChangedCallback((evt) => this.timeManager.SetDayLength(evt.newValue));
    }

    private void reset()
    {
        Debug.Log("Resetting");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void haltResume()
    {
        Button btn = this.controlPanel.Q<GroupBox>("SimButtons").Q<Button>("HaltResume");

        if (systemsManager.Running)
        {
            systemsManager.Halt();

            btn.text = "Resume";
        }
        else
        {
            systemsManager.Resume();

            btn.text = "Halt";
        }
    }

    private void playPause()
    {
        timeManager.ToggleTime();

        Button playPause = this.controlPanel.Q<GroupBox>("TimeButtons").Q<Button>("PlayPause");

        if (timeManager.isActiveAndEnabled)
        {
            playPause.text = "Pause";
        }
        else
        {
            playPause.text = "Play";
        }
    }

    private void toggleWaterSim(bool newValue)
    {
        if (newValue)
        {
            this.waterSim.enabled = true;
        }
        else
        {
            this.waterSim.enabled = false;
        }
    }

    private void toggleWeatherSim(bool newValue)
    {
        if (newValue)
        {
            this.weatherSim.enabled = true;
        }
        else
        {
            this.weatherSim.enabled = false;
        }
    }

    private void toggleTreeSim(bool newValue)
    {
        if (newValue)
        {
            this.treeManager.enabled = true;
        }
        else
        {
            this.treeManager.enabled = false;
        }
    }

    private void toggleRenderTree(bool newValue)
    {
        if (newValue)
        {
            this.treeManager.TestTreePrefab = this.TreeRender;
            this.treeManager.TestParameters.GenerateBranchMesh = true;
            this.treeManager.TestParameters.GenerateLeafMesh = true;
        }
        else
        {
            this.treeManager.TestTreePrefab = this.TreeNoRender;
            this.treeManager.TestParameters.GenerateBranchMesh = true;
            this.treeManager.TestParameters.GenerateLeafMesh = true;
        }
    }

    private void toggleDrawWater(bool newValue)
    {
        if (newValue)
        {
            this.waterSimDebug.enabled = true;
        }
        else
        {
            this.waterSimDebug.enabled = false;
        }
    }

    private bool metricsOpen = true;

    private void toggleMetrics()
    {
        this.metricsOpen = !this.metricsOpen;
    }


    public void BeginTick(float deltaTime)
    {

    }

    public void Tick(float deltaTime)
    {

    }

    public void EndTick(float deltaTime)
    {
        systemsManager.CalculateDebugInfo = metricsOpen;
        dataStructure.CalculateDebugInfo = metricsOpen;

        Foldout tickList = this.metricsPanel.Q<Foldout>("TickList");

        IEnumerable<Label> labels = tickList.contentContainer.Children().OfType<Label>();

        Label tickMetricsText = labels.ElementAt(0);

        string text = "";
        text += "Begin Tick Time: " + systemsManager.BeginTickTime + "\n";
        text += "Tick Time: " + systemsManager.TickTime + "\n";
        text += "End Tick Time: " + systemsManager.EndTickTime + "\n";
        text += "Total Tick Time: " + systemsManager.TotalTickTime;
        tickMetricsText.text = text;


        Foldout dsList = this.metricsPanel.Q<Foldout>("DSList");
        labels = dsList.contentContainer.Children().OfType<Label>();
        Label dsMetricsText = labels.ElementAt(0);

        text = "";
        text += "Grid Reads Per Tick: " + dataStructure.gridReadsPerTick + "\n";
        text += "Grid Writes Per Tick: " + dataStructure.gridWritesPerTick + "\n";
        text += "Grid Activity Per Tick: " + dataStructure.gridActivityPerTick + "\n";

        text += "\n";

        text += "Cell Reads Per Tick: " + dataStructure.cellReadsPerTick + "\n";
        text += "Cell Writes Per Tick: " + dataStructure.cellWritesPerTick + "\n";
        text += "Cell Activity Per Tick: " + dataStructure.cellActivityPerTick + "\n";

        text += "\n";

        text += "Grid Read Time Per Tick: " + dataStructure.gridReadTimePerTick + "\n";
        text += "Grid Write Time Per Tick: " + dataStructure.gridWriteTimePerTick + "\n";

        text += "\n";

        text += "Cell Read Time Per Tick: " + dataStructure.cellReadTimePerTick + "\n";
        text += "Cell Write Time Per Tick: " + dataStructure.cellWriteTimePerTick;

        dsMetricsText.text = text;

        Slider timeOfDaySlider = this.timePanel.Q<Slider>("Day");
        timeOfDaySlider.value = timeManager.TimeOfDay;

        SliderInt dayOfYearSlider = this.timePanel.Q<SliderInt>("Year");
        dayOfYearSlider.value = timeManager.DayOfYear;

        TextField dayLengthInput = this.timePanel.Q<TextField>("DayLength");
        dayLengthInput.value = timeManager.DayLength.ToString();

        // ageChart.AddValue("age", this.treeManager.averageAge);
        // ageChartNonzero.AddValue("age", this.treeManager.averageAgeNonzero);
        // maxAgeChart.AddValue("age", this.treeManager.maxAge);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Camera.main.GetComponent<FreeCamera>().enabled = !Camera.main.GetComponent<FreeCamera>().enabled;

            this.UIDocument.gameObject.SetActive(!this.UIDocument.gameObject.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            this.toggleMetrics();
        }
    }
}