using System;

namespace Identity.Application.Exceptions
{
    public class UpdateUserException : Exception
    {
        public UpdateUserException() { }

        public UpdateUserException(string message)
            : base(message) { }

        public UpdateUserException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
