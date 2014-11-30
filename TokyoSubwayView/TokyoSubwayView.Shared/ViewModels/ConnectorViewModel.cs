using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TokyoSubwayView.Models;
using TokyoSubwayView.Models.Metro;
using Windows.Foundation;

namespace TokyoSubwayView.ViewModels
{
	[DataContract]
	public class ConnectorViewModel : RailwayMemberViewModel
	{
		public ConnectorViewModel()
		{
			this.LanguageTag = Settings.Current.LanguageTag;

			ZIndex = 0;

			Trains = new ObservableCollection<TrainViewModel>();
		}


		#region Railway description

		[DataMember]
		public string RailwayId { get; set; }

		public string RailwayDescription
		{
			get { return MetroHelper.GetRailwayDescription(RailwayId, LanguageTag); }
		}

		#endregion


		#region Stations

		[DataMember]
		public string StationIdA { get; set; }
		[DataMember]
		public string StationIdB { get; set; }

		[DataMember]
		public int StationIndexA { get; set; }
		[DataMember]
		public int StationIndexB { get; set; }

		[DataMember]
		public Point LocationA
		{
			get { return _locationA; }
			set
			{
				_locationA = value;
				Left = value.X;
				Top = value.Y;
			}
		}
		private Point _locationA;

		[DataMember]
		public Point LocationB { get; set; }

		public TrainState StateFromA
		{
			get { return _stateFromA; }
			set
			{
				if (_stateFromA == value)
					return;

				_stateFromA = value;
				RaisePropertyChanged();
			}
		}
		private TrainState _stateFromA = default(TrainState);

		public TrainState StateFromB
		{
			get { return _stateFromB; }
			set
			{
				if (_stateFromB == value)
					return;

				_stateFromB = value;
				RaisePropertyChanged();
			}
		}
		private TrainState _stateFromB = default(TrainState);

		#endregion


		public ObservableCollection<TrainViewModel> Trains { get; private set; }


		[DataMember]
		public LanguageSubtag LanguageTag { get; private set; }


		public void UpdateContent(LanguageSubtag languageTag)
		{
			this.LanguageTag = languageTag;

			RaisePropertyChanged(() => RailwayDescription);

			foreach (var train in Trains)
				train.UpdateContent(languageTag);

			StateFromA = CheckState(Trains.Where(x => x.IsFromA).ToArray());
			StateFromB = CheckState(Trains.Where(x => x.IsFromB).ToArray());
		}

		private static TrainState CheckState(TrainViewModel[] trains)
		{
			if (!trains.Any())
				return TrainState.Vacant;

			return trains.Select(x => x.State).OrderBy(x => x).Last();
		}
	}
}