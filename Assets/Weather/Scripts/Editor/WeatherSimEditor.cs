#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Weather
{
    [CustomEditor(typeof(WeatherSim))]
    public class WeatherSimEditor : Editor
    {
        // Target
        WeatherSim system;
        private Rect controlRect;
        private Rect headerRect;

        // Serialized properties
        private SerializedProperty referencesHeaderGroup;
        private SerializedProperty controlHeaderGroup;
        private SerializedProperty temperatureHeaderGroup;
        private SerializedProperty lightningHeaderGroup;
        private SerializedProperty weatherHeaderGroup;
        private SerializedProperty blendingHeaderGroup;
        private SerializedProperty otherHeaderGroup;
        // private SerializedProperty skyController;
        // private SerializedProperty timeController;
        // private SerializedProperty seasonTime;
        private SerializedProperty isWeather;
        private SerializedProperty computeVals;
        private SerializedProperty applyVals;
        private SerializedProperty doWeather;
        private SerializedProperty stormCooldown;
        private SerializedProperty seasonTempCurve;
        private SerializedProperty dayTempCurve;
        private SerializedProperty shadowCasterMask;
        private SerializedProperty inSun;
        private SerializedProperty lightningSpawnChance;
        private SerializedProperty lightningSpawnBounds;
        private SerializedProperty lightningPrefab;
        private SerializedProperty lightningParent;
        private SerializedProperty lightningPoolSize;

        // Weather Creation
        private SerializedProperty createRandomWeather;
        private SerializedProperty createThunder;
        private SerializedProperty createRain;
        private SerializedProperty createSnow;
        private SerializedProperty createWeatherIntensity;
        private SerializedProperty createWeatherTemp;
        private SerializedProperty createWeatherDuration;

        private SerializedProperty enviroColorNormal;
        private SerializedProperty enviroColorRain;
        private SerializedProperty lightColorNormal;
        private SerializedProperty lightColorRain;
        private SerializedProperty rayleighColorNormal;
        private SerializedProperty rayleighColorRain;
        private SerializedProperty mieColorNormal;
        private SerializedProperty mieColorRain;
        private SerializedProperty clouds1ColorNormal;
        private SerializedProperty clouds2ColorNormal;
        private SerializedProperty clouds1ColorRain;
        private SerializedProperty clouds2ColorRain;
        private SerializedProperty lightCurveNormal;
        private SerializedProperty lightCurveRain;
        private SerializedProperty enviroCurveNormal;
        private SerializedProperty enviroCurveRain;
        private SerializedProperty rayleighCurveNormal;
        private SerializedProperty rayleighCurveRain;
        private SerializedProperty mieCurveNormal;
        private SerializedProperty mieCurveRain;
        private SerializedProperty starsCurveNormal;
        private SerializedProperty starsCurveRain;
        private SerializedProperty milkyWayCurveNormal;
        private SerializedProperty milkyWayCurveRain;
        private SerializedProperty baseTempDiff;
        private SerializedProperty baseHumidityDiff;

        private void OnEnable()
        {
            // Get target
            system = (WeatherSim)target;

            // Find the serialized properties
            referencesHeaderGroup = serializedObject.FindProperty("referencesHeaderGroup");
            controlHeaderGroup = serializedObject.FindProperty("controlHeaderGroup");
            temperatureHeaderGroup = serializedObject.FindProperty("temperatureHeaderGroup");
            lightningHeaderGroup = serializedObject.FindProperty("lightningHeaderGroup");
            weatherHeaderGroup = serializedObject.FindProperty("weatherHeaderGroup");
            blendingHeaderGroup = serializedObject.FindProperty("blendingHeaderGroup");
            otherHeaderGroup = serializedObject.FindProperty("otherHeaderGroup");
            // skyController = serializedObject.FindProperty("SkyController");
            // timeController = serializedObject.FindProperty("TimeController");
            // seasonTime = serializedObject.FindProperty("SeasonTime");
            isWeather = serializedObject.FindProperty("IsWeather");
            computeVals = serializedObject.FindProperty("ComputeValues");
            applyVals = serializedObject.FindProperty("ApplyValues");
            doWeather = serializedObject.FindProperty("DoWeather");
            stormCooldown = serializedObject.FindProperty("StormCooldown");
            seasonTempCurve = serializedObject.FindProperty("SeasonTempCurve");
            dayTempCurve = serializedObject.FindProperty("DayTempCurve");
            shadowCasterMask = serializedObject.FindProperty("ShadowCasterMask");
            inSun = serializedObject.FindProperty("InSun");
            lightningSpawnChance = serializedObject.FindProperty("LightningSpawnChance");
            lightningSpawnBounds = serializedObject.FindProperty("LightningSpawnBounds");
            lightningPrefab = serializedObject.FindProperty("LightningPrefab");
            lightningParent = serializedObject.FindProperty("LightningParent");
            lightningPoolSize = serializedObject.FindProperty("LightningPoolSize");

            // Weather Creation
            createRandomWeather = serializedObject.FindProperty("CreateRandomWeather");
            createThunder = serializedObject.FindProperty("CreateThunder");
            createRain = serializedObject.FindProperty("CreateRain");
            createSnow = serializedObject.FindProperty("CreateSnow");
            createWeatherIntensity = serializedObject.FindProperty("CreateWeatherIntensity");
            createWeatherTemp = serializedObject.FindProperty("CreateWeatherTemp");
            createWeatherDuration = serializedObject.FindProperty("CreateWeatherDuration");


            enviroColorNormal = serializedObject.FindProperty("EnvironmentColorNormal");
            enviroColorRain = serializedObject.FindProperty("EnvironmentColorRain");
            lightColorNormal = serializedObject.FindProperty("DirectionalLightColorNormal");
            lightColorRain = serializedObject.FindProperty("DirectionalLightColorRain");
            rayleighColorNormal = serializedObject.FindProperty("RayleighColorNormal");
            rayleighColorRain = serializedObject.FindProperty("RayleighColorRain");
            mieColorNormal = serializedObject.FindProperty("MieColorNormal");
            mieColorRain = serializedObject.FindProperty("MieColorRain");
            clouds1ColorNormal = serializedObject.FindProperty("DynamicCloudsColor1Normal");
            clouds2ColorNormal = serializedObject.FindProperty("DynamicCloudsColor2Normal");
            clouds1ColorRain = serializedObject.FindProperty("DynamicCloudsColor1Rain");
            clouds2ColorRain = serializedObject.FindProperty("DynamicCloudsColor2Rain");
            lightCurveNormal = serializedObject.FindProperty("DirectionalLightIntensityNormal");
            lightCurveRain = serializedObject.FindProperty("DirectionalLightIntensityRain");
            enviroCurveNormal = serializedObject.FindProperty("EnvironmentIntensityNormal");
            enviroCurveRain = serializedObject.FindProperty("EnvironmentIntensityRain");
            rayleighCurveNormal = serializedObject.FindProperty("RayleighCurveNormal");
            rayleighCurveRain = serializedObject.FindProperty("RayleighCurveRain");
            mieCurveNormal = serializedObject.FindProperty("MieCurveNormal");
            mieCurveRain = serializedObject.FindProperty("MieCurveRain");
            starsCurveNormal = serializedObject.FindProperty("StarsIntensityNormal");
            starsCurveRain = serializedObject.FindProperty("StarsIntensityRain");
            milkyWayCurveNormal = serializedObject.FindProperty("MilkyWayIntensityNormal");
            milkyWayCurveRain = serializedObject.FindProperty("MilkyWayIntensityRain");
            baseTempDiff = serializedObject.FindProperty("BaseTempDifference");
            baseHumidityDiff = serializedObject.FindProperty("BaseHumidityDifference");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space(10);

            // References area
            // controlRect = EditorGUILayout.GetControlRect();
            // headerRect = new Rect(controlRect.x + 15, controlRect.y, controlRect.width - 20, EditorGUIUtility.singleLineHeight);
            // referencesHeaderGroup.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, referencesHeaderGroup.isExpanded, GUIContent.none, "RL Header");
            // EditorGUI.Foldout(headerRect, referencesHeaderGroup.isExpanded, GUIContent.none);
            // EditorGUI.LabelField(headerRect, new GUIContent(" References", ""));
            // if (referencesHeaderGroup.isExpanded)
            // {
            //     EditorGUILayout.Space(2);
            //     EditorGUILayout.PropertyField(skyController);
            //     EditorGUILayout.PropertyField(timeController);
            // }
            // EditorGUILayout.EndFoldoutHeaderGroup();
            // EditorGUILayout.Space(2);

            // Control area
            controlRect = EditorGUILayout.GetControlRect();
            headerRect = new Rect(controlRect.x + 15, controlRect.y, controlRect.width - 20, EditorGUIUtility.singleLineHeight);
            controlHeaderGroup.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, controlHeaderGroup.isExpanded, GUIContent.none, "RL Header");
            EditorGUI.Foldout(headerRect, controlHeaderGroup.isExpanded, GUIContent.none);
            EditorGUI.LabelField(headerRect, new GUIContent(" Control", ""));
            if (controlHeaderGroup.isExpanded)
            {
                EditorGUILayout.Space(2);
                // EditorGUILayout.PropertyField(seasonTime);
                EditorGUILayout.PropertyField(isWeather);
                EditorGUILayout.PropertyField(computeVals);
                EditorGUILayout.PropertyField(applyVals);
                EditorGUILayout.PropertyField(doWeather);
                EditorGUILayout.PropertyField(stormCooldown);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);

            // Temperature variables area
            controlRect = EditorGUILayout.GetControlRect();
            headerRect = new Rect(controlRect.x + 15, controlRect.y, controlRect.width - 20, EditorGUIUtility.singleLineHeight);
            temperatureHeaderGroup.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, temperatureHeaderGroup.isExpanded, GUIContent.none, "RL Header");
            EditorGUI.Foldout(headerRect, temperatureHeaderGroup.isExpanded, GUIContent.none);
            EditorGUI.LabelField(headerRect, new GUIContent(" Temperature", ""));
            if (temperatureHeaderGroup.isExpanded)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(seasonTempCurve);
                EditorGUILayout.PropertyField(dayTempCurve);
                EditorGUILayout.PropertyField(shadowCasterMask);
                EditorGUILayout.PropertyField(inSun);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);

            // Lightning variables area
            controlRect = EditorGUILayout.GetControlRect();
            headerRect = new Rect(controlRect.x + 15, controlRect.y, controlRect.width - 20, EditorGUIUtility.singleLineHeight);
            lightningHeaderGroup.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, lightningHeaderGroup.isExpanded, GUIContent.none, "RL Header");
            EditorGUI.Foldout(headerRect, lightningHeaderGroup.isExpanded, GUIContent.none);
            EditorGUI.LabelField(headerRect, new GUIContent(" Lightning", ""));
            if (lightningHeaderGroup.isExpanded)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(lightningSpawnChance);
                EditorGUILayout.PropertyField(lightningSpawnBounds);
                EditorGUILayout.PropertyField(lightningPrefab);
                EditorGUILayout.PropertyField(lightningParent);
                EditorGUILayout.PropertyField(lightningPoolSize);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);

            // Weather manipulation
            controlRect = EditorGUILayout.GetControlRect();
            headerRect = new Rect(controlRect.x + 15, controlRect.y, controlRect.width - 20, EditorGUIUtility.singleLineHeight);
            weatherHeaderGroup.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, weatherHeaderGroup.isExpanded, GUIContent.none, "RL Header");
            EditorGUI.Foldout(headerRect, weatherHeaderGroup.isExpanded, GUIContent.none);
            EditorGUI.LabelField(headerRect, new GUIContent(" Weather", ""));
            if (weatherHeaderGroup.isExpanded)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(createRandomWeather);
                EditorGUILayout.PropertyField(createRain);
                EditorGUILayout.PropertyField(createThunder);
                EditorGUILayout.PropertyField(createSnow);
                EditorGUILayout.PropertyField(createWeatherTemp);
                EditorGUILayout.PropertyField(createWeatherIntensity);
                EditorGUILayout.PropertyField(createWeatherDuration);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create Weather"))
                {
                    if (createRandomWeather.boolValue)
                        system.CreateWeather(true, true, true, true);
                    else
                        system.CreateWeather(createRain.boolValue, createThunder.boolValue, createSnow.boolValue, createWeatherTemp.floatValue, createWeatherIntensity.floatValue, createWeatherDuration.floatValue, true);
                }

                if (GUILayout.Button("Cancel Weather"))
                    system.CancelWeather(true);

                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);

            // Blending area
            controlRect = EditorGUILayout.GetControlRect();
            headerRect = new Rect(controlRect.x + 15, controlRect.y, controlRect.width - 20, EditorGUIUtility.singleLineHeight);
            blendingHeaderGroup.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, blendingHeaderGroup.isExpanded, GUIContent.none, "RL Header");
            EditorGUI.Foldout(headerRect, blendingHeaderGroup.isExpanded, GUIContent.none);
            EditorGUI.LabelField(headerRect, new GUIContent(" Blending", ""));
            if (blendingHeaderGroup.isExpanded)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(enviroColorNormal);
                EditorGUILayout.PropertyField(enviroColorRain);
                EditorGUILayout.PropertyField(lightColorNormal);
                EditorGUILayout.PropertyField(lightColorRain);
                EditorGUILayout.PropertyField(rayleighColorNormal);
                EditorGUILayout.PropertyField(rayleighColorRain);
                EditorGUILayout.PropertyField(mieColorNormal);
                EditorGUILayout.PropertyField(mieColorRain);
                EditorGUILayout.PropertyField(clouds1ColorNormal);
                EditorGUILayout.PropertyField(clouds2ColorNormal);
                EditorGUILayout.PropertyField(clouds1ColorRain);
                EditorGUILayout.PropertyField(clouds2ColorRain);
                EditorGUILayout.PropertyField(lightCurveNormal);
                EditorGUILayout.PropertyField(lightCurveRain);
                EditorGUILayout.PropertyField(enviroCurveNormal);
                EditorGUILayout.PropertyField(enviroCurveRain);
                EditorGUILayout.PropertyField(rayleighCurveNormal);
                EditorGUILayout.PropertyField(rayleighCurveRain);
                EditorGUILayout.PropertyField(mieCurveNormal);
                EditorGUILayout.PropertyField(mieCurveRain);
                EditorGUILayout.PropertyField(starsCurveNormal);
                EditorGUILayout.PropertyField(starsCurveRain);
                EditorGUILayout.PropertyField(milkyWayCurveNormal);
                EditorGUILayout.PropertyField(milkyWayCurveRain);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);

            // Other variables area
            controlRect = EditorGUILayout.GetControlRect();
            headerRect = new Rect(controlRect.x + 15, controlRect.y, controlRect.width - 20, EditorGUIUtility.singleLineHeight);
            otherHeaderGroup.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerRect, otherHeaderGroup.isExpanded, GUIContent.none, "RL Header");
            EditorGUI.Foldout(headerRect, otherHeaderGroup.isExpanded, GUIContent.none);
            EditorGUI.LabelField(headerRect, new GUIContent(" Other", ""));
            if (otherHeaderGroup.isExpanded)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(baseTempDiff);
                EditorGUILayout.PropertyField(baseHumidityDiff);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);


            // Buttons
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("New Day"))
                system.NewDay(true);
            if (GUILayout.Button("Reset Values"))
                system.ResetEditor();

            GUILayout.EndHorizontal();

            // Update the inspector when there is a change
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif