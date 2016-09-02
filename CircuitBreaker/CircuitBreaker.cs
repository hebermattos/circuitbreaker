using CB.CircuitStates;
using CB.Events;
using CB.Exceptions;
using System;

namespace CB.Core
{
    public delegate void ChangedStatusHandler(NewCircuitState e);

    public class CircuitBreaker
    {
        private CircuitState State;
        private int MaxErrors;
        private int MaxSuccess;
        private int Errors;
        private int Success;
        private TimeSpan CircuitReset;
        private DateTime OpenedAt;

        public event ChangedStatusHandler StatusChanged;

        public CircuitBreaker(int maxErrors, int maxSuccess, TimeSpan circuitReset)
        {
            State = CircuitState.Closed;
            MaxErrors = maxErrors;
            MaxSuccess = maxSuccess;
            Errors = 0;
            Success = 0;
            CircuitReset = circuitReset;
        }

        public void Open()
        {
            State = CircuitState.Open;
            OpenedAt = DateTime.UtcNow;

            OnStatusChanged(new NewCircuitState(State));
        }

        public void HalfOpen()
        {
            State = CircuitState.HalfOpen;
            Success = 0;

            OnStatusChanged(new NewCircuitState(State));
        }

        public void Close()
        {
            Errors = 0;
            State = CircuitState.Closed;

            OnStatusChanged(new NewCircuitState(State));
        }

        public CircuitState GetState()
        {
            return State;
        }

        public void SetErrorsCount(int quantity)
        {
            Errors = quantity;

            if (Errors >= MaxErrors)
                Open();
        }

        protected virtual void OnStatusChanged(NewCircuitState e)
        {
            StatusChanged?.Invoke(e);
        }

        public void Execute(Action action)
        {
            switch (State)
            {
                case CircuitState.Closed:
                    ExecuteClosedCircuitAction(action);
                    break;
                case CircuitState.HalfOpen:
                    ExecuteHalfOpenCircuitAction(action);
                    break;
                case CircuitState.Open:
                    ExecuteOpenCircuitAction();
                    break;
                default:
                    break;
            }
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

        private void ExecuteOpenCircuitAction()
        {
            TryHalfOpenCircuit();

            if (State == CircuitState.Open)
                throw new OpenCircuitException("Circuit is open");
        }

        private void ExecuteClosedCircuitAction(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
                IncrementErrors();

                throw;
            }
        }

        private void IncrementErrors()
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