﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Exceptions
{
    public class LoginUserException : Exception
    {
        public LoginUserException() { }

        public LoginUserException(string? message)
            : base(message) { }

        public LoginUserException(string? message, Exception? innerException)
            : base(message, innerException) { }
    }
}
