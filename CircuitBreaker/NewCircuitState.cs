using CB.CircuitStates;
using System;

namespace CB.Events
{
    public class NewCircuitStatus : EventArgs
    {
        public CircuitStatus Status { get; set; }
        public object Date { get; private set; }

        public NewCircuitStatus(CircuitStatus newStatus)
        {
            Status = newStatus;
            Date = DateTime.UtcNow;
        }
    }
}
