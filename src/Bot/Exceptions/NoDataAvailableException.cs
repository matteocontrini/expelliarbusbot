using System;

namespace Bot.Exceptions
{
    class DataNotAvailableException : Exception
    {
        public DataNotAvailableException() : base()
        {
        }

        public DataNotAvailableException(string message) : base(message)
        {
        }

        public DataNotAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
