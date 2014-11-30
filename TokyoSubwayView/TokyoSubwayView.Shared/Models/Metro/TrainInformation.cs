using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TokyoSubwayView.Models.Metro
{
	public class TrainInformation
	{
		//[JsonProperty("@context")]
		//public string Context { get; set; }

		//[JsonProperty("@id")]
		//public string Id { get; set; }

		[JsonProperty("dc:date")]
		public DateTimeOffset Date { get; set; }

		[JsonProperty("dct:valid")]
		public DateTimeOffset Valid { get; set; }

		[JsonProperty("odpt:operator")]
		public string Operator { get; set; }

		[JsonProperty("odpt:railway")]
		public string RailwayId { get; set; }

		[JsonProperty("odpt:timeOfOrigin")]
		public DateTimeOffset TimeOfOrigin { get; set; }

		[JsonProperty("odpt:trainInformationText")]
		public string TrainInformationText { get; set; }

		//[JsonProperty("@type")]
		//public string Type { get; set; }
	}
}