using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;
using SimDataStructure.Data;
using SimDataStructure.Interfaces;
using UnityEngine;
using Utilities;
using Graphing;
using Random = UnityEngine.Random;
using UnityEngine.UIElements;

namespace Weather
{
    //[ExecuteInEditMode]
    public class WeatherSim : MonoBehaviour, ITickableSystem, ISetupGridData, IReadGridData, IWriteGridData
    {
        #region Variables
        public LTDescr delay, tween;

#if UNITY_EDITOR
        [SerializeField][HideInInspector] private bool referencesHeaderGroup;
        [SerializeField][HideInInspector] private bool controlHeaderGroup;
        [SerializeField][HideInInspector] private bool temperatureHeaderGroup;
        [SerializeField][HideInInspector] private bool lightningHeaderGroup;
        [SerializeField][HideInInspector] private bool weatherHeaderGroup;
        [SerializeField][HideInInspector] private bool blendingHeaderGroup;
        [SerializeField][HideInInspector] private bool otherHeaderGroup;
#endif

        #region Public
        #region TEMPORARY
        public Transform sunTransform;
        public float precipDurationMultiplier = 1.0f;

        public PrecipitationManager precipitationManager;
        #endregion

        #region Value containers
        [Header("Value containers")]
        public WeatherGridData TodayVals;
        public WeatherGridData YesterdayVals;
        #endregion

        //         #region Script references
        //         // [Header("Script references")]
        //         [SerializeField][HideInInspector] private AzureWeatherController SkyController;
        //         [SerializeField][HideInInspector] private AzureTimeController TimeController;
        //         #endregion

        #region Control variables
        [Header("Control variables")]
        [SerializeField][HideInInspector] public bool IsWeather;
        [SerializeField][HideInInspector] private bool ComputeValues = true;
        [SerializeField][HideInInspector] private bool ApplyValues = true;
        [SerializeField][HideInInspector] private bool DoWeather = true;
        [SerializeField][HideInInspector] private int StormCooldown;
        #endregion

        #region Temperature variables
        [Header("Temperature variables")]
        [SerializeField][HideInInspector] private AnimationCurve SeasonTempCurve;
        [SerializeField][HideInInspector] private AnimationCurve DayTempCurve;
        [SerializeField][HideInInspector] private LayerMask ShadowCasterMask;
        [SerializeField][HideInInspector] private bool InSun;
        private Transform shadowReceiver;
        private float currentSunExposure;
        #endregion

        #region Lightning spawning
        [Header("Lightning spawning")]
        [Range(0, 100)]
        [SerializeField][HideInInspector] private float LightningSpawnChance = 3;
        [SerializeField][HideInInspector] private Bounds LightningSpawnBounds;
        [SerializeField][HideInInspector] private GameObject LightningPrefab;
        [SerializeField][HideInInspector] private Transform LightningParent;
        [SerializeField][HideInInspector] private int LightningPoolSize = 25;
        private List<GameObject> pooledLightning = new List<GameObject>();
        #endregion

        #region Weather Creation
        [SerializeField][HideInInspector] private bool CreateRandomWeather;
        [SerializeField][HideInInspector] private bool CreateRain;
        [SerializeField][HideInInspector] private bool CreateThunder;
        [SerializeField][HideInInspector] private bool CreateSnow;
        [SerializeField][HideInInspector] private float CreateWeatherIntensity;
        [SerializeField][HideInInspector] private float CreateWeatherTemp;
        [SerializeField][HideInInspector] private float CreateWeatherDuration;
        #endregion

        #region Lighting blending gradients
        [Header("Lighting blending gradients")]
        [SerializeField][HideInInspector] private Gradient EnvironmentColorNormal;
        [SerializeField][HideInInspector] private Gradient EnvironmentColorRain;
        [SerializeField][HideInInspector] private Gradient DirectionalLightColorNormal;
        [SerializeField][HideInInspector] private Gradient DirectionalLightColorRain;
        #endregion

        #region Sky blending gradients
        [Header("Sky blending gradients")]
        [SerializeField][HideInInspector] private Gradient RayleighColorNormal;
        [SerializeField][HideInInspector] private Gradient RayleighColorRain;
        [SerializeField][HideInInspector] private Gradient MieColorNormal;
        [SerializeField][HideInInspector] private Gradient MieColorRain;
        #endregion

        #region Cloud blending gradients
        [Header("Cloud blend gradients")]
        [SerializeField][HideInInspector] private Gradient DynamicCloudsColor1Normal;
        [SerializeField][HideInInspector] private Gradient DynamicCloudsColor2Normal;
        [SerializeField][HideInInspector] private Gradient DynamicCloudsColor1Rain;
        [SerializeField][HideInInspector] private Gradient DynamicCloudsColor2Rain;
        #endregion

