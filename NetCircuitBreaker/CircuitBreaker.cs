using NetCircuitBreaker.CircuitStates;
using NetCircuitBreaker.Events;
using NetCircuitBreaker.Exceptions;
using System;

namespace NetCircuitBreaker.Core
{
    public delegate void ChangedStatusHandler(NewCircuitStatus e);

    public class CircuitBreaker
    {
        private CircuitStatus State;
        private int MaxErrors;
        private int MaxSuccess;
        private int Errors;
        private int Success;
        private TimeSpan CircuitReset;
        private DateTime OpenedAt;

        public event ChangedStatusHandler StatusChanged;

        public CircuitBreaker(int maxErrors, int maxSuccess, TimeSpan circuitReset)
        {
            State = CircuitStatus.Closed;
            MaxErrors = maxErrors;
            MaxSuccess = maxSuccess;
            CircuitReset = circuitReset;
        }

        public void Execute(Action action)
        {
            switch (State)
            {
                case CircuitStatus.Closed:
                    ExecuteClosedCircuitAction(action);
                    break;
                case CircuitStatus.HalfOpen:
                    ExecuteHalfOpenCircuitAction(action);
                    break;
                case CircuitStatus.Open:
                    ExecuteOpenCircuitAction(action);
                    break;
                default:
                    break;
            }
        }

        private void ExecuteClosedCircuitAction(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                TryOpenCircuit(ex.Message);

                throw;
            }
        }

        private void ExecuteOpenCircuitAction(Action action)
        {
            TryHalfOpenCircuit();

            ExecuteHalfOpenCircuitAction(action);
        }

        private void ExecuteHalfOpenCircuitAction(Action action)
        {
            try
            {
                action.Invoke();

                TryCloseCircuit();
            }
            catch (Exception ex)
            {
                Open(ex.Message);

                throw;
            }
        }

        public void Open(string reason)
        {
            State = CircuitStatus.Open;
            OpenedAt = DateTime.UtcNow;

            OnStatusChanged(new NewCircuitStatus(State, reason));
        }

        public void HalfOpen()
        {
            State = CircuitStatus.HalfOpen;
            Success = 0;

            OnStatusChanged(new NewCircuitStatus(State));
        }

        public void Close()
        {
            Errors = 0;
            State = CircuitStatus.Closed;

            OnStatusChanged(new NewCircuitStatus(State));
        }

        public CircuitStatus GetState()
        {
            return State;
        }

        protected virtual void OnStatusChanged(NewCircuitStatus e)
        {
            StatusChanged?.Invoke(e);
        }

        private void TryOpenCircuit(string reason)
        {
            Errors++;

            if (Errors >= MaxErrors)
                Open(reason);
        }

        private void TryHalfOpenCircuit()
        {
            if (DateTime.UtcNow.Subtract(OpenedAt) >= CircuitReset)
                HalfOpen();

            if (State == CircuitStatus.Open)
                throw new OpenCircuitException("Circuit is open");
        }

        private void TryCloseCircuit()
        {
            Success++;

            if (Success >= MaxSuccess)
                Close();
        }

    }

}