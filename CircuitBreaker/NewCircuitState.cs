using CB.CircuitStates;
using System;

namespace CB.Events
{
    public class NewCircuitStatus : EventArgs
    {
        public string Reason { get; set; }
        public CircuitStatus Status { get; set; }
        public object Date { get; private set; }

        public NewCircuitStatus(CircuitStatus newStatus, string reason = "")
        {
            Status = newStatus;
            Date = DateTime.UtcNow;
            Reason = reason;
        }

    
    }
}
