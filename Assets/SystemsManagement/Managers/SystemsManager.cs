using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;

namespace Managers {
    public class SystemsManager : MonoBehaviour {
        public static SystemsManager Instance { get; private set; }

        public List<ITickableSystem> tickableSystems = new List<ITickableSystem>();
        
        private Dictionary<ITickableSystem, int> lastTicks = new Dictionary<ITickableSystem, int>();

        public void Awake() {
            Instance = this;
        }

        public void FixedUpdate() {
            List<ITickableSystem> toTick = new List<ITickableSystem>(tickableSystems);
            
            // Figure out which systems need to be ticked this frame
            for (int i = 0; i < tickableSystems.Count; i++) {
                ITickableSystem system = tickableSystems[i];
                int lastTick = lastTicks.ContainsKey(system) ? lastTicks[system] : 0;
                int currentTick = Time.frameCount;
                int tickInterval = system.TickInterval;
                if (currentTick - lastTick >= tickInterval) {
                    lastTicks[system] = currentTick;
                    toTick.Add(system);
                }
            }
            
            // Sort systems by priority
            toTick.Sort((a, b) => a.TickPriority.CompareTo(b.TickPriority));

            // Tick all manager systems that need to be ticked this frame
            for (int i = 0; i < toTick.Count; i++) {
                toTick[i].BeginTick();
            }

            for (int i = 0; i < toTick.Count; i++) {
                toTick[i].Tick();
            }

            for (int i = 0; i < toTick.Count; i++) {
                toTick[i].EndTick();
            }
        }
    }
}