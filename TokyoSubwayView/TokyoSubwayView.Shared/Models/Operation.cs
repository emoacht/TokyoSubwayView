﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokyoSubwayView.Common;
using TokyoSubwayView.Models.Metro;
using TokyoSubwayView.ViewModels;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace TokyoSubwayView.Models
{
	public class Operation : NotificationObject
	{
		private readonly ObservableCollection<RailwayMemberViewModel> _core;

		public Operation(ObservableCollection<RailwayMemberViewModel> core)
		{
			this._core = core;
		}


		#region Property

		public DateTimeOffset CreatedTime
		{
			get { return _createdTime; }
			set
			{
				_createdTime = value;
				RaisePropertyChanged();
			}
		}
		private DateTimeOffset _createdTime;

		public TimeSpan ElapsedTime
		{
			get { return _elapsedTime; }
			set
			{
				_elapsedTime = value;
				RaisePropertyChanged();
			}
		}
		private TimeSpan _elapsedTime;

		public string[] DelayedRailwayIds
		{
			get { return _delayedRailwayIds; }
			set
			{
				_delayedRailwayIds = value;
				RaisePropertyChanged();
			}
		}
		private string[] _delayedRailwayIds;

		#endregion


		#region Timer

		private readonly DispatcherTimer _updateTimer = new DispatcherTimer();
		private readonly DispatcherTimer _elapsedTimer = new DispatcherTimer();

		private static readonly TimeSpan _waitLength = TimeSpan.FromSeconds(10); // Waiting time length when data have not been updated
		private static readonly TimeSpan _standardLength = TimeSpan.FromSeconds(60); // Standard interval length of update

		internal void StartTimerBase()
		{
			if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
				return;

			_updateTimer.Tick += UpdateTimerTick;
			_updateTimer.Start();

			_elapsedTimer.Interval = TimeSpan.FromSeconds(1);
			_elapsedTimer.Tick += (_, __) => ElapsedTime = DateTimeOffset.Now - CreatedTime;

			Debug.WriteLine("Update Timer started!");
		}

		private async void UpdateTimerTick(object sender, object e)
		{
			_updateTimer.Stop();

			if (!_core.Any())
				await InitiateBaseAsync();

			var trains = await MetroManager.Current.GetTrainsAsync(TimeSpan.Zero);

			if ((trains != null) && trains.Any())
			{
				if (CreatedTime != trains[0].Date)
				{
					CreatedTime = trains[0].Date;

					if (!_elapsedTimer.IsEnabled)
						_elapsedTimer.Start();

					UpdateBase(trains, Settings.Current.LanguageTag);

					_updateTimer.Interval = trains[0].Valid - DateTimeOffset.Now;

					Debug.WriteLine("Updated! {0:F0} ({1:HH:mm:ss} => Date {2:HH:mm:ss} Valid {3:HH:mm:ss})",
						_updateTimer.Interval.TotalSeconds,
						DateTime.Now, trains[0].Date, trains[0].Valid);
				}
				else
				{
					// "Valid" time seems not always correct and may be earlier than actual updated time.
					_updateTimer.Interval = _waitLength;

					Debug.WriteLine("Tardy! {0} ({1:HH:mm:ss} => Date {2:HH:mm:ss} Valid {3:HH:mm:ss})",
						_updateTimer.Interval.Seconds,
						DateTime.Now, trains[0].Date, trains[0].Valid);
				}
			}
			else
			{
				_updateTimer.Interval = _standardLength; // As fallback
			}

			_updateTimer.Start();
		}

		#endregion


		#region Initiate

		internal async Task InitiateBaseAsync(Size viewerSize = default(Size))
		{
			if (!CanConvert && (viewerSize == default(Size)))
				return;

			var railways = await MetroManager.Current.GetRailwaysAsync();
			var stations = await MetroManager.Current.GetStationsAsync();

			if ((railways == null) || (stations == null))
				return;

			// Check real railways area.
			CheckAreaCoordinates(stations, viewerSize);

			// Clear existing railway members.
			_core.Clear();

			// Filter and sort.
			if ((Settings.Current.RailwayIdPriority != null) && Settings.Current.RailwayIdPriority.Any())
			{
				var buff = new List<Railway>();

				foreach (var railwayId in Settings.Current.RailwayIdPriority)
				{
					var railway = railways.FirstOrDefault(x => x.RailwayId == railwayId);
					if (railway != null)
						buff.Add(railway);
				}

				railways = buff.ToArray();
			}

			// Populate railway members. Reversing is to make the higher priority railway come to the upper layer.
			foreach (var railway in railways.Reverse())
			{
				var stationIds = MetroManager.Current.RailwayIdStationsMap[railway.RailwayId];

				for (int i = 0; i < stationIds.Count; i++)
				{
					var station = stations.FirstOrDefault(x => x.StationId == stationIds[i]);
					if (station == null) // It is unlikely to happen though.
						continue;

					var stationMember = new StationViewModel
					{
						RailwayId = railway.RailwayId,

						StationId = station.StationId,
						StationTitleJa = station.Title,
						StationIndex = i,
						StationCode = station.StationCode,

						Location = ConvertPositionFromReal(station.Longitude, station.Latitude),
					};

					var existingStations = _core.OfType<StationGroupViewModel>()
						.SelectMany(x => x.Members)
						.ToArray();

					var stationGroup = _core.OfType<StationGroupViewModel>()
						.FirstOrDefault(x => x.GroupTitleJa == station.Title);

					if (stationGroup != null)
					{
						stationGroup.AddMember(stationMember);
					}
					else
					{
						stationGroup = new StationGroupViewModel();
						stationGroup.AddMember(stationMember);

						_core.Add(stationGroup);
					}

					if (0 < i)
					{
						var previousStation = existingStations.FirstOrDefault(x => x.StationId == stationIds[i - 1]);
						if (previousStation != null)
						{
							var connector = new ConnectorViewModel
							{
								RailwayId = railway.RailwayId,

								StationIdA = previousStation.StationId,
								StationIdB = stationMember.StationId,

								StationIndexA = previousStation.StationIndex,
								StationIndexB = stationMember.StationIndex,

								LocationA = previousStation.Location,
								LocationB = stationMember.Location,
							};

							_core.Add(connector);
						}
					}
				}
			}

			MetroManager.Current.ClearRailwaysStationsCached();
		}

		#endregion


		#region Conversion

		private bool CanConvert
		{
			get { return (_railwaysArea != Rect.Empty); }
		}

		private Rect _railwaysArea = Rect.Empty;
		private const double _railwaysAreaPadding = 0.008; // To add vacant space around real railways area
		private double _inCanvasZoomFactor;
		private double _inCanvasPaddingLeft;
		private double _inCanvasPaddingTop;

		private void CheckAreaCoordinates(IEnumerable<Station> stations, Size viewerSize)
		{
			if (viewerSize == default(Size))
				return;

			double left = 180D; // Starting value is the greatest in longitude (East longitude, International Date Line).
			double right = 0D;
			double top = 0D;
			double bottom = 90D; // Starting value is the greatest in latitude (North latitude, North pole).

			foreach (var station in stations)
			{
				left = Math.Min(left, station.Longitude);
				right = Math.Max(right, station.Longitude);
				top = Math.Max(top, station.Latitude);
				bottom = Math.Min(bottom, station.Latitude);
			}

			//Debug.WriteLine("x={0} Y={1} Width={2} Height={3}", left, top, right - left, top - bottom);

			_railwaysArea = new Rect(
				left - _railwaysAreaPadding,
				top + _railwaysAreaPadding,
				right - left + _railwaysAreaPadding * 2,
				top - bottom + _railwaysAreaPadding * 2); // Height is latitude and so top is greater than bottom.

			_inCanvasZoomFactor = Math.Min(viewerSize.Width / _railwaysArea.Width, viewerSize.Height / _railwaysArea.Height);
			_inCanvasPaddingLeft = (viewerSize.Width - _railwaysArea.Width * _inCanvasZoomFactor) / 2;
			_inCanvasPaddingTop = (viewerSize.Height - _railwaysArea.Height * _inCanvasZoomFactor) / 2;
		}

		internal Point ConvertPositionFromReal(Point realPosition)
		{
			// If argument is invalid or if not checked yet, return invalid value.
			if ((realPosition == default(Point)) || (_inCanvasZoomFactor == 0D))
				return default(Point);

			return ConvertPositionFromReal(realPosition.X, realPosition.Y);
		}

		private Point ConvertPositionFromReal(double x, double y)
		{
			return new Point(
				(x - _railwaysArea.X) * _inCanvasZoomFactor + _inCanvasPaddingLeft,
				-(y - _railwaysArea.Y) * _inCanvasZoomFactor + _inCanvasPaddingTop);
		}

		internal Point ConvertPositionToReal(Point inCanvasPosition)
		{
			// If argument is invalid or if not checked yet, return invalid value.
			if ((inCanvasPosition == default(Point)) || (_inCanvasZoomFactor == 0D))
				return default(Point);

			return ConvertPositionToReal(inCanvasPosition.X, inCanvasPosition.Y);
		}

		private Point ConvertPositionToReal(double x, double y)
		{
			return new Point(
				(x - _inCanvasPaddingLeft) / _inCanvasZoomFactor + _railwaysArea.X,
				-(y - _inCanvasPaddingTop) / _inCanvasZoomFactor + _railwaysArea.Y);
		}

		internal float ConvertZoomFactorFromReal(double realZoomFactor)
		{
			// If argument is invalid or if not checked yet, return invalid value.
			if ((realZoomFactor == 0D) || (_inCanvasZoomFactor == 0D))
				return 0F;

			return (float)realZoomFactor / (float)_inCanvasZoomFactor;
		}

		internal double ConvertZoomFactorToReal(float viewerZoomFactor)
		{
			// If argument is invalid or if not checked yet, return invalid value.
			if ((viewerZoomFactor == 0D) || (_inCanvasZoomFactor == 0D))
				return 0D;

			return viewerZoomFactor * (float)_inCanvasZoomFactor;
		}

		#endregion


		#region Update

		internal async Task UpdateBaseAsync(LanguageSubtag languageTag)
		{
			if (!CanConvert)
				return;

			var trains = await MetroManager.Current.GetTrainsAsync(_standardLength);
			if (trains == null)
				return;

			UpdateBase(trains, languageTag);
		}

		private void UpdateBase(Train[] trains, LanguageSubtag languageTag)
		{
			var locker = new Object();

			lock (locker)
			{
				foreach (var stationGroup in _core.OfType<StationGroupViewModel>())
				{
					if (trains != null)
					{
						var stationIds = stationGroup.Members.Select(x => x.StationId).ToArray();
						stationGroup.Trains.Clear();

						foreach (var train in trains.Where(x => x.ToStationId == null))
						{
							if (stationIds.Contains(train.FromStationId))
								stationGroup.Trains.Add(new TrainViewModel(train));
						}
					}

					stationGroup.UpdateContent(languageTag);
				}

				foreach (var connector in _core.OfType<ConnectorViewModel>())
				{
					if (trains != null)
					{
						var buffList = new List<TrainViewModel>();

						foreach (var train in trains)
						{
							if (train.FromStationId == connector.StationIdA)
							{
								if (train.ToStationId == connector.StationIdB)
								{
									buffList.Add(new TrainViewModel(train, true, false));
								}
								else if (train.RailwayId == connector.RailwayId)
								{
									var terminalStationIndex = GetStationIndex(train.RailwayId, train.TerminalStationId);
									if ((0 <= terminalStationIndex) &&
										IsIncluded(connector.StationIndexB, connector.StationIndexA, terminalStationIndex))
										buffList.Add(new TrainViewModel(train, true, true));
								}
							}
							else if (train.FromStationId == connector.StationIdB)
							{
								if (train.ToStationId == connector.StationIdA)
								{
									buffList.Add(new TrainViewModel(train, false, false));
								}
								else if (train.RailwayId == connector.RailwayId)
								{
									var terminalStationIndex = GetStationIndex(train.RailwayId, train.TerminalStationId);
									if ((0 <= terminalStationIndex) &&
										IsIncluded(connector.StationIndexA, connector.StationIndexB, terminalStationIndex))
										buffList.Add(new TrainViewModel(train, false, true));
								}
							}
						}

						connector.Trains.Clear();
						foreach (var buffItem in buffList.OrderBy(x => x.IsPhantom).ThenBy(x => x.IsFromA))
							connector.Trains.Add(buffItem);
					}

					connector.UpdateContent(languageTag);
				}

				DelayedRailwayIds = trains
					.GroupBy(x => x.RailwayId)
					.Where(x => x.Any(y => 0 < y.Delay))
					.Select(x => x.First().RailwayId)
					.ToArray();
			}
		}

		private static int GetStationIndex(string railwayId, string stationId)
		{
			if (!MetroManager.Current.RailwayIdStationsMap.ContainsKey(railwayId))
				return -1; // Not found

			return MetroManager.Current.RailwayIdStationsMap[railwayId].FindIndex(x => x == stationId);
		}

		private static bool IsIncluded(int targetIndex, int endIndex1, int endIndex2)
		{
			if (targetIndex < endIndex1)
				return endIndex2 <= targetIndex;
			else
				return targetIndex <= endIndex2;
		}

		#endregion
	}
}