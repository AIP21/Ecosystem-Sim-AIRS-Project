using UnityEngine;

namespace Managers.Interfaces {
    public interface ITickableSystem {
        int TickPriority { get; }
        int TickInterval { get; } // in frames
        int lastTick { get; set; }
        bool willTickNow { get; set; }

        void BeginTick(float deltaTime);
        void Tick(float deltaTime);
        void EndTick(float deltaTime);
    }
}