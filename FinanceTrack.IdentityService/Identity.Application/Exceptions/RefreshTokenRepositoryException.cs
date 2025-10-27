using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Exceptions
{
    public class RefreshTokenRepositoryException : Exception
    {
        public RefreshTokenRepositoryException() { }

        public RefreshTokenRepositoryException(string? message)
            : base(message) { }

        public RefreshTokenRepositoryException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
