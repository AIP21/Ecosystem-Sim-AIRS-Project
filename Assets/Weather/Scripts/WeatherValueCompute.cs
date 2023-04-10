// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Configuration;
// using UnityEngine;
// using UnityEngine.AzureSky;
// using Random = UnityEngine.Random;

// //[ExecuteInEditMode]
// public class WeatherValueCompute : MonoBehaviour
// {
//     public LTDescr delay, tween;

// #if UNITY_EDITOR
//     [SerializeField] [HideInInspector] private bool referencesHeaderGroup;
//     [SerializeField] [HideInInspector] private bool controlHeaderGroup;
//     [SerializeField] [HideInInspector] private bool temperatureHeaderGroup;
//     [SerializeField] [HideInInspector] private bool lightningHeaderGroup;
//     [SerializeField] [HideInInspector] private bool blendingHeaderGroup;
//     [SerializeField] [HideInInspector] private bool otherHeaderGroup;
// #endif

//     #region Value containers
//     [Header("Value containers")]
//     public ValueContainer TodayVals;
//     public ValueContainer YesterdayVals;
//     #endregion

//     #region Script references
//     // [Header("Script references")]
//     [SerializeField] [HideInInspector] private AzureWeatherController SkyController;
//     [SerializeField] [HideInInspector] private AzureTimeController TimeController;
//     #endregion

//     #region Control variables
//     // [Header("Control variables")]
//     [Range(0, 365)]
//     [SerializeField] [HideInInspector] private int SeasonTime;
//     [SerializeField] [HideInInspector] public bool IsWeather;
//     [SerializeField] [HideInInspector] private bool ComputeValues = true;
//     [SerializeField] [HideInInspector] private bool ApplyValues = true;
//     [SerializeField] [HideInInspector] private bool DoWeather = true;
//     [SerializeField] [HideInInspector] private int StormCooldown;
//     #endregion

//     #region Temperature variables
//     // [Header("Temperature variables")]
//     [SerializeField] [HideInInspector] private AnimationCurve SeasonTempCurve;
//     [SerializeField] [HideInInspector] private AnimationCurve DayTempCurve;
//     [SerializeField] [HideInInspector] private LayerMask ShadowCasterMask;
//     [SerializeField] [HideInInspector] private bool InSun;
//     private Transform shadowReceiver;
//     private float currentSunExposure;
//     #endregion

//     #region Lightning spawning
//     // [Header("Lightning spawning")]
//     [Range(0, 100)]
//     [SerializeField] [HideInInspector] private float LightningSpawnChance = 3;
//     [SerializeField] [HideInInspector] private Bounds LightningSpawnBounds;
//     [SerializeField] [HideInInspector] private GameObject LightningPrefab;
//     [SerializeField] [HideInInspector] private Transform LightningParent;
//     [SerializeField] [HideInInspector] private int LightningPoolSize = 25;
//     private List<GameObject> pooledLightning = new List<GameObject>();
//     #endregion

//     #region Lighting blending gradients
//     // [Header("Lighting blending gradients")]
//     [SerializeField] [HideInInspector] private Gradient EnvironmentColorNormal;
//     [SerializeField] [HideInInspector] private Gradient EnvironmentColorRain;
//     [SerializeField] [HideInInspector] private Gradient DirectionalLightColorNormal;
//     [SerializeField] [HideInInspector] private Gradient DirectionalLightColorRain;
//     #endregion

//     #region Sky blending gradients
//     // [Header("Sky blending gradients")]
//     [SerializeField] [HideInInspector] private Gradient RayleighColorNormal;
//     [SerializeField] [HideInInspector] private Gradient RayleighColorRain;
//     [SerializeField] [HideInInspector] private Gradient MieColorNormal;
//     [SerializeField] [HideInInspector] private Gradient MieColorRain;
//     #endregion

//     #region Cloud blending gradients
//     // [Header("Cloud blend gradients")]
//     [SerializeField] [HideInInspector] private Gradient DynamicCloudsColor1Normal;
//     [SerializeField] [HideInInspector] private Gradient DynamicCloudsColor2Normal;
//     [SerializeField] [HideInInspector] private Gradient DynamicCloudsColor1Rain;
//     [SerializeField] [HideInInspector] private Gradient DynamicCloudsColor2Rain;
//     #endregion

