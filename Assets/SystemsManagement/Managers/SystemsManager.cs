using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Utilities;

namespace Managers
{
    public class SystemsManager : MonoBehaviour
    {
        #region Public
        public static SystemsManager Instance { get; private set; }

        public List<GameObject> TickableObjects = new List<GameObject>();

        [Header("Debug")]
        public bool CalculateDebugInfo = false;
        public int StatHistory = 50;

        [Space(5)]
        public float BeginTickTime = 0;
        public float TickTime = 0;
        public float EndTickTime = 0;
        public float TotalTickTime = 0;

        [Space(5)]
        public bool Running = true;
        #endregion

        #region Private
        private List<ITickableSystem> tickableSystems = new List<ITickableSystem>();

        private List<ITickableSystem> toTick = new List<ITickableSystem>();

        private List<float> _beginTickTimes = new List<float>();
        private List<float> _tickTimes = new List<float>();
        private List<float> _endTickTimes = new List<float>();
        private List<float> _totalTickTimes = new List<float>();
        #endregion

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            Debug.LogWarning("Remember to add all tickable systems to the TickableObjects list in SystemsManager!");

            for (int i = 0; i < TickableObjects.Count; i++)
            {
                ITickableSystem[] systems = TickableObjects[i].GetComponents<ITickableSystem>();

                if (systems == null || systems.Length == 0)
                {
                    Debug.LogError("SystemsManager: GameObject " + TickableObjects[i].name + " does not have any components that implement ITickableSystem");
                }

                tickableSystems.AddRange(systems);
            }
        }

        public void Update() // FixedUpdate
        {
            if (!Running)
                return;

            float deltaT = Time.fixedDeltaTime;

            toTick.Clear();

            Stopwatch st = null;

            float tick = 0.0f;

            if (CalculateDebugInfo)
            {
                st = new Stopwatch();
                st.Start();
            }

            // Figure out which systems need to be ticked this frame
            for (int i = 0; i < tickableSystems.Count; i++)
            {
                ITickableSystem system = tickableSystems[i];

                if (system.ticksSinceLastTick >= system.TickInterval)
                {
                    system.ticksSinceLastTick = 0;
                    if (system.shouldTick)
                    {
                        system.willTickNow = true;
                        toTick.Add(system);
                    }
                }
                else
                {
                    system.ticksSinceLastTick++;
                    system.willTickNow = false;
                }
            }

            // Sort systems by priority
            toTick.Sort((a, b) => a.TickPriority.CompareTo(b.TickPriority));

            // Tick all manager systems that need to be ticked this frame
            for (int i = 0; i < toTick.Count; i++)
            {
                toTick[i].BeginTick(deltaT * (toTick[i].TickInterval + 1));
            }

            if (CalculateDebugInfo)
            {
                st.Stop();

                tick += (float)st.Elapsed.TotalMilliseconds;
                Utils.AddToAverageList<float>(_beginTickTimes, (float)st.Elapsed.TotalMilliseconds, StatHistory);

                st.Reset();
                st.Start();
            }

            for (int i = 0; i < toTick.Count; i++)
            {
                toTick[i].Tick(deltaT * (toTick[i].TickInterval + 1));
            }

            if (CalculateDebugInfo)
            {
                st.Stop();

                tick += (float)st.Elapsed.TotalMilliseconds;
                Utils.AddToAverageList<float>(_tickTimes, (float)st.Elapsed.TotalMilliseconds, StatHistory);

                st.Reset();
                st.Start();
            }

            for (int i = 0; i < toTick.Count; i++)
            {
                toTick[i].EndTick(deltaT * (toTick[i].TickInterval + 1));
            }

            if (CalculateDebugInfo)
            {
                st.Stop();

                tick += (float)st.Elapsed.TotalMilliseconds;
                Utils.AddToAverageList<float>(_endTickTimes, (float)st.Elapsed.TotalMilliseconds, StatHistory);
                Utils.AddToAverageList<float>(_totalTickTimes, tick, StatHistory);

                BeginTickTime = Utils.Average(_beginTickTimes);
                TickTime = Utils.Average(_tickTimes);
                EndTickTime = Utils.Average(_endTickTimes);
                TotalTickTime = Utils.Average(_totalTickTimes);
            }
        }

        public void Halt()
        {
            Running = false;
        }

        public void Resume()
        {
            Running = true;
        }
    }
}