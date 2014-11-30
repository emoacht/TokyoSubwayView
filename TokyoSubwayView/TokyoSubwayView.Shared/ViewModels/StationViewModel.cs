using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using TokyoSubwayView.Models.Metro;
using Windows.Foundation;

namespace TokyoSubwayView.ViewModels
{
	[DataContract]
	public class StationViewModel : ViewModelBase
	{
		[DataMember]
		public string RailwayId { get; set; }

		[DataMember]
		public string StationId { get; set; }

		[DataMember]
		public string StationTitleJa { get; set; }

		public string StationTitleEn
		{
			get { return _stationTitleEn ?? (_stationTitleEn = MetroHelper.GetStationTitleEn(StationId)); }
		}
		private string _stationTitleEn;

		[DataMember]
		public int StationIndex { get; set; }

		[DataMember]
		public string StationCode { get; set; }

		[DataMember]
		public Point Location { get; set; }
	}
}