        #region Lighting blending curves
        [Header("Lighting blending curves")]
        [SerializeField][HideInInspector] private AnimationCurve DirectionalLightIntensityNormal;
        [SerializeField][HideInInspector] private AnimationCurve DirectionalLightIntensityRain;
        [SerializeField][HideInInspector] private AnimationCurve EnvironmentIntensityNormal;
        [SerializeField][HideInInspector] private AnimationCurve EnvironmentIntensityRain;
        #endregion

        #region Sky blending curves
        [Header("Sky blending curves")]
        [SerializeField][HideInInspector] private AnimationCurve RayleighCurveNormal;
        [SerializeField][HideInInspector] private AnimationCurve RayleighCurveRain;
        [SerializeField][HideInInspector] private AnimationCurve MieCurveNormal;
        [SerializeField][HideInInspector] private AnimationCurve MieCurveRain;
        [SerializeField][HideInInspector] private AnimationCurve StarsIntensityNormal;
        [SerializeField][HideInInspector] private AnimationCurve StarsIntensityRain;
        [SerializeField][HideInInspector] private AnimationCurve MilkyWayIntensityNormal;
        [SerializeField][HideInInspector] private AnimationCurve MilkyWayIntensityRain;
        #endregion

        #region Extra Computation Variables
        [Header("Extra computation variables")]
        [SerializeField][HideInInspector] private float BaseTempDifference;
        [SerializeField][HideInInspector] private float BaseHumidityDifference;
        #endregion
        #endregion

        #region External integration
        [HideInInspector]
        public float zaraRainIntensity;
        [HideInInspector]
        public float zaraTemperature;
        [HideInInspector]
        public float zaraWindSpeed;

        public UIDocument uiDocument;
        public LineChart baseChart;
        public LineChart actualChart;
        public LineChart precipChart;
        public LineChart precipTypeChart;
        public LineChart fogCloudsChart;
        public LineChart dayTempChart;
        public LineChart dayHumidityChart;
        public LineChart windSpeedChart;
        public LineChart windDirChart;
        #endregion

        #region Private
        private float timeOfDay;
        private int dayOfYear;
        private int ticksPerDay;

        private Dictionary<Tuple<string, int>, object> data = new Dictionary<Tuple<string, int>, object>();
        private Tuple<string, int> weatherYesterdayKey = new Tuple<string, int>("weatherYesterday", 2);
        private Tuple<string, int> weatherTodayKey = new Tuple<string, int>("weatherToday", 2);
        private Tuple<string, int> rainStrengthKey = new Tuple<string, int>("rainStrength", 2);

        #region Interface Stuff
        private Dictionary<string, int> _readDataNames = new Dictionary<string, int>() {
            { "timeOfDay", 2 },
            { "dayOfYear", 2 },
            { "newDay", 2 },
            { "ticksPerDay", 2}
        };  // The names of the grid data this is reading from the data structure, along with its grid level
        public Dictionary<string, int> ReadDataNames { get { return _readDataNames; } }

        private Dictionary<string, int> _writeDataNames = new Dictionary<string, int>(){
            { "weatherYesterday", 2 },
            { "weatherToday", 2 },
            { "rainStrength", 2 },
        };  // The names of the grid data this is writing to the data structure, along with its grid level
        public Dictionary<string, int> WriteDataNames { get { return _writeDataNames; } }

        public float TickPriority { get { return 3; } }
        public int TickInterval { get { return 0; } } // 20
        public int ticksSinceLastTick { get; set; }
        public bool willTickNow { get; set; }
        public bool shouldTick { get { return this.isActiveAndEnabled; } }
        #endregion
        #endregion
        #endregion

