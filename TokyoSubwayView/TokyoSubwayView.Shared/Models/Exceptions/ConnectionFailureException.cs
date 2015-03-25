using System;
using System.Collections.Generic;
using System.Text;
using Windows.Web.Http;

namespace TokyoSubwayView.Models.Exceptions
{
	public class ConnectionFailureException : Exception
	{
		public HttpStatusCode StatusCode { get; private set; }
		public string ReasonPhrase { get; private set; }

		public ConnectionFailureException() { }
		public ConnectionFailureException(string message) : base(message) { }
		public ConnectionFailureException(string message, Exception inner) : base(message, inner) { }

		public ConnectionFailureException(string message, HttpStatusCode statusCode, string reasonPhrase)
			: base(message)
		{
			this.StatusCode = statusCode;
			this.ReasonPhrase = reasonPhrase;
		}
	}
}