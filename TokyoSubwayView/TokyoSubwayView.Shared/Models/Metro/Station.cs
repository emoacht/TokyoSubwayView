using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TokyoSubwayView.Models.Metro
{
	public class Station
	{
		//[JsonProperty("@id")]
		//public string Id { get; set; }

		//[JsonProperty("@type")]
		//public string type { get; set; }

		[JsonProperty("owl:sameAs")]
		public string StationId { get; set; }

		[JsonProperty("dc:date")]
		public DateTimeOffset Date { get; set; }

		[JsonProperty("dc:title")]
		public string Title { get; set; }

		[JsonProperty("ug:region")]
		public string Region { get; set; }

		[JsonProperty("odpt:operator")]
		public string Operator { get; set; }

		[JsonProperty("odpt:railway")]
		public string RailwayId { get; set; }

		[JsonProperty("odpt:connectingRailway")]
		public string[] ConnectingRailwayId { get; set; }

		//[JsonProperty("odpt:facility")]
		//public string Facility { get; set; }

		//[JsonProperty("odpt:passengerSurvey")]
		//public string[] PassengerSurvey { get; set; }

		[JsonProperty("odpt:stationCode")]
		public string StationCode { get; set; }

		//[JsonProperty("odpt:exit")]
		//public string[] Exit { get; set; }

		//[JsonProperty("@context")]
		//public string Context { get; set; }

		[JsonProperty("geo:lat")]
		public double Latitude { get; set; }

		[JsonProperty("geo:long")]
		public double Longitude { get; set; }
	}
}