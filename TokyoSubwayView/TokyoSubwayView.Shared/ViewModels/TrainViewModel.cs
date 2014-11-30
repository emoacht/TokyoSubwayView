using System;
using System.Collections.Generic;
using System.Text;
using TokyoSubwayView.Models;
using TokyoSubwayView.Models.Metro;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Media;

namespace TokyoSubwayView.ViewModels
{
	public class TrainViewModel : ViewModelBase
	{
		public TrainViewModel()
		{
			this.LanguageTag = Settings.Current.LanguageTag;
		}

		public TrainViewModel(Train train)
			: this()
		{
			this.RailwayId = train.RailwayId;

			this.TrainId = train.TrainId;
			this.TrainNumber = train.TrainNumber;
			this.TrainType = train.TrainType;

			this.Delay = train.Delay;

			this.StartingStationId = train.StartingStationId;
			this.TerminalStationId = train.TerminalStationId;

			this.FromStationId = train.FromStationId;
			this.ToStationId = train.ToStationId;

			this.RailDirection = train.RailDirection;
		}

		public TrainViewModel(Train train, bool isFromA, bool isPhantom)
			: this(train)
		{
			this.IsFromA = isFromA;
			this.IsPhantom = isPhantom;
		}


		#region Railway description

		public string RailwayId { get; private set; }

		public string RailwayDescription
		{
			get { return MetroHelper.GetRailwayDescription(RailwayId, LanguageTag); }
		}

		#endregion


		#region Train Type description

		public string TrainId { get; private set; }
		public string TrainNumber { get; private set; }
		public string TrainType { get; private set; }

		public string TrainTypeDescription
		{
			get { return MetroHelper.GetTrainTypeDescription(TrainType, LanguageTag) + Environment.NewLine + TrainNumber; }
		}

		#endregion


		public int Delay { get; set; }


		#region Starting/Terminal Stations

		public string StartingStationId { get; set; }
		public string TerminalStationId { get; set; }

		public string TerminalStationDescription
		{
			get { return String.Format("({0})", MetroHelper.GetStationDescription(TerminalStationId, LanguageTag)); }
		}

		#endregion


		public string FromStationId { get; set; }
		public string ToStationId { get; set; }


		#region Rail Direction description

		public string RailDirection { get; private set; }

		public string RailDirectionDescription
		{
			get { return MetroHelper.GetRailDirectionDescription(RailDirection, LanguageTag); }
		}

		#endregion


		#region State

		public TrainState State
		{
			get
			{
				if (IsPhantom)
					return TrainState.Phantom;

				if (Delay == 0)
					return TrainState.OnTime;

				return (Delay < 900) // 15 min * 60 = 900 sec
					? TrainState.DelayShort
					: TrainState.DelayLong;
			}
		}

		#endregion


		#region Note

		public string Note
		{
			get
			{
				var loader = new ResourceLoader();
				var sb = new StringBuilder();

				if (!IsPhantom)
				{
					if (ToStationId != null)
					{
						sb.Append(loader.GetString("CaptionRunning") + Environment.NewLine);
					}
				}
				else
				{
					sb.Append(loader.GetString("CaptionExpected") + Environment.NewLine);
				}

				if (0 < Delay)
				{
					sb.Append(String.Format(loader.GetString("CaptionMinDelay"), Math.Round((double)Delay / 60D)));
				}

				return sb.ToString();
			}
		}

		#endregion


		public bool IsFromA { get; private set; }
		public bool IsFromB
		{
			get { return !IsFromA; }
			private set { IsFromA = !value; }
		}

		public bool IsPhantom { get; private set; }


		public LanguageSubtag LanguageTag { get; private set; }


		public void UpdateContent(LanguageSubtag languageTag)
		{
			this.LanguageTag = languageTag;

			RaisePropertyChanged(() => RailwayId);
			RaisePropertyChanged(() => RailwayDescription);
			RaisePropertyChanged(() => TrainTypeDescription);
			RaisePropertyChanged(() => TerminalStationDescription);
			RaisePropertyChanged(() => RailDirectionDescription);
			RaisePropertyChanged(() => State);
			RaisePropertyChanged(() => Note);
		}
	}
}