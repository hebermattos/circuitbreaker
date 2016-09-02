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
        private int MaxAttempts;
        private int ShouldCloseSucessAttempts;
        private int Errors;
        private int HalfOpenSucess;
        private TimeSpan CircuitReset;
        private DateTime OpenedAt;

        public event ChangedStatusHandler StatusChanged;

        public CircuitBreaker(int maxAttempts, int shouldCloseSucessAttempts, TimeSpan circuitReset)
        {
            State = CircuitState.Closed;
            MaxAttempts = maxAttempts;
            ShouldCloseSucessAttempts = shouldCloseSucessAttempts;
            Errors = 0;
            HalfOpenSucess = 0;
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
            HalfOpenSucess = 0;

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

            if (Errors >= MaxAttempts)
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

            if (Errors >= MaxAttempts)
                Open();
        }

        private void TryHalfOpenCircuit()
        {
            if (DateTime.UtcNow.Subtract(OpenedAt) >= CircuitReset)
                HalfOpen();
        }

        private void TryCloseCircuit()
        {
            HalfOpenSucess++;

            if (HalfOpenSucess >= ShouldCloseSucessAttempts)
                Close();
        }

    }

}