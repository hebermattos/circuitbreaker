using System;

namespace NetCircuitBreaker.Exceptions
{
    public class OpenCircuitException : Exception
    {
        public OpenCircuitException(string message) : base(message)
        {

        }
    }
}
