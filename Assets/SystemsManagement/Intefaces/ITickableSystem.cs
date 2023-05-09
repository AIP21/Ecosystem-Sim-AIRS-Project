using UnityEngine;

namespace Managers.Interfaces {
    public interface ITickableSystem {
        float TickPriority { get; }
        int TickInterval { get; } // in frames
        int ticksSinceLastTick { get; set; }
        bool willTickNow { get; set; }

        /**
        <summary>
            Called at the beginning of a tick, before the main tick begins execution.
            Use this to request any data from the data structure that will be used during the tick.
        </summary>
        **/
        void BeginTick(float deltaTime);

        /**
        <summary>
            The main tick.
        </summary>
        **/
        void Tick(float deltaTime);

        /**
        <summary>
            Called at the end of a tick, after the main tick is done executing.
        </summary>
        **/
        void EndTick(float deltaTime);
    }
}