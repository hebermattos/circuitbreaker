using System;

namespace CB.Exceptions
{
    public class OpenCircuitException : Exception
    {
        public OpenCircuitException(string message) : base(message)
        {

        }
    }
}