        #region Unity Methods
        private void Start()
        {
            // Set up ui references
            // baseChart = uiDocument.rootVisualElement.Q<LineChart>("baseChart");
            // actualChart = uiDocument.rootVisualElement.Q<LineChart>("actualChart");
            // precipChart = uiDocument.rootVisualElement.Q<LineChart>("precipChart");
            // precipTypeChart = uiDocument.rootVisualElement.Q<LineChart>("precipTypeChart");
            // fogCloudsChart = uiDocument.rootVisualElement.Q<LineChart>("fogCloudsChart");
            // dayTempChart = uiDocument.rootVisualElement.Q<LineChart>("dayTempChart");
            // dayHumidityChart = uiDocument.rootVisualElement.Q<LineChart>("dayHumidityChart");
            // windSpeedChart = uiDocument.rootVisualElement.Q<LineChart>("windSpeedChart");
            // windDirChart = uiDocument.rootVisualElement.Q<LineChart>("windDirChart");

            // // Set up graphs
            // baseChart.EnableGraph(365);
            // actualChart.EnableGraph(365);
            // precipChart.EnableGraph(365);
            // precipTypeChart.EnableGraph(365);
            // fogCloudsChart.EnableGraph(365);
            // dayTempChart.EnableGraph(29);
            // dayHumidityChart.EnableGraph(29);
            // windSpeedChart.EnableGraph(29);
            // windDirChart.EnableGraph(29);

            // // Set up lines
            // baseChart.AddLine("baseTemp", Color.red);
            // baseChart.AddLine("actualTemp", Color.blue);
            // actualChart.AddLine("baseHumidity", Color.red);
            // actualChart.AddLine("actualHumidity", Color.blue);
            // precipChart.AddLine("intensity", Color.red, true);
            // precipChart.AddLine("temp", Color.green, true);
            // precipChart.AddLine("duration", Color.blue, true);
            // precipChart.AddLine("chance", Color.yellow, true);
            // precipTypeChart.AddLine("type", Color.red);
            // dayTempChart.AddLine("temp", Color.red);
            // dayHumidityChart.AddLine("humidity", Color.red);
            // windSpeedChart.AddLine("speed", Color.red);
            // windDirChart.AddLine("direction", Color.red);
            // fogCloudsChart.AddLine("fog", Color.red);
            // fogCloudsChart.AddLine("cloudCover", Color.blue);

            shadowReceiver = Utils.PrimaryCamera.transform;
            TodayVals.PrecipIntensity = 0;
            this.precipitationManager.rain.amount = 0.0f;
            this.precipitationManager.snow.amount = 0.0f;
            TodayVals.isThundering = false;
            TodayVals.isRaining = false;
            TodayVals.isSnowing = false;
            tween = LeanTween.delayedCall(0, () => { });
            delay = LeanTween.delayedCall(0, () => { });

            // pooledLightning = new List<GameObject>();
            // for (int i = 0; i < LightningPoolSize; i++)
            // {
            //     GameObject obj = (GameObject)Instantiate(LightningPrefab);
            //     obj.transform.parent = LightningParent;
            //     obj.SetActive(false);
            //     pooledLightning.Add(obj);
            // }

            NewDay();
        }

        // Reset called by the inspector button
        public void ResetEditor()
        {
            TodayVals.ResetEditor();
            // i = 0;
            // SkyController.SetDefaultWeatherProfile(1);
            this.precipitationManager.rain.amount = 0.0f;
            this.precipitationManager.snow.amount = 0.0f;
            TodayVals.isThundering = false;
            TodayVals.isRaining = false;
            TodayVals.isSnowing = false;
            BaseTempDifference = 0;
            BaseHumidityDifference = 0;
            ComputeValues = false;
            UpdateSimulation();
            ComputeValues = true;
        }
        #endregion

        #region Ticking
        public void BeginTick(float deltaTime)
        {

        }

        public void Tick(float deltaTime)
        {
            UpdateSimulation();
        }


        public void EndTick(float deltaTime)
        {

        }
        #endregion

        #region Data Structure
        public Dictionary<Tuple<string, int>, object> initializeData()
        {
            this.data.Add(this.weatherYesterdayKey, this.YesterdayVals);
            this.data.Add(this.weatherTodayKey, this.TodayVals);
            this.data.Add(this.rainStrengthKey, new FloatGridData(this.TodayVals.PrecipIntensity));

            return this.data;
        }

        public void readData(List<AbstractGridData> sentData)
        {
            // Read data from list
            object temp = (object)0.0f;

            sentData[0].GetData(ref temp);
            this.timeOfDay = (float)temp;

            temp = (object)0;

            sentData[1].GetData(ref temp);
            this.dayOfYear = (int)temp;

            temp = (object)false;

            sentData[2].GetData(ref temp);
            if ((bool)temp)
                NewDay();

            temp = (object)0;

            sentData[3].GetData(ref temp);
            this.ticksPerDay = (int)temp;
        }

        public Dictionary<Tuple<string, int>, object> writeData()
        {
            // Update data dictionary
            this.data[this.weatherYesterdayKey] = this.YesterdayVals;
            this.data[this.weatherTodayKey] = this.TodayVals;
            this.data[this.rainStrengthKey] = this.TodayVals.PrecipIntensity;

            return data;
        }
        #endregion

