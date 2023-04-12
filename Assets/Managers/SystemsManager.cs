using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Managers {
    public class SystemsManager : MonoBehaviour {
        public static SystemsManager Instance { get; private set; }

        public List<ITickableSystem> tickableSystems = new List<ITickableSystem>();

        public void Awake() {
            Instance = this;
        }

        public void FixedUpdate() {
            // Tick all manager systems
            for (int i = 0; i < tickableSystems.Count; i++) {
                tickableSystems[i].BeginTick();
            }
            
            for (int i = 0; i < tickableSystems.Count; i++) {
                tickableSystems[i].Tick();
            }

            for (int i = 0; i < tickableSystems.Count; i++) {
                tickableSystems[i].EndTick();
            }
        }
    }
}