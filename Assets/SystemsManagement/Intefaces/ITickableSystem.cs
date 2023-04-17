using UnityEngine;

namespace Managers.Interfaces {
    public interface ITickableSystem {
        int TickPriority { get; }
        int TickInterval { get; } // in frames

        void BeginTick();
        void Tick();
        void EndTick();
    }
}