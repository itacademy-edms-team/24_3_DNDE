using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Exceptions
{
    public class CreateUserException : Exception
    {
        public CreateUserException() { }

        public CreateUserException(string? message)
            : base(message) { }

        public CreateUserException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