        #region Methods
        public void UpdateSimulation()
        {
            if (!IsWeather && StormCooldown > 0)
                StormCooldown--;

            // CalcSunExposure();
            CalcActualTemp();
            CalcActualHumidity();
            if (DoWeather && ComputeValues && !IsWeather && StormCooldown == 0)
                CalcPrecipChance();
            CalcWindSpeed();
            CalcFogginess();
            CalcCloudCover();

            // Update graphs
            // dayTempChart.AddValue("temp", TodayVals.ActualTemp);
            // dayHumidityChart.AddValue("humidity", TodayVals.ActualHumidity);
            // windSpeedChart.AddValue("speed", TodayVals.WindSpeed);
            // windDirChart.AddValue("direction", TodayVals.WindDir);

            // if (DoWeather && IsWeather && TodayVals.isThundering)
            // {
            //     if (LightningSpawnChance > Random.Range(0f, 100f))
            //     {
            //         Vector3 randomVec = RandomPointInBox();
            //         GameObject bolt = GetPooledObject();
            //         if (bolt != null)
            //         {
            //             Debug.Log("Spawned new lightning");
            //             bolt.transform.position = new Vector3(bolt.transform.position.x + Random.Range(-50, 50), 150, bolt.transform.position.z + Random.Range(-50, 50));
            //             bolt.SetActive(true);
            //             bolt.GetComponent<LightningGenerator>().Generate(Random.Range(1, 3));
            //         }
            //     }
            // }
        }

        public void NewDay(bool fromEditor = false)
        {
            // Save yesterday's values and prepare for today's values
            CopyWeatherValues();

            // Add to graph
            // baseChart.AddValue("baseTemp", TodayVals.BaseTemp);
            // baseChart.AddValue("actualTemp", TodayVals.ActualTemp);
            // actualChart.AddValue("baseHumidity", TodayVals.BaseHumidity);
            // actualChart.AddValue("actualHumidity", TodayVals.ActualHumidity);
            // precipChart.AddValue("intensity", TodayVals.PrecipIntensity);
            // precipChart.AddValue("temp", TodayVals.PrecipTemp);
            // precipChart.AddValue("duration", TodayVals.PrecipLength);
            // precipChart.AddValue("chance", TodayVals.PrecipChance);

            // int precipType = 0;
            // if (TodayVals.isRaining)
            //     precipType = 1;
            // else if (TodayVals.isSnowing)
            //     precipType = 2;
            // else if (TodayVals.isThundering)
            //     precipType = 3;
            // else if (TodayVals.isRaining && TodayVals.isThundering)
            //     precipType = 4;
            // else if (TodayVals.isRaining && TodayVals.isSnowing)
            //     precipType = 5;

            // precipTypeChart.AddValue("type", precipType);
            // fogCloudsChart.AddValue("fog", TodayVals.Fogginess);
            // fogCloudsChart.AddValue("cloudCover", TodayVals.CloudCover);

            #region Base temp
            float tempBaseTemp;

            // Calculate today's base temp with yesterday's influence
            // Sample season temp from curve
            tempBaseTemp = SeasonTempCurve.Evaluate(dayOfYear);

            // Add yesterday's temp influence
            tempBaseTemp += (YesterdayVals.BaseTemp / 20);

            // Add a biased random deviation of 5-25 degrees
            tempBaseTemp -= Utils.BiasedRandom(5, 25, 0.5f);
            tempBaseTemp += Utils.BiasedRandom(5, 25, 0.5f);

            // Smoothly transition the temperature
            // if (fromEditor)
            // {
            TodayVals.BaseTemp = tempBaseTemp;
            BaseTempDifference = Mathf.Abs(TodayVals.BaseTemp - YesterdayVals.BaseTemp);
            // }
            // else
            // {
            //     LeanTween.value(this.gameObject, YesterdayVals.BaseTemp, tempBaseTemp, 5).setOnUpdate(UpdateBaseTemp).setOnComplete(() => { BaseTempDifference = Mathf.Abs(TodayVals.BaseTemp - YesterdayVals.BaseTemp); });
            // }
            #endregion

            #region Base humidity
            float tempBaseHumidity;

            // Calculate base humidity with yesterday's influence
            tempBaseHumidity = (BaseHumidityDifference + (TodayVals.PrecipIntensity / 20)) + Utils.ScaleNumber(-25, 120, 0, 1, TodayVals.BaseTemp);

            // Smoothly transition the humidity
            // if (fromEditor)
            // {
            TodayVals.BaseHumidity = tempBaseHumidity;
            BaseHumidityDifference = Mathf.Abs(TodayVals.BaseHumidity - YesterdayVals.BaseHumidity);
            // }
            // else
            // LeanTween.value(this.gameObject, YesterdayVals.BaseHumidity, tempBaseHumidity, 5).setOnUpdate(UpdateBaseHumidity).setOnComplete(() => { BaseHumidityDifference = Mathf.Abs(TodayVals.BaseHumidity - YesterdayVals.BaseHumidity); });

            #endregion

            #region Wind direction
            // Smoothly transition the wind direction to a new random value
            // LeanTween.value(this.gameObject, YesterdayVals.BaseHumidity, Random.value * 360, 5).setOnUpdate(UpdateWindDir);
            TodayVals.WindDir = Random.value * 360;
            #endregion

            // Calculate morning fog and (maybe) dew?

            CalcActualTemp();
            CalcActualHumidity();
            if (DoWeather && ComputeValues && !IsWeather && StormCooldown == 0)
                CalcPrecipChance();
            CalcWindSpeed();
            CalcFogginess();
            CalcCloudCover();
        }

        public GameObject GetPooledObject()
        {
            for (int i = 0; i < pooledLightning.Count; i++)
            {
                if (!pooledLightning[i].activeInHierarchy)
                {
                    return pooledLightning[i];
                }
            }
            return null;
        }

        public void UpdateBaseTemp(float value)
        {
            TodayVals.BaseTemp = value;
        }

        public void UpdateBaseHumidity(float value)
        {
            TodayVals.BaseHumidity = value;
        }

        public void UpdateWindDir(float value)
        {
            TodayVals.WindDir = value;
            this.precipitationManager.windYRotation = value;
        }

        public void UpdatePrecipValues(float value)
        {
            // No applyValues check because its only called if applyValues is true
            TodayVals.PrecipIntensity = value;
            zaraRainIntensity = value;
            // SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider = value;

            if (TodayVals.isRaining)
                this.precipitationManager.rain.amount = value;

            if (TodayVals.isSnowing)
                this.precipitationManager.snow.amount = value;

            if (value == 0)
            {
                this.precipitationManager.rain.amount = 0;
                this.precipitationManager.snow.amount = 0;
            }

            CalcCloudCover();
        }

        // Copy every value from today to yesterday
        public void CopyWeatherValues()
        {
            TodayVals.CopyTo(YesterdayVals);
        }

        public void CreateWeather(bool rain, bool thunder, bool snow, bool force = false)
        {
            float intensity = CalcPrecipIntensity();
            this.CreateWeather(rain, thunder, snow, this.calcPrecipTemp(), intensity, this.calcPrecipLength(intensity), force);
        }

        public void CreateWeather(bool rain, bool thunder, bool snow, float temperature, float intensity, float duration, bool force = false)
        {
            this.IsWeather = true;
            this.TodayVals.isRaining = rain;
            this.TodayVals.isThundering = thunder;
            this.TodayVals.isSnowing = snow;
            this.TodayVals.PrecipTemp = temperature;
            this.TodayVals.PrecipLength = duration;

            if ((ApplyValues && IsWeather) || force)
            {
                if (force)
                    this.StormCooldown = 0;

                tween = LeanTween.value(this.gameObject, TodayVals.PrecipIntensity, intensity, Random.Range(15, 30)).setOnComplete(() =>
                {
                    delay = LeanTween.delayedCall(TodayVals.PrecipLength, () =>
                    {
                        tween = LeanTween.value(this.gameObject, TodayVals.PrecipIntensity, 0, Random.Range(15, 30)).setOnComplete(() =>
                        {
                            IsWeather = false;
                            TodayVals.isThundering = false;
                            TodayVals.isRaining = false;
                            TodayVals.isSnowing = false;
                            TodayVals.PrecipTemp = 0;
                            TodayVals.PrecipLength = 0;
                        }).setOnUpdate(UpdatePrecipValues);
                    });
                }).setOnUpdate(UpdatePrecipValues);
            }
        }