//     #region Lighting blending curves
//     // [Header("Lighting blending curves")]
//     [SerializeField] [HideInInspector] private AnimationCurve DirectionalLightIntensityNormal;
//     [SerializeField] [HideInInspector] private AnimationCurve DirectionalLightIntensityRain;
//     [SerializeField] [HideInInspector] private AnimationCurve EnvironmentIntensityNormal;
//     [SerializeField] [HideInInspector] private AnimationCurve EnvironmentIntensityRain;
//     #endregion

//     #region Sky blending curves
//     // [Header("Sky blending curves")]
//     [SerializeField] [HideInInspector] private AnimationCurve RayleighCurveNormal;
//     [SerializeField] [HideInInspector] private AnimationCurve RayleighCurveRain;
//     [SerializeField] [HideInInspector] private AnimationCurve MieCurveNormal;
//     [SerializeField] [HideInInspector] private AnimationCurve MieCurveRain;
//     [SerializeField] [HideInInspector] private AnimationCurve StarsIntensityNormal;
//     [SerializeField] [HideInInspector] private AnimationCurve StarsIntensityRain;
//     [SerializeField] [HideInInspector] private AnimationCurve MilkyWayIntensityNormal;
//     [SerializeField] [HideInInspector] private AnimationCurve MilkyWayIntensityRain;
//     #endregion

//     #region Extra computation variables
//     // [Header("Extra computation variables")]
//     [SerializeField] [HideInInspector] private float BaseTempDifference;
//     [SerializeField] [HideInInspector] private float BaseHumidityDifference;
//     #endregion

//     #region Zara integration
//     [HideInInspector]
//     public float zaraRainIntensity;
//     [HideInInspector]
//     public float zaraTemperature;
//     [HideInInspector]
//     public float zaraWindSpeed;
//     #endregion

//     #region Methods
//     private void Start()
//     {
//         shadowReceiver = Utils.PrimaryCamera.transform;
//         TodayVals.PrecipIntensity = 0;
//         SkyController.GetCurrentWeatherProfile().profilePropertyList[25].slider = 0;
//         SkyController.GetCurrentWeatherProfile().profilePropertyList[26].slider = 0;
//         TodayVals.isThundering = false;
//         TodayVals.isRaining = false;
//         TodayVals.isSnowing = false;
//         tween = LeanTween.delayedCall(0, () => { });
//         delay = LeanTween.delayedCall(0, () => { });

//         pooledLightning = new List<GameObject>();
//         for (int i = 0; i < LightningPoolSize; i++)
//         {
//             GameObject obj = (GameObject)Instantiate(LightningPrefab);
//             obj.transform.parent = LightningParent;
//             obj.SetActive(false);
//             pooledLightning.Add(obj);
//         }

//         NewDay();
//     }

//     public void ResetEditor()
//     {
//         TodayVals.ResetEditor();
//         // i = 0;
//         SkyController.SetDefaultWeatherProfile(1);
//         TodayVals.isThundering = false;
//         TodayVals.isRaining = false;
//         TodayVals.isSnowing = false;
//         BaseTempDifference = 0;
//         BaseHumidityDifference = 0;
//         ComputeValues = false;
//         UpdateSimulation();
//         ComputeValues = true;
//     }

//     int e = 0;
//     public void UpdateSimulation()
//     {
//         if (e >= 20)
//         {
//             if (!IsWeather && StormCooldown > 0)
//                 StormCooldown--;

//             CalcSunExposure();
//             CalcActualTemp();
//             CalcActualHumidity();
//             if (DoWeather && ComputeValues && !IsWeather && StormCooldown == 0)
//                 CalcPrecipChance();
//             CalcWindSpeed();
//             CalcFogginess();
//             CalcCloudCover();

