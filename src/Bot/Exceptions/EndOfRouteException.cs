using System;

namespace Bot.Exceptions
{
    class EndOfRouteException : Exception
    {
        public EndOfRouteException() : base()
        {
        }

        public EndOfRouteException(string message) : base(message)
        {
        }

        public EndOfRouteException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
