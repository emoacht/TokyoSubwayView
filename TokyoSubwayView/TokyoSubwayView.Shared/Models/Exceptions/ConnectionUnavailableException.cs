using System;
using System.Collections.Generic;
using System.Text;

namespace TokyoSubwayView.Models.Exceptions
{
    public class ConnectionUnavailableException : Exception
    {
        public ConnectionUnavailableException() { }
        public ConnectionUnavailableException(string message) : base(message) { }
        public ConnectionUnavailableException(string message, Exception inner) : base(message, inner) { }
    }
}