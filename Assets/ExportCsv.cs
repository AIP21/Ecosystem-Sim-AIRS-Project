using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Weather;
using DayTime;
using Managers.Interfaces;
using Managers;
using SimDataStructure;
using UnityEditor;
using TreeGrowth;

public class ExportCsv : MonoBehaviour, ITickableSystem
{
    public StringBuilder sb = new System.Text.StringBuilder();

    public WeatherSim sim;
    public TimeManager time;
    public SystemsManager systems;
    public TreeManager trees;
    public DataStructure ds;

    public float TickPriority { get { return 6; } }
    public int TickInterval { get { return 1; } }
    public int ticksSinceLastTick { get; set; }
    public bool willTickNow { get; set; }
    public bool shouldTick { get { return this.isActiveAndEnabled; } }

    void Awake()
    {
        string localDir = Application.dataPath;
        string filePath = Path.Combine(localDir, "export.csv");

        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.Write("TimeOfDay;DayOfYear;BaseTemperature;BaseHumidity;ActualTemperature;ActualHumidity;PrecipChange;PrecipIntensity;PrecipTemperature;PrecipDuration;CloudCover;Fogginess;WindSpeed;WindDirection;Raining;Thundering;Snowing;GRPT;GWPT;GAPT;CRPT;CWPT;CAPT;GRTPT;GWTPT;CRTPT;CWTPT;TPBT;TPT;TPET;AvgAge;AvgAgeNonzero;MaxAge;\n");
        }
    }

    public void BeginTick(float deltaTime)
    {

    }

    public void Tick(float deltaTime)
    {

    }

    public void EndTick(float deltaTime)
    {
        if (sb.Length > 10000)
        {
            string localDir = Application.dataPath;
            string filePath = Path.Combine(localDir, "export.csv");
            print("Writing to file: " + filePath);

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.Write(sb.ToString());
            }

            sb.Clear();

            record();
        }
        else
        {
            record();
        }
    }

    public void OnApplicationQuit()
    {
        string localDir = Application.dataPath;
        string filePath = Path.Combine(localDir, "export.csv");

        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            writer.Write(sb.ToString());
        }
    }

    int years = 0;
    public int YearsToSimulate = 10;

    public void record()
    {
        sb.AppendLine(time.TimeOfDay.ToString() + ";" + time.DayOfYear.ToString() + ";" + sim.TodayVals.BaseTemp.ToString() + ";" + sim.TodayVals.BaseHumidity.ToString() + ";" + sim.TodayVals.ActualTemp.ToString() + ";" + sim.TodayVals.ActualHumidity.ToString() + ";" + sim.TodayVals.PrecipChance.ToString() + ";" + sim.TodayVals.PrecipIntensity.ToString() + ";" + sim.TodayVals.PrecipTemp.ToString() + ";" + sim.TodayVals.PrecipLength.ToString() + ";" + sim.TodayVals.CloudCover.ToString() + ";" + sim.TodayVals.Fogginess.ToString() + ";" + sim.TodayVals.WindSpeed.ToString() + ";" + sim.TodayVals.WindDir.ToString() + ";" + sim.TodayVals.isRaining.ToString() + ";" + sim.TodayVals.isThundering.ToString() + ";" + sim.TodayVals.isSnowing.ToString() + ";" + ds.gridReadsPerTick.ToString() + ";" + ds.gridWritesPerTick.ToString() + ";" + ds.gridActivityPerTick.ToString() + ";" + ds.cellReadsPerTick.ToString() + ";" + ds.cellWritesPerTick.ToString() + ";" + ds.cellActivityPerTick.ToString() + ";" + ds.gridReadTimePerTick.ToString() + ";" + ds.gridWriteTimePerTick.ToString() + ";" + ds.cellReadTimePerTick.ToString() + ";" + ds.cellWriteTimePerTick.ToString() + ";" + systems.BeginTickTime.ToString() + ";" + systems.TickTime.ToString() + ";" + systems.EndTickTime.ToString() + ";" + trees.averageAge.ToString() + ";" + trees.averageAgeNonzero.ToString() + ";" + trees.maxAge.ToString() + ";");

        if (time.DayOfYear >= 365)
        {
            years++;

            if (years >= YearsToSimulate)
            {
                string localDir = Application.dataPath;
                string filePath = Path.Combine(localDir, "export.csv");

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.Write(sb.ToString());
                }

                sb.Clear();

                this.enabled = false;

                EditorApplication.isPlaying = false;
            }
        }
    }
}