        public void CancelWeather(bool resetCooldown = false)
        {
            if (IsWeather)
            {
                LeanTween.cancel(tween.uniqueId);
                LeanTween.cancel(delay.uniqueId);

                tween = LeanTween.value(this.gameObject, TodayVals.PrecipIntensity, 0, 5).setOnUpdate(UpdatePrecipValues).setOnComplete((() =>
                {
                    IsWeather = false;
                    TodayVals.isThundering = false;
                    TodayVals.isRaining = false;
                    TodayVals.isSnowing = false;
                    TodayVals.PrecipTemp = 0;
                    TodayVals.PrecipLength = 0;

                    this.precipitationManager.rain.amount = 0;
                    this.precipitationManager.snow.amount = 0;

                    if (resetCooldown)
                        StormCooldown = 0;
                }));
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(LightningSpawnBounds.center, LightningSpawnBounds.extents);
        }

        public Vector3 RandomPointInBox()
        {
            return new Vector3(
                Random.Range(LightningSpawnBounds.min.x, LightningSpawnBounds.max.x),
                Random.Range(LightningSpawnBounds.min.y, LightningSpawnBounds.max.y),
                Random.Range(LightningSpawnBounds.min.z, LightningSpawnBounds.max.z)
            );
        }
        #endregion

        #region Computation Functions
        public void CalcActualTemp()
        {
            if (ComputeValues)
                TodayVals.ActualTemp = (TodayVals.BaseTemp + (2 * DayTempCurve.Evaluate(this.timeOfDay / 24.0f)))
                                       + ((TodayVals.WindSpeed * -1.5f)
                                       + (TodayVals.ActualHumidity)
                                       + (TodayVals.PrecipIntensity / 10)
                                       + (TodayVals.PrecipTemp + (TodayVals.PrecipIntensity / 10))
                                       + currentSunExposure);
            if (ApplyValues)
                zaraTemperature = ((TodayVals.ActualTemp - 32) * 5) / 9; // Convert to Celsius
        }

        public void CalcActualHumidity()
        {
            if (ComputeValues)
                TodayVals.ActualHumidity = TodayVals.BaseHumidity + (TodayVals.ActualTemp / 80);
        }

        public void CalcWindSpeed()
        {
            if (ComputeValues)
                TodayVals.WindSpeed = Mathf.Clamp(Mathf.InverseLerp(0.0f, 10.0f, ((BaseTempDifference + TodayVals.PrecipIntensity) - (TodayVals.CloudCover / 4) - 1)) / 10.0f, 0.0f, 1.0f); // miles/hr
                // TodayVals.WindSpeed = Utils.ScaleNumber(0, 5, 0, 1, (BaseTempDifference + TodayVals.PrecipIntensity) - (TodayVals.CloudCover / 5));

            if (ApplyValues)
            {
                // WTF IS THIS CONVERSION
                zaraWindSpeed = TodayVals.WindSpeed / 2.237f; // meters/s

                this.precipitationManager.windStrength = TodayVals.WindSpeed;

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[33].slider = TodayVals.WindSpeed;
                // SkyController.GetCurrentWeatherProfile().profilePropertyList[34].slider = TodayVals.WindDir;
                // SkyController.GetCurrentWeatherProfile().profilePropertyList[14].slider = TodayVals.WindDir;
                // SkyController.GetCurrentWeatherProfile().profilePropertyList[15].slider = Utils.ScaleNumber(0, 15, 0, 1, TodayVals.WindSpeed);
            }
        }

        public void CalcFogginess()
        {
            if (ComputeValues)
                TodayVals.Fogginess = Mathf.Abs((BaseTempDifference * BaseHumidityDifference) / 10) + (TodayVals.PrecipIntensity / 1.25f);

            if (ApplyValues)
            {
                // SkyController.GetCurrentWeatherProfile().profilePropertyList[7].slider = Utils.ScaleNumber(0, 1, 0.6f, 0.05f, TodayVals.Fogginess * 2);
                // SkyController.GetCurrentWeatherProfile().profilePropertyList[9].slider = TodayVals.Fogginess + 0.1f;
                // SkyController.GetCurrentWeatherProfile().profilePropertyList[12].slider = TodayVals.Fogginess;
                // SkyController.GetCurrentWeatherProfile().profilePropertyList[0].slider = Mathf.Clamp(Utils.ScaleNumber(0, 0.15f, 0.5f, 0.01f, Mathf.Abs(TodayVals.CloudCover) * TodayVals.Fogginess), 0.1f, 0.6f);
            }
        }

        // private float CalcSunExposure() // SHOULD NOT BE USED
        // {
        //     Vector3 sunDir = sunTransform.forward;

        //     if (Physics.Raycast(shadowReceiver.position, shadowReceiver.position - sunDir, 30, ShadowCasterMask))
        //     {
        //         // In a shadow
        //         Debug.DrawLine(shadowReceiver.position, shadowReceiver.position - sunDir, Color.red);
        //         InSun = false;

        //         if (currentSunExposure > 0)
        //             currentSunExposure -= Time.deltaTime * 0.4f;
        //     }
        //     else
        //     {
        //         // Not in a shadow
        //         Debug.DrawLine(shadowReceiver.position, shadowReceiver.position - sunDir, Color.green);
        //         InSun = true;

        //         if (currentSunExposure < 1)
        //             currentSunExposure += Time.deltaTime * 0.4f;
        //     }

        //     return currentSunExposure;
        // }

        #region Precipitation and Clouds
        public void CalcPrecipChance()
        {
            if (TodayVals.BaseTemp <= 50)
                TodayVals.PrecipChance = TodayVals.ActualHumidity * 2;
            else if (TodayVals.BaseTemp > 50 && TodayVals.BaseTemp <= 80)
                TodayVals.PrecipChance = TodayVals.ActualHumidity;
            else if (TodayVals.BaseTemp > 80)
                TodayVals.PrecipChance = TodayVals.ActualHumidity * 0.75f;

            CalcPrecipType();
        }

        public void CalcPrecipType()
        {
            float rand = Random.value;

            // print("Rand is: " + rand);
            if (rand <= (TodayVals.PrecipChance / 100))
            {
                rand = Random.value;
                // print("Random chance calculation succeeded.");
                // print("New rand is: " + rand);
                if ((TodayVals.ActualTemp > 70 && TodayVals.ActualTemp <= 90) && rand <= (TodayVals.PrecipChance / 10))
                {
                    print("Created a thunderstorm");
                    this.CreateWeather(true, true, false);
                }
                else if (TodayVals.ActualTemp > 40)
                {
                    print("Created a rainstorm");
                    this.CreateWeather(true, false, false);
                }
                else if (TodayVals.ActualTemp > 25 && TodayVals.ActualTemp <= 35)
                {
                    print("Created a sleet storm");
                    this.CreateWeather(false, true, true);
                }
                else if (TodayVals.ActualTemp <= 25)
                {
                    print("Created a snowstorm");
                    this.CreateWeather(false, false, true);
                }

                // if (ApplyValues && IsWeather)
                // {
                //     float intensity = CalcPrecipIntensity();
                //     // CalcPrecipTemp();
                //     // CalcPrecipLength(intensity);

                //     tween = LeanTween.value(this.gameObject, TodayVals.PrecipIntensity, intensity, Random.Range(15, 30)).setOnComplete(() =>
                //     {
                //         delay = LeanTween.delayedCall(TodayVals.PrecipLength, () =>
                //         {
                //             tween = LeanTween.value(this.gameObject, TodayVals.PrecipIntensity, 0, Random.Range(15, 30)).setOnComplete(() =>
                //             {
                //                 IsWeather = false;
                //                 TodayVals.isThundering = false;
                //                 TodayVals.isRaining = false;
                //                 TodayVals.isSnowing = false;
                //                 TodayVals.PrecipTemp = 0;
                //                 TodayVals.PrecipLength = 0;
                //             }).setOnUpdate(UpdatePrecipValues);
                //         });
                //     }).setOnUpdate(UpdatePrecipValues);
                // }
            }
        }

        // Only calculated when choosing precip type
        public float CalcPrecipIntensity()
        {
            if (TodayVals.BaseTemp <= 50)
                return Mathf.Abs((TodayVals.PrecipChance / 3) + (TodayVals.ActualHumidity / 10) * 0.5f);
            else if (TodayVals.BaseTemp > 50 && TodayVals.BaseTemp <= 80)
                return Mathf.Abs((TodayVals.PrecipChance / 3) + (TodayVals.ActualHumidity / 10));
            else if (TodayVals.BaseTemp > 80)
                return Mathf.Abs((TodayVals.PrecipChance / 3) + (TodayVals.ActualHumidity / 10) * 1.75f);

            return 0;
        }

        // Only calculated when choosing precip type
        private float calcPrecipTemp()
        {
            return Utils.ScaleNumber(-25, 120, -10, 1, TodayVals.ActualTemp);
        }

        private float calcPrecipLength(float intensity)
        {
            float length = 0.0f;

            if (TodayVals.isThundering)
                length = (5 * intensity) * precipDurationMultiplier; //Mathf.Clamp(precipLengthConstant, 0.1f, 100);
            else
                length = (7 * intensity) * precipDurationMultiplier; //Mathf.Clamp(precipLengthConstant, 0.1f, 100);

            StormCooldown = (int)length + 120;
            print("Storm cooldown: " + StormCooldown);

            return length;
        }

        public void CalcCloudCover()
        {
            if (ComputeValues)
                TodayVals.CloudCover = Mathf.Abs((BaseTempDifference * BaseHumidityDifference) - Utils.ScaleNumber(-0.5f, 3, 0, 1, TodayVals.ActualHumidity)) + TodayVals.PrecipIntensity;

            if (ApplyValues)
            {
                // SkyController.GetCurrentWeatherProfile().profilePropertyList[16].slider = TodayVals.CloudCover;

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[37].slider = Mathf.Clamp01(Utils.ScaleNumber(0.6f, 1, 1, 0.2f, TodayVals.CloudCover));

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[22].timelineBasedGradient = Utils.GradientLerp(EnvironmentColorNormal, EnvironmentColorRain, TodayVals.CloudCover, true);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[23].timelineBasedGradient = Utils.GradientLerp(EnvironmentColorNormal, EnvironmentColorRain, TodayVals.CloudCover, true);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[24].timelineBasedGradient = Utils.GradientLerp(EnvironmentColorNormal, EnvironmentColorRain, TodayVals.CloudCover, true);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[17].timelineBasedGradient = Utils.GradientLerp(DynamicCloudsColor1Normal, DynamicCloudsColor1Rain, TodayVals.CloudCover, true);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[18].timelineBasedGradient = Utils.GradientLerp(DynamicCloudsColor2Normal, DynamicCloudsColor2Rain, TodayVals.CloudCover, true);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[20].timelineBasedGradient = Utils.GradientLerp(DirectionalLightColorNormal, DirectionalLightColorRain, TodayVals.CloudCover, true);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[3].timelineBasedGradient = Utils.GradientLerp(RayleighColorNormal, RayleighColorRain, TodayVals.CloudCover, true);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[1].timelineBasedCurve = Utils.CurveLerp(RayleighCurveNormal, RayleighCurveRain, TodayVals.CloudCover);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[4].timelineBasedGradient = Utils.GradientLerp(MieColorNormal, MieColorRain, TodayVals.CloudCover, true);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[2].timelineBasedCurve = Utils.CurveLerp(MieCurveNormal, MieCurveRain, TodayVals.CloudCover);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[5].timelineBasedCurve = Utils.CurveLerp(StarsIntensityNormal, StarsIntensityRain, TodayVals.CloudCover / 3);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[6].timelineBasedCurve = Utils.CurveLerp(MilkyWayIntensityNormal, MilkyWayIntensityRain, TodayVals.CloudCover / 3);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[19].timelineBasedCurve = Utils.CurveLerp(DirectionalLightIntensityNormal, DirectionalLightIntensityRain, TodayVals.CloudCover);

                // SkyController.GetCurrentWeatherProfile().profilePropertyList[21].timelineBasedCurve = Utils.CurveLerp(EnvironmentIntensityNormal, EnvironmentIntensityRain, TodayVals.CloudCover);
            }
        }
        #endregion
        #endregion
    }

