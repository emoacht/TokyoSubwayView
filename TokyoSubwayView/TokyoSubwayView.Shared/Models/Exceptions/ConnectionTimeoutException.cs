using System;
using System.Collections.Generic;
using System.Text;

namespace TokyoSubwayView.Models.Exceptions
{
    public class ConnectionTimeoutException : Exception
    {
        public ConnectionTimeoutException() { }
        public ConnectionTimeoutException(string message) : base(message) { }
        public ConnectionTimeoutException(string message, Exception inner) : base(message, inner) { }
    }
}