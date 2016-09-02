using CB.CircuitStates;
using CB.Events;
using CB.Exceptions;
using System;

namespace CB.Core
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
            Errors = 0;
            Success = 0;
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
                    ExecuteOpenCircuitAction();
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
            catch (Exception)
            {
                TryOpenCircuit();

                throw;
            }
        }

        private void ExecuteOpenCircuitAction()
        {
            TryHalfOpenCircuit();

            if (State == CircuitStatus.Open)
                throw new OpenCircuitException("Circuit is open");
        }

        private void ExecuteHalfOpenCircuitAction(Action action)
        {
            try
            {
                action.Invoke();

                TryCloseCircuit();
            }
            catch (Exception)
            {
                Open();

                throw;
            }
        }

        public void Open()
        {
            State = CircuitStatus.Open;
            OpenedAt = DateTime.UtcNow;

            OnStatusChanged(new NewCircuitStatus(State));
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

        private void TryOpenCircuit()
        {
            Errors++;

            if (Errors >= MaxErrors)
                Open();
        }

        private void TryHalfOpenCircuit()
        {
            if (DateTime.UtcNow.Subtract(OpenedAt) >= CircuitReset)
                HalfOpen();
        }

        private void TryCloseCircuit()
        {
            Success++;

            if (Success >= MaxSuccess)
                Close();
        }

    }

}