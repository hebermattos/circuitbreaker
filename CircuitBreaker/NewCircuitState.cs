using CB.CircuitStates;
using System;

namespace CB.Events
{
    public class NewCircuitState : EventArgs
    {
        public CircuitState State { get; set; }
        public object Date { get; private set; }

        public NewCircuitState(CircuitState newState)
        {
            State = newState;
            Date = DateTime.UtcNow;
        }
    }
}