//             if (DoWeather && IsWeather && TodayVals.isThundering)
//             {
//                 // Maybe increase spawn chances based on difficulty
//                 if (LightningSpawnChance > Random.Range(0f, 100f))
//                 {
//                     Vector3 randomVec = RandomPointInBox();
//                     GameObject bolt = GetPooledObject();
//                     if (bolt != null)
//                     {
//                         Debug.Log("Spawned new lightning");
//                         bolt.transform.position = new Vector3(bolt.transform.position.x + Random.Range(-50, 50), 150, bolt.transform.position.z + Random.Range(-50, 50));
//                         bolt.SetActive(true);
//                         bolt.GetComponent<LightningGenerator>().Generate(Random.Range(1, 3));
//                     }
//                 }
//             }
//             e = 0;
//         }
//         e++;
//     }

//     public GameObject GetPooledObject()
//     {
//         for (int i = 0; i < pooledLightning.Count; i++)
//         {
//             if (!pooledLightning[i].activeInHierarchy)
//             {
//                 return pooledLightning[i];
//             }
//         }
//         return null;
//     }

//     public void NewDay(bool fromEditor = false)
//     {
//         if (SeasonTime < 365)
//             SeasonTime++;
//         else if (SeasonTime >= 365)
//             SeasonTime = 0;

//         // Save yesterday's values and prepare for today's values
//         SaveTodaysValues();

//         #region Base temp
//         float tempBaseTemp;

//         // Calculate today's base temp with yesterday's influence
//         // Sample season temp from curve
//         tempBaseTemp = SeasonTempCurve.Evaluate(SeasonTime);

//         // Add yesterday's temp influence
//         tempBaseTemp += (YesterdayVals.BaseTemp / 20);


//         // Add a biased random deviation of 5-25 degrees
//         // Maybe change the bias based on chosen difficulty?
//         tempBaseTemp -= Utils.BiasedRandom(5, 25, 0.5f);
//         tempBaseTemp += Utils.BiasedRandom(5, 25, 0.5f);

//         // Smoothly transition the temperature
//         if (fromEditor)
//         {
//             TodayVals.BaseTemp = tempBaseTemp;
//             BaseTempDifference = Mathf.Abs(TodayVals.BaseTemp - YesterdayVals.BaseTemp);
//         }
//         else
//             LeanTween.value(this.gameObject, YesterdayVals.BaseTemp, tempBaseTemp, 5).setOnUpdate(UpdateBaseTemp).setOnComplete(() => { BaseTempDifference = Mathf.Abs(TodayVals.BaseTemp - YesterdayVals.BaseTemp); });
//         #endregion

//         #region Base humidity
//         float tempBaseHumidity;

//         // Calculate base humidity with yesterday's influence
//         tempBaseHumidity = (BaseHumidityDifference + (TodayVals.PrecipIntensity / 20)) + Utils.ScaleNumber(-25, 120, 0, 1, TodayVals.BaseTemp);

//         // Smoothly transition the humidity
//         if (fromEditor)
//         {
//             TodayVals.BaseHumidity = tempBaseHumidity;
//             BaseHumidityDifference = Mathf.Abs(TodayVals.BaseHumidity - YesterdayVals.BaseHumidity);
//         }
//         else
//             LeanTween.value(this.gameObject, YesterdayVals.BaseHumidity, tempBaseHumidity, 5).setOnUpdate(UpdateBaseHumidity).setOnComplete(() => { BaseHumidityDifference = Mathf.Abs(TodayVals.BaseHumidity - YesterdayVals.BaseHumidity); });
//         #endregion

//         #region Wind direction
//         // Smoothly transition the wind direction to a new random value
//         LeanTween.value(this.gameObject, YesterdayVals.BaseHumidity, Random.value * 360, 5).setOnUpdate(UpdateWindDir);
//         #endregion

//         // Calculate morning fog and (maybe) dew?

//         CalcActualTemp();
//         CalcActualHumidity();
//         if (DoWeather && ComputeValues && !IsWeather && StormCooldown == 0)
//             CalcPrecipChance();
//         CalcWindSpeed();
//         CalcFogginess();
//         CalcCloudCover();
//     }

