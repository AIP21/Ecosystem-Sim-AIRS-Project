using UnityEngine;
using System.Collections.Generic;
using Managers.Interfaces;
using SimDataStructure.Interfaces;
using SimDataStructure.Data;
using System;

namespace DayTime
{
    public class TimeManager : MonoBehaviour, ITickableSystem, ISetupGridData, IWriteGridData
    {
        #region Public
        [Header("Current State")]
        [Range(0.0f, 24.0f)]
        public float TimeOfDay = 12.0f;
        [Range(0, 365)]
        public int DayOfYear = 50;
        public bool IsNewDay = false;

        [Header("Config")]
        // [Tooltip("The number of hours to step per tick")]
        // public float TickTimeStep = 1.0f;
        [Tooltip("In minutes")]
        public float DayLength = 2.0f;

        public int ticksPerDay = 0;
        #endregion

        #region Private
        [SerializeField]
        private float deltaDay;

        private Dictionary<Tuple<string, int>, object> data = new Dictionary<Tuple<string, int>, object>();
        private Tuple<string, int> timeOfDayKey = new Tuple<string, int>("timeOfDay", 2);
        private Tuple<string, int> dayOfYearKey = new Tuple<string, int>("dayOfYear", 2);
        private Tuple<string, int> newDayKey = new Tuple<string, int>("newDay", 2);
        private Tuple<string, int> ticksPerDayKey = new Tuple<string, int>("ticksPerDay", 2);

        #region Interface Stuff
        private Dictionary<string, int> _writeDataNames = new Dictionary<string, int>(){
            { "timeOfDay", 2 },
            { "dayOfYear", 2 },
            { "newDay", 2 },
            { "ticksPerDay", 2 }
        };  // The names of the grid data this is writing to the data structure, along with its grid level
        public Dictionary<string, int> WriteDataNames { get { return _writeDataNames; } }


        public float TickPriority { get { return 0.1f; } }

        public int TickInterval { get { return 0; } } // 3

        public int ticksSinceLastTick { get; set; }
        public bool willTickNow { get; set; }

        public bool shouldTick { get { return this.isActiveAndEnabled; } }
        #endregion
        #endregion

        #region Ticking
        public void BeginTick(float deltaTime)
        {
            // daylength is in minutes
            // deltatime is in seconds
            this.deltaDay = (deltaTime / 60.0f) * (24.0f / this.DayLength);

            float newVal = this.TimeOfDay + this.deltaDay;

            // Wrap back to 0 if it's 24
            if (newVal >= 24)
            {
                this.TimeOfDay = newVal - 24.0f;

                // Increment day of year
                if (this.DayOfYear < 365)
                    this.DayOfYear++;
                else if (this.DayOfYear >= 365)
                    this.DayOfYear = 0;

                this.IsNewDay = true;
            }
            else
            {
                this.TimeOfDay = newVal;
                this.IsNewDay = false;
            }

            ticksPerDay = (int)(24.0f / this.deltaDay);
        }

        public void Tick(float deltaTime)
        {

        }

        public void EndTick(float deltaTime)
        {

        }
        #endregion

        #region Data Structure
        public Dictionary<Tuple<string, int>, object> initializeData()
        {
            data.Add(this.timeOfDayKey, new FloatGridData(this.TimeOfDay));
            data.Add(this.dayOfYearKey, new IntGridData(this.DayOfYear));
            data.Add(this.newDayKey, new BoolGridData(this.IsNewDay));
            data.Add(this.ticksPerDayKey, new IntGridData(this.ticksPerDay));

            return data;
        }

        public Dictionary<Tuple<string, int>, object> writeData()
        {
            data[this.timeOfDayKey] = this.TimeOfDay;
            data[this.dayOfYearKey] = this.DayOfYear;
            data[this.newDayKey] = this.IsNewDay;
            data[this.ticksPerDayKey] = this.ticksPerDay;

            return data;
        }
        #endregion

        public void NewDay()
        {
            this.DayOfYear++;
            this.IsNewDay = true;
        }

        public void ToggleTime()
        {
            if (this.isActiveAndEnabled)
                this.enabled = false;
            else
                this.enabled = true;
        }

        public void SetTimeOfDay(float time)
        {
            this.TimeOfDay = time;
        }

        public void SetDayOfYear(int day)
        {
            this.DayOfYear = day;
        }

        public void SetDayLength(string length)
        {
            float.TryParse(length, out this.DayLength);
        }
    }
}