    // float val1 = SkyController.GetCurrentWeatherProfile().profilePropertyList[27].slider;
    // float val2 = SkyController.GetCurrentWeatherProfile().profilePropertyList[28].slider;
    // float val3 = SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider;
    // if (rainValue >= 0 && rainValue < 0.3f)
    // {
    //     SkyController.GetCurrentWeatherProfile().profilePropertyList[27].slider = Mathf.Lerp(0, 1, Utils.ScaleNumber(0, 0.3f, 0, 1, rainValue));
    //     SkyController.GetCurrentWeatherProfile().profilePropertyList[28].slider = Mathf.Lerp(val2, 0, Utils.ScaleNumber(0, 0.3f, 1, 0, rainValue));//
    //     SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider = Mathf.Lerp(val3, 0, Utils.ScaleNumber(0, 0.3f, 1, 0, rainValue));//
    // }
    // else if (rainValue >= 0.3f && rainValue < 0.6f)
    // {
    //     SkyController.GetCurrentWeatherProfile().profilePropertyList[27].slider = Mathf.Lerp(val1, 0, Utils.ScaleNumber(0.3f, 0.6f, 0, 1, rainValue));//
    //     SkyController.GetCurrentWeatherProfile().profilePropertyList[28].slider = Mathf.Lerp(0, 1, Utils.ScaleNumber(0.3f, 0.6f, 0, 1, rainValue));
    //     SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider = Mathf.Lerp(val3, 0, Utils.ScaleNumber(0.3f, 0.6f, 0, 1, rainValue));//
    // }
    // else if (rainValue >= 0.6f)
    // {
    //     SkyController.GetCurrentWeatherProfile().profilePropertyList[27].slider = Mathf.Lerp(val1, 0, Utils.ScaleNumber(0.6f, 1, 0, 1, rainValue));//
    //     SkyController.GetCurrentWeatherProfile().profilePropertyList[28].slider = Mathf.Lerp(val2, 0, Utils.ScaleNumber(0.6f, 1, 0, 1, rainValue));//
    //     SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider = scaledVal;
    // }
}