//     public void CancelWeather()
//     {
//         if (IsWeather)
//         {
//             LeanTween.cancel(tween.uniqueId);
//             LeanTween.cancel(delay.uniqueId);

//             tween = LeanTween.value(this.gameObject, TodayVals.PrecipIntensity, 0, 5).setOnUpdate(UpdatePrecipValues).setOnComplete((() =>
//             {
//                 IsWeather = false;
//                 TodayVals.isThundering = false;
//                 TodayVals.isRaining = false;
//                 TodayVals.isSnowing = false;
//                 TodayVals.PrecipTemp = 0;
//                 TodayVals.PrecipLength = 0;
//             }));
//         }
//     }

//     public void UpdateBaseTemp(float value)
//     {
//         TodayVals.BaseTemp = value;
//     }

//     public void UpdateBaseHumidity(float value)
//     {
//         TodayVals.BaseHumidity = value;
//     }

//     public void UpdateWindDir(float value)
//     {
//         TodayVals.WindDir = value;
//     }

//     public void UpdatePrecipValues(float value)
//     {
//         // No applyValues check because its only called if applyValues is true
//         TodayVals.PrecipIntensity = value;
//         zaraRainIntensity = value;
//         SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider = value;

//         if (TodayVals.isRaining)
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[25].slider = value;
//         if (TodayVals.isSnowing)
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[26].slider = value;

//         CalcCloudCover();
//     }

//     public void SaveTodaysValues()
//     {
//         YesterdayVals.BaseTemp = TodayVals.BaseTemp;
//         YesterdayVals.BaseHumidity = TodayVals.BaseHumidity;
//         YesterdayVals.WindSpeed = TodayVals.WindSpeed;
//         YesterdayVals.PrecipChance = TodayVals.PrecipChance;
//         YesterdayVals.PrecipIntensity = TodayVals.PrecipIntensity;
//         YesterdayVals.PrecipLength = TodayVals.PrecipLength;
//         YesterdayVals.PrecipTemp = TodayVals.PrecipTemp;
//         YesterdayVals.Fogginess = TodayVals.Fogginess;
//         YesterdayVals.CloudCover = TodayVals.CloudCover;
//         YesterdayVals.isThundering = TodayVals.isThundering;
//         YesterdayVals.isRaining = TodayVals.isRaining;
//         YesterdayVals.isSnowing = TodayVals.isSnowing;
//         YesterdayVals.WindDir = TodayVals.WindDir;
//     }

//     public void OnDrawGizmosSelected()
//     {
//         Gizmos.DrawWireCube(LightningSpawnBounds.center, LightningSpawnBounds.extents);
//     }

//     public Vector3 RandomPointInBox()
//     {
//         return new Vector3(
//             Random.Range(LightningSpawnBounds.min.x, LightningSpawnBounds.max.x),
//             Random.Range(LightningSpawnBounds.min.y, LightningSpawnBounds.max.y),
//             Random.Range(LightningSpawnBounds.min.z, LightningSpawnBounds.max.z)
//         );
//     }
//     #endregion

//     #region Computation Functions
//     public void CalcActualTemp()
//     {
//         if (ComputeValues)
//             TodayVals.ActualTemp = (TodayVals.BaseTemp + (2 * DayTempCurve.Evaluate(TimeController.GetTimeline() / 24)))
//                                    + ((TodayVals.WindSpeed * -1.5f)
//                                    + (TodayVals.ActualHumidity)
//                                    + (TodayVals.PrecipIntensity / 10)
//                                    + (TodayVals.PrecipTemp + (TodayVals.PrecipIntensity / 10))
//                                    + currentSunExposure);
//         if (ApplyValues)
//             zaraTemperature = ((TodayVals.ActualTemp - 32) * 5) / 9; // Convert to celcius bc Zara is annoying
//     }

//     public void CalcActualHumidity()
//     {
//         if (ComputeValues)
//             TodayVals.ActualHumidity = TodayVals.BaseHumidity + (TodayVals.ActualTemp / 80);
//     }

