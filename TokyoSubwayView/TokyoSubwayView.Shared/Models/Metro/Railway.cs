using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TokyoSubwayView.Models.Metro
{
	public class Railway
	{
		//[JsonProperty("@context")]
		//public string Context { get; set; }

		//[JsonProperty("@id")]
		//public string Id { get; set; }

		//[JsonProperty("@type")]
		//public string Type { get; set; }

		[JsonProperty("owl:sameAs")]
		public string RailwayId { get; set; }

		[JsonProperty("dc:title")]
		public string Title { get; set; }

		[JsonProperty("odpt:stationOrder")]
		public StationOrder[] StationOrder { get; set; }

		//[JsonProperty("odpt:travelTime")]
		//public Traveltime[] TravelTime { get; set; }

		[JsonProperty("odpt:lineCode")]
		public string LineCode { get; set; }

		//[JsonProperty("odpt:womenOnlyCar")]
		//public WomenOnlyCar[] WomenOnlyCar { get; set; }

		[JsonProperty("ug:region")]
		public string Region { get; set; }

		[JsonProperty("dc:date")]
		public DateTimeOffset Date { get; set; }

		[JsonProperty("odpt:operator")]
		public string Operator { get; set; }
	}

	public class StationOrder
	{
		[JsonProperty("odpt:station")]
		public string StationId { get; set; }

		[JsonProperty("odpt:index")]
		public int Index { get; set; }
	}

	//public class Traveltime
	//{
	//	[JsonProperty("odpt:fromStation")]
	//	public string FromStation { get; set; }

	//	[JsonProperty("odpt:toStation")]
	//	public string ToStation { get; set; }

	//	[JsonProperty("odpt:necessaryTime")]
	//	public int NecessaryTime { get; set; }

	//	[JsonProperty("odpt:trainType")]
	//	public string TrainType { get; set; }
	//}

	//public class WomenOnlyCar
	//{
	//	[JsonProperty("odpt:fromStation")]
	//	public string FromStation { get; set; }

	//	[JsonProperty("odpt:toStation")]
	//	public string ToStation { get; set; }

	//	[JsonProperty("odpt:operationDay")]
	//	public string OperationDay { get; set; }

	//	[JsonProperty("odpt:availableTimeFrom")]
	//	public string AvailableTimeFrom { get; set; }

	//	[JsonProperty("odpt:availableTimeUntil")]
	//	public string AvailableTimeUntil { get; set; }

	//	[JsonProperty("odpt:carComposition")]
	//	public int CarComposition { get; set; }

	//	[JsonProperty("odpt:carNumber")]
	//	public int CarNumber { get; set; }
	//}
}