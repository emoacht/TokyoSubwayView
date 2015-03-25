using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TokyoSubwayView.Models.Metro
{
	public class Train
	{
		//[JsonProperty("@context")]
		//public string Context { get; set; }

		//[JsonProperty("@type")]
		//public string Type { get; set; }

		//[JsonProperty("@id")]
		//public string Id { get; set; }

		[JsonProperty("dc:date")]
		public DateTimeOffset Date { get; set; }

		[JsonProperty("dct:valid")]
		public DateTimeOffset Valid { get; set; }

		[JsonProperty("odpt:frequency")]
		public int Frequency { get; set; }

		[JsonProperty("odpt:railway")]
		public string RailwayId { get; set; }

		[JsonProperty("owl:sameAs")]
		public string TrainId { get; set; }

		[JsonProperty("odpt:trainNumber")]
		public string TrainNumber { get; set; }

		[JsonProperty("odpt:trainType")]
		public string TrainType { get; set; }

		[JsonProperty("odpt:delay")]
		public int Delay { get; set; }

		[JsonProperty("odpt:startingStation")]
		public string StartingStationId { get; set; }

		[JsonProperty("odpt:terminalStation")]
		public string TerminalStationId { get; set; }

		[JsonProperty("odpt:fromStation")]
		public string FromStationId { get; set; }

		[JsonProperty("odpt:toStation")]
		public string ToStationId { get; set; }

		[JsonProperty("odpt:railDirection")]
		public string RailDirection { get; set; }

		//[JsonProperty("odpt:trainOwner")]
		//public string TrainOwner { get; set; }
	}
}