//     public void CalcWindSpeed()
//     {
//         if (ComputeValues)
//             TodayVals.WindSpeed = (BaseTempDifference + TodayVals.PrecipIntensity) - (TodayVals.CloudCover / 4) - 1; // miles/hr
//         // TodayVals.WindSpeed = Utils.ScaleNumber(0, 5, 0, 1, (BaseTempDifference + TodayVals.PrecipIntensity) - (TodayVals.CloudCover / 5));

//         if (ApplyValues)
//         {
//             zaraWindSpeed = TodayVals.WindSpeed / 2.237f; // meters/s
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[33].slider = TodayVals.WindSpeed;
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[34].slider = TodayVals.WindDir;
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[14].slider = TodayVals.WindDir;
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[15].slider = Utils.ScaleNumber(0, 15, 0, 1, TodayVals.WindSpeed);
//         }
//     }

//     public void CalcFogginess()
//     {
//         if (ComputeValues)
//             TodayVals.Fogginess = Mathf.Abs((BaseTempDifference * BaseHumidityDifference) / 10) + (TodayVals.PrecipIntensity / 1.25f);

//         if (ApplyValues)
//         {
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[7].slider = Utils.ScaleNumber(0, 1, 0.6f, 0.05f, TodayVals.Fogginess * 2);
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[9].slider = TodayVals.Fogginess + 0.1f;
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[12].slider = TodayVals.Fogginess;
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[0].slider = Mathf.Clamp(Utils.ScaleNumber(0, 0.15f, 0.5f, 0.01f, Mathf.Abs(TodayVals.CloudCover) * TodayVals.Fogginess), 0.1f, 0.6f);
//         }
//     }

//     private float CalcSunExposure()
//     {
//         Vector3 sunDir = TimeController.m_sunLocalDirection.normalized;

//         if (Physics.Raycast(shadowReceiver.position, shadowReceiver.position - sunDir, 30, ShadowCasterMask))
//         {
//             // In a shadow
//             Debug.DrawLine(shadowReceiver.position, shadowReceiver.position - sunDir, Color.red);
//             InSun = false;

//             if (currentSunExposure > 0)
//                 currentSunExposure -= Time.deltaTime * 0.4f;
//         }
//         else
//         {
//             // Not in a shadow
//             Debug.DrawLine(shadowReceiver.position, shadowReceiver.position - sunDir, Color.green);
//             InSun = true;

//             if (currentSunExposure < 1)
//                 currentSunExposure += Time.deltaTime * 0.4f;
//         }

//         return currentSunExposure;
//     }

//     #region Precipitation and Clouds

//     public void CalcPrecipChance()
//     {
//         if (TodayVals.BaseTemp <= 50)
//             TodayVals.PrecipChance = TodayVals.ActualHumidity * 2;
//         else if (TodayVals.BaseTemp > 50 && TodayVals.BaseTemp <= 80)
//             TodayVals.PrecipChance = TodayVals.ActualHumidity;
//         else if (TodayVals.BaseTemp > 80)
//             TodayVals.PrecipChance = TodayVals.ActualHumidity * 0.75f;

//         CalcPrecipType();
//     }

//     public void CalcPrecipType()
//     {
//         float rand = Random.value;
//         // print("Rand is: " + rand);
//         if (rand <= (TodayVals.PrecipChance / 100))
//         {
//             rand = Random.value;
//             // print("Random chance calculation suceeded.");
//             // print("New rand is: " + rand);
//             if ((TodayVals.ActualTemp > 70 && TodayVals.ActualTemp <= 90) && rand <= (TodayVals.PrecipChance / 10))
//             {
//                 print("Created a thunderstorm");
//                 IsWeather = true;
//                 TodayVals.isThundering = true;
//                 TodayVals.isRaining = true;
//                 TodayVals.isSnowing = false;
//             }
//             else if (TodayVals.ActualTemp > 40)
//             {
//                 print("Created a rainstorm");
//                 IsWeather = true;
//                 TodayVals.isThundering = false;
//                 TodayVals.isRaining = true;
//                 TodayVals.isSnowing = false;
//             }
//             else if (TodayVals.ActualTemp > 25 && TodayVals.ActualTemp <= 35)
//             {
//                 print("Created a sleetstorm");
//                 IsWeather = true;
//                 TodayVals.isThundering = false;
//                 TodayVals.isRaining = true;
//                 TodayVals.isSnowing = true;
//             }
//             else if (TodayVals.ActualTemp <= 25)
//             {
//                 print("Created a snowstorm");
//                 IsWeather = true;
//                 TodayVals.isThundering = false;
//                 TodayVals.isRaining = false;
//                 TodayVals.isSnowing = true;
//             }

