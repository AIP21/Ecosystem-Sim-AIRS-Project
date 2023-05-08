using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Managers.Interfaces;

namespace Managers {
    public class SystemsManager : MonoBehaviour {
        public static SystemsManager Instance { get; private set; }

        public List<GameObject> tickableObjects = new List<GameObject>();
        private List<ITickableSystem> tickableSystems = new List<ITickableSystem>();

        
        private List<ITickableSystem> toTick = new List<ITickableSystem>();

        public void Awake() {
            Instance = this;
        }

        public void Start() {
            for (int i = 0; i < tickableObjects.Count; i++) {
                ITickableSystem system = tickableObjects[i].GetComponent<ITickableSystem>();
                if (system == null) {
                    Debug.LogError("SystemsManager: GameObject " + tickableObjects[i].name + " does not have a component that implements ITickableSystem");
                }
                
                tickableSystems.Add(system);
            }
        }

        public void FixedUpdate() {
            float deltaT = Time.fixedDeltaTime;
            
            toTick.Clear();
            
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