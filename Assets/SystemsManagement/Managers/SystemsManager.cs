using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;

namespace Managers
{
    public class SystemsManager : MonoBehaviour
    {
        public static SystemsManager Instance { get; private set; }

        public List<GameObject> TickableObjects = new List<GameObject>();
        private List<ITickableSystem> tickableSystems = new List<ITickableSystem>();


        private List<ITickableSystem> toTick = new List<ITickableSystem>();

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
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

        public void FixedUpdate()
        {
            float deltaT = Time.fixedDeltaTime;

            toTick.Clear();

            // Figure out which systems need to be ticked this frame
            int currentTick = Time.frameCount;
            for (int i = 0; i < tickableSystems.Count; i++)
            {
                ITickableSystem system = tickableSystems[i];

                if (system.ticksSinceLastTick >= system.TickInterval)
                {
                    system.ticksSinceLastTick = 0;
                    system.willTickNow = true;
                    toTick.Add(system);
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
                toTick[i].BeginTick(deltaT);
            }

            for (int i = 0; i < toTick.Count; i++)
            {
                toTick[i].Tick(deltaT);
            }

            for (int i = 0; i < toTick.Count; i++)
            {
                toTick[i].EndTick(deltaT);
            }
        }
    }
}