//             if (ApplyValues && IsWeather)
//             {
//                 float intensity = CalcPrecipIntensity();
//                 CalcPrecipTemp();
//                 CalcPrecipLength(intensity);
//                 tween = LeanTween.value(this.gameObject, TodayVals.PrecipIntensity, intensity, Random.Range(15, 30)).setOnComplete(() =>
//                 {
//                     delay = LeanTween.delayedCall(TodayVals.PrecipLength, () =>
//                     {
//                         tween = LeanTween.value(this.gameObject, TodayVals.PrecipIntensity, 0, Random.Range(15, 30)).setOnComplete(() =>
//                         {
//                             IsWeather = false;
//                             TodayVals.isThundering = false;
//                             TodayVals.isRaining = false;
//                             TodayVals.isSnowing = false;
//                             TodayVals.PrecipTemp = 0;
//                             TodayVals.PrecipLength = 0;
//                         }).setOnUpdate(UpdatePrecipValues);
//                     });
//                 }).setOnUpdate(UpdatePrecipValues);
//             }
//         }
//     }

//     // Only calculated when choosing precip type
//     public float CalcPrecipIntensity()
//     {
//         if (TodayVals.BaseTemp <= 50)
//             return Mathf.Abs((TodayVals.PrecipChance / 3) + (TodayVals.ActualHumidity / 10) * 0.5f);
//         else if (TodayVals.BaseTemp > 50 && TodayVals.BaseTemp <= 80)
//             return Mathf.Abs((TodayVals.PrecipChance / 3) + (TodayVals.ActualHumidity / 10));
//         else if (TodayVals.BaseTemp > 80)
//             return Mathf.Abs((TodayVals.PrecipChance / 3) + (TodayVals.ActualHumidity / 10) * 1.75f);

//         return 0;
//     }

//     // Only calculated when choosing precip type
//     public void CalcPrecipTemp()
//     {
//         TodayVals.PrecipTemp = Utils.ScaleNumber(-25, 120, -10, 1, TodayVals.ActualTemp);
//     }

//     public void CalcPrecipLength(float intensity)
//     {
//         if (TodayVals.isThundering)
//             TodayVals.PrecipLength = (5 * intensity) * Mathf.Clamp(TimeController.GetDayLength(), 0.1f, 100);
//         else
//             TodayVals.PrecipLength = (7 * intensity) * Mathf.Clamp(TimeController.GetDayLength(), 0.1f, 100);

//         StormCooldown = (int)TodayVals.PrecipLength + 120;
//         print("Storm cooldown: " + StormCooldown);
//     }

//     public void CalcCloudCover()
//     {
//         if (ComputeValues)
//             TodayVals.CloudCover = Mathf.Abs((BaseTempDifference * BaseHumidityDifference) - Utils.ScaleNumber(-0.5f, 3, 0, 1, TodayVals.ActualHumidity)) + TodayVals.PrecipIntensity;

