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
	public class StationGroupViewModel : RailwayMemberViewModel
	{
		public StationGroupViewModel()
		{
			this.LanguageTag = Settings.Current.LanguageTag;

			ZIndex = 1;

			Trains = new ObservableCollection<TrainViewModel>();
		}


		#region Station Group description

		[DataMember]
		public string GroupTitleJa { get; private set; }
		[DataMember]
		public string GroupTitleEn { get; private set; }

		public string StationGroupDescription
		{
			get
			{
				return (LanguageTag == LanguageSubtag.Ja)
					? GroupTitleJa
					: GroupTitleEn;
			}
		}

		#endregion


		[DataMember]
		public Point GroupLocation
		{
			get { return _groupLocation; }
			private set
			{
				_groupLocation = value;
				RaisePropertyChanged();

				Left = value.X;
				Top = value.Y;
			}
		}
		private Point _groupLocation;


		#region Station Members

		[DataMember]
		public IReadOnlyCollection<StationViewModel> Members
		{
			get { return _members; }
		}
		public List<StationViewModel> _members = new List<StationViewModel>();

		public void AddMember(StationViewModel member)
		{
			_members.Add(member);

			if (_members.Count == 1)
			{
				GroupTitleJa = member.StationTitleJa;
				GroupTitleEn = member.StationTitleEn;
			}

			GroupLocation = new Point(
				Members.Average(x => x.Location.X),
				Members.Average(x => x.Location.Y));

			Diameter = 16D + Members.Count * 1.5D;
		}

		public IReadOnlyList<BadgeViewModel> StationBadges
		{
			get
			{
				if (_stationBadges == null)
				{
					_stationBadges = new List<BadgeViewModel>();

					foreach (var member in Members)
					{
						if (_stationBadges.Any(x => x.StationCode == member.StationCode)) // For Nakano-sakaue
							continue;

						_stationBadges.Add(new BadgeViewModel
						{
							StationCode = member.StationCode,
							RailwayId = member.RailwayId,
						});
					}
				}

				return _stationBadges;
			}
		}
		[DataMember]
		private List<BadgeViewModel> _stationBadges;

		#endregion


		[DataMember]
		public double Diameter
		{
			get { return _diameter; }
			set
			{
				_diameter = value;
				RaisePropertyChanged();
			}
		}
		private double _diameter;

		public TrainState State
		{
			get { return _state; }
			set
			{
				if (_state == value)
					return;

				_state = value;
				RaisePropertyChanged();
			}
		}
		private TrainState _state = default(TrainState);


		public ObservableCollection<TrainViewModel> Trains { get; private set; }


		[DataMember]
		public LanguageSubtag LanguageTag { get; private set; }


		public void UpdateContent(LanguageSubtag languageTag)
		{
			this.LanguageTag = languageTag;

			RaisePropertyChanged(() => StationGroupDescription);

			foreach (var train in Trains)
				train.UpdateContent(languageTag);

			State = CheckState(Trains.ToArray());
		}

		private static TrainState CheckState(TrainViewModel[] trains)
		{
			if (!trains.Any())
				return TrainState.Vacant;

			return trains.Select(x => x.State).OrderBy(x => x).Last();
		}
	}
}