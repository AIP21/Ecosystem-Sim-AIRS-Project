using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;

namespace Managers {
    public class SystemsManager : MonoBehaviour {
        public static SystemsManager Instance { get; private set; }

        public List<ITickableSystem> tickableSystems = new List<ITickableSystem>();
        
        public void Awake() {
            Instance = this;
        }

        public void FixedUpdate() {
            float deltaT = Time.fixedDeltaTime;
            
            List<ITickableSystem> toTick = new List<ITickableSystem>(tickableSystems);
            
            // Figure out which systems need to be ticked this frame
            int currentTick = Time.frameCount;
            for (int i = 0; i < tickableSystems.Count; i++) {
                ITickableSystem system = tickableSystems[i];
                int lastTick = system.lastTick;
                int tickInterval = system.TickInterval;
                if (currentTick - lastTick >= tickInterval) {
                    system.lastTick = currentTick;
                    system.willTickNow = true;
                    toTick.Add(system);
                } else {
                    system.willTickNow = false;
                }
            }
            
            // Sort systems by priority
            toTick.Sort((a, b) => a.TickPriority.CompareTo(b.TickPriority));

            // Tick all manager systems that need to be ticked this frame
            for (int i = 0; i < toTick.Count; i++) {
                toTick[i].BeginTick(deltaT);
            }

            for (int i = 0; i < toTick.Count; i++) {
                toTick[i].Tick(deltaT);
            }

            for (int i = 0; i < toTick.Count; i++) {
                toTick[i].EndTick(deltaT);
            }
        }
    }
}