//         if (ApplyValues)
//         {
//             SkyController.GetCurrentWeatherProfile().profilePropertyList[16].slider = TodayVals.CloudCover;

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[37].slider = Mathf.Clamp01(Utils.ScaleNumber(0.6f, 1, 1, 0.2f, TodayVals.CloudCover));

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[22].timelineBasedGradient = Utils.GradientLerp(EnvironmentColorNormal, EnvironmentColorRain, TodayVals.CloudCover, true);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[23].timelineBasedGradient = Utils.GradientLerp(EnvironmentColorNormal, EnvironmentColorRain, TodayVals.CloudCover, true);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[24].timelineBasedGradient = Utils.GradientLerp(EnvironmentColorNormal, EnvironmentColorRain, TodayVals.CloudCover, true);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[17].timelineBasedGradient = Utils.GradientLerp(DynamicCloudsColor1Normal, DynamicCloudsColor1Rain, TodayVals.CloudCover, true);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[18].timelineBasedGradient = Utils.GradientLerp(DynamicCloudsColor2Normal, DynamicCloudsColor2Rain, TodayVals.CloudCover, true);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[20].timelineBasedGradient = Utils.GradientLerp(DirectionalLightColorNormal, DirectionalLightColorRain, TodayVals.CloudCover, true);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[3].timelineBasedGradient = Utils.GradientLerp(RayleighColorNormal, RayleighColorRain, TodayVals.CloudCover, true);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[1].timelineBasedCurve = Utils.CurveLerp(RayleighCurveNormal, RayleighCurveRain, TodayVals.CloudCover);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[4].timelineBasedGradient = Utils.GradientLerp(MieColorNormal, MieColorRain, TodayVals.CloudCover, true);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[2].timelineBasedCurve = Utils.CurveLerp(MieCurveNormal, MieCurveRain, TodayVals.CloudCover);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[5].timelineBasedCurve = Utils.CurveLerp(StarsIntensityNormal, StarsIntensityRain, TodayVals.CloudCover / 3);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[6].timelineBasedCurve = Utils.CurveLerp(MilkyWayIntensityNormal, MilkyWayIntensityRain, TodayVals.CloudCover / 3);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[19].timelineBasedCurve = Utils.CurveLerp(DirectionalLightIntensityNormal, DirectionalLightIntensityRain, TodayVals.CloudCover);

//             SkyController.GetCurrentWeatherProfile().profilePropertyList[21].timelineBasedCurve = Utils.CurveLerp(EnvironmentIntensityNormal, EnvironmentIntensityRain, TodayVals.CloudCover);
//         }
//     }
//     #endregion
//     #endregion
// }

// // float val1 = SkyController.GetCurrentWeatherProfile().profilePropertyList[27].slider;
// // float val2 = SkyController.GetCurrentWeatherProfile().profilePropertyList[28].slider;
// // float val3 = SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider;
// // if (rainValue >= 0 && rainValue < 0.3f)
// // {
// //     SkyController.GetCurrentWeatherProfile().profilePropertyList[27].slider = Mathf.Lerp(0, 1, Utils.ScaleNumber(0, 0.3f, 0, 1, rainValue));
// //     SkyController.GetCurrentWeatherProfile().profilePropertyList[28].slider = Mathf.Lerp(val2, 0, Utils.ScaleNumber(0, 0.3f, 1, 0, rainValue));//
// //     SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider = Mathf.Lerp(val3, 0, Utils.ScaleNumber(0, 0.3f, 1, 0, rainValue));//
// // }
// // else if (rainValue >= 0.3f && rainValue < 0.6f)
// // {
// //     SkyController.GetCurrentWeatherProfile().profilePropertyList[27].slider = Mathf.Lerp(val1, 0, Utils.ScaleNumber(0.3f, 0.6f, 0, 1, rainValue));//
// //     SkyController.GetCurrentWeatherProfile().profilePropertyList[28].slider = Mathf.Lerp(0, 1, Utils.ScaleNumber(0.3f, 0.6f, 0, 1, rainValue));
// //     SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider = Mathf.Lerp(val3, 0, Utils.ScaleNumber(0.3f, 0.6f, 0, 1, rainValue));//
// // }
// // else if (rainValue >= 0.6f)
// // {
// //     SkyController.GetCurrentWeatherProfile().profilePropertyList[27].slider = Mathf.Lerp(val1, 0, Utils.ScaleNumber(0.6f, 1, 0, 1, rainValue));//
// //     SkyController.GetCurrentWeatherProfile().profilePropertyList[28].slider = Mathf.Lerp(val2, 0, Utils.ScaleNumber(0.6f, 1, 0, 1, rainValue));//
// //     SkyController.GetCurrentWeatherProfile().profilePropertyList[29].slider = scaledVal;
// // }