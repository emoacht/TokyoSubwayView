using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TokyoSubwayView.Common;
using TokyoSubwayView.Models.Exceptions;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace TokyoSubwayView.Models.Metro
{
	[DataContract]
	public class MetroManager : NotificationObject
	{
		private MetroManager()
		{
			CheckConnectionAvailable();

			NetworkInformation.NetworkStatusChanged += _ => CheckConnectionAvailable();
		}

		public static MetroManager Current
		{
			get { return _current ?? (_current = new MetroManager()); }
			private set { _current = value; }
		}
		private static MetroManager _current;


		#region Internet Connection

		public bool IsConnectionAvailable
		{
			get { return _isConnectionAvailable; }
			set
			{
				if (_isConnectionAvailable == value)
					return;

				_isConnectionAvailable = value;
				RaisePropertyChanged();
			}
		}
		private bool _isConnectionAvailable;

		private async void CheckConnectionAvailable()
		{
			var dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
			if (dispatcher.HasThreadAccess)
			{
				CheckConnectionAvailableBase();
			}
			else
			{
				await dispatcher.RunAsync(CoreDispatcherPriority.Normal, CheckConnectionAvailableBase);
			}
		}

		private void CheckConnectionAvailableBase()
		{
			var profile = NetworkInformation.GetInternetConnectionProfile();

			IsConnectionAvailable = (profile != null) &&
				(profile.GetNetworkConnectivityLevel() >= NetworkConnectivityLevel.InternetAccess);

			if (!IsConnectionAvailable)
			{
				var loader = new ResourceLoader();

				ErrorMessage = loader.GetString("ConnectionUnavailable");
			}
		}

		#endregion


		#region Message

		public string ErrorMessage
		{
			get { return _errorMessage; }
			private set
			{
				if (ErrorMessage == value)
					return;

				_errorMessage = value;
				RaisePropertyChanged();
			}
		}
		private string _errorMessage;

		#endregion


		#region Lists, Maps

		public IReadOnlyList<string> RailwayIds
		{
			get { return _railwayIds; }
		}
		[DataMember]
		private List<string> _railwayIds;

		public IReadOnlyDictionary<string, List<string>> RailwayIdStationsMap
		{
			get { return _railwayIdStationsMap; }
		}
		[DataMember]
		private Dictionary<string, List<string>> _railwayIdStationsMap;

		public IReadOnlyDictionary<string, string> RailwayIdTitleMap
		{
			get { return _railwayIdTitleMap; }
		}
		[DataMember]
		private Dictionary<string, string> _railwayIdTitleMap;

		public IReadOnlyDictionary<string, string> RailwayIdCodeMap
		{
			get { return _railwayIdCodeMap; }
		}
		[DataMember]
		private Dictionary<string, string> _railwayIdCodeMap;

		public IReadOnlyDictionary<string, string> StationIdTitleMap
		{
			get { return _stationIdTitleMap; }
		}
		[DataMember]
		private Dictionary<string, string> _stationIdTitleMap;

		public IReadOnlyDictionary<string, string> TrainTypeMap
		{
			get { return _trainTypeMap ?? (_trainTypeMap = GetMapFromXaml(trainTypePath)); }
		}
		[DataMember]
		private Dictionary<string, string> _trainTypeMap;

		public IReadOnlyDictionary<string, Brush> RailwayIdBrushMap
		{
			get
			{
				if (!_railwayIdBrushMap.Any() && (RailwayIds != null))
				{
					var brushKeys = Application.Current.Resources.MergedDictionaries
						.SelectMany(x => x)
						.Where(x => x.Value is Brush)
						.Select(x => x.Key.ToString())
						.Where(x => x.Contains("Railway"))
						.ToArray();

					foreach (string railwayId in RailwayIds)
					{
						var matchingKey = brushKeys.FirstOrDefault(x => x.Contains(MetroHelper.GetRailwayTitleEn(railwayId)));
						if (matchingKey == null)
							continue; // TODO: Consider to add fallback

						_railwayIdBrushMap.Add(railwayId, (Brush)Application.Current.Resources[matchingKey]);
					}
				}

				return _railwayIdBrushMap;
			}
		}
		// Brush cannot be serialized.
		private Dictionary<string, Brush> _railwayIdBrushMap = new Dictionary<string, Brush>();

		public IReadOnlyDictionary<TrainState, Brush> TrainStateBrushMap
		{
			get
			{
				if (!_trainStateBrushMap.Any())
				{
					var brushKeys = Application.Current.Resources.MergedDictionaries
						.SelectMany(x => x)
						.Where(x => x.Value is Brush)
						.Select(x => x.Key.ToString())
						.Where(x => x.Contains("TrainState"))
						.ToArray();

					foreach (string name in Enum.GetNames(typeof(TrainState)))
					{
						var matchingKey = brushKeys.FirstOrDefault(x => x.Contains(name));
						if (matchingKey == null)
							continue; // Any fallback value?

						_trainStateBrushMap.Add((TrainState)Enum.Parse(typeof(TrainState), name), (Brush)Application.Current.Resources[matchingKey]);
					}
				}

				return _trainStateBrushMap;
			}
		}
		// Brush cannot be serialized.
		private Dictionary<TrainState, Brush> _trainStateBrushMap = new Dictionary<TrainState, Brush>();

		#endregion


		#region Railways, Stations, Trains

		private Railway[] _railways;
		private Station[] _stations;
		private Train[] _trains;

		private DateTimeOffset cachedTime;
		private bool isCaching = false;

		private int errorCount = 0;
		private int errorCountMax = 3;

		private const string stationExtraPath = "ms-appx:///Models/Metro/StationExtra.json";
		private const string trainTypePath = "ms-appx:///Models/Metro/TrainType.xaml";

		public async Task GetRailwaysStationsTrainsCachedAsync()
		{
			try
			{
				isCaching = true;

				await GetRailwaysAsync();
				await GetStationsAsync();
				await GetTrainsAsync(TimeSpan.Zero);
			}
			finally
			{
				isCaching = false;
			}
		}

		private async Task Import(Railway[] railways, Station[] stations)
		{
			if ((railways == null) || (stations == null))
				return;

			Current._railwayIds = railways
				.Select(x => x.RailwayId)
				.ToList();
			RaisePropertyChanged(() => RailwayIds);

			Current._railwayIdStationsMap = railways.ToDictionary(
				x => x.RailwayId,
				x => x.StationOrder
					.OrderBy(y => y.Index)
					.Select(y => y.StationId)
					.ToList());
			RaisePropertyChanged(() => RailwayIdStationsMap);

			Current._railwayIdTitleMap = railways.ToDictionary(x => x.RailwayId, x => x.Title);
			RaisePropertyChanged(() => RailwayIdTitleMap);

			Current._railwayIdCodeMap = railways.ToDictionary(x => x.RailwayId, x => x.LineCode);
			RaisePropertyChanged(() => RailwayIdCodeMap);

			Current._stationIdTitleMap = stations.ToDictionary(x => x.StationId, x => x.Title)
				.Concat(await GetMapFromJsonAsync(stationExtraPath))
				.GroupBy(x => x.Key)
				.ToDictionary(x => x.Key, x => x.First().Value);
			RaisePropertyChanged(() => StationIdTitleMap);
		}

		public void ClearRailwaysStationsCached()
		{
			_railways = null;
			_stations = null;
		}

		public async Task<Railway[]> GetRailwaysAsync()
		{
			if ((_railways == null) && IsConnectionAvailable)
			{
				try
				{
					_railways = await MetroAccessor.GetRailwaysAsync(true);
					await Import(_railways, _stations);
					errorCount = 0;
				}
				catch (Exception ex)
				{
					HandleException(ex);
				}
			}

			return _railways;
		}

		public async Task<Station[]> GetStationsAsync()
		{
			if ((_stations == null) && IsConnectionAvailable)
			{
				try
				{
					_stations = await MetroAccessor.GetStationsAsync(true);
					await Import(_railways, _stations);
					errorCount = 0;
				}
				catch (Exception ex)
				{
					HandleException(ex);
				}
			}

			return _stations;
		}

		public async Task<Train[]> GetTrainsAsync(TimeSpan timeLength)
		{
			if (((_trains == null) || (cachedTime + timeLength < DateTimeOffset.Now))
				&& IsConnectionAvailable)
			{
				try
				{
					_trains = await MetroAccessor.GetTrainsAsync();
					cachedTime = DateTimeOffset.Now;
					errorCount = 0;
				}
				catch (Exception ex)
				{
					HandleException(ex);
				}
			}

			return _trains;
		}

		private void HandleException(Exception ex)
		{
			var message = CheckException(ex);

			if (!isCaching && IsConnectionAvailable)
			{
				errorCount++;
				if (errorCount < errorCountMax)
					return;
			}

			ErrorMessage = message;
		}

		private string CheckException(Exception ex)
		{
			var loader = new ResourceLoader();

			var cue = ex as ConnectionUnavailableException;
			if (cue != null)
			{
				IsConnectionAvailable = false;

				var message = loader.GetString("ConnectionUnavailable");
				return message;
			}

			var mce = ex as ConnectionFailureException;
			if (mce != null)
			{
				var message = loader.GetString("ConnectionFailure");
#if DEBUG
				var detailed = String.Format("Connection failed. {0} {1}", mce.StatusCode, mce.ReasonPhrase);
				Debug.WriteLine(detailed);
				message += Environment.NewLine + detailed;
#endif
				return message;
			}

			var mte = ex as ConnectionTimeoutException;
			if (mte != null)
			{
				var message = loader.GetString("ConnectionTimeout");
#if DEBUG
				var detailed = String.Format("Connection timed out.");

				var innerException = mte.InnerException;
				if (innerException != null)
					detailed += String.Format(" {0}", innerException.GetType().FullName);

				Debug.WriteLine(detailed);
				message += Environment.NewLine + detailed;
#endif
				return message;
			}

			{
				var message = loader.GetString("ConnectionOtherError");
#if DEBUG
				var detailed = String.Format("Failed. {0}", ex);
				Debug.WriteLine(detailed);
				message += Environment.NewLine + detailed;
#endif
				return message;
			}
		}

		#endregion


		#region Helper

		public static async Task<Dictionary<string, string>> GetMapFromJsonAsync(string filePath)
		{
			var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(filePath));
			var json = await FileIO.ReadTextAsync(file);

			var outcome = new Dictionary<string, string>();

			try
			{
				JsonObject buff;
				if (JsonObject.TryParse(json, out buff))
				{
					foreach (var key in buff.Keys)
					{
						if (buff[key].ValueType == JsonValueType.String)
							outcome.Add(key, buff[key].GetString());
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to parse json. {0}", ex);
				throw;
			}

			foreach (var item in outcome)
				Debug.WriteLine("{0}-{1}", item.Key, item.Value);

			return outcome;
		}

		public static Dictionary<string, string> GetMapFromXaml(string filePath)
		{
			var outcome = new Dictionary<string, string>();

			try
			{
				var dic = new ResourceDictionary { Source = new Uri(filePath, UriKind.Absolute) };

				foreach (var key in dic.Keys)
					outcome.Add(key.ToString(), dic[key].ToString());
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				if ((uint)ex.HResult != 0x80004005) // E_FAIL
					throw;
			}

			foreach (var item in outcome)
				Debug.WriteLine("{0}={1}", item.Key, item.Value);

			return outcome;
		}

		#endregion


		#region Load/Save

		private const string dataFileName = "Data.xml";

		public async Task LoadAsync()
		{
			var localFolder = ApplicationData.Current.LocalFolder;

			var file = await localFolder.TryGetItemAsync(dataFileName) as StorageFile;
			if (file == null)
				return;

			using (var stream = await file.OpenStreamForReadAsync())
			{
				var serializer = new DataContractSerializer(typeof(MetroManager));
				_current = (MetroManager)serializer.ReadObject(stream);
			}
		}

		public async Task SaveAsync()
		{
			var localFolder = ApplicationData.Current.LocalFolder;

			var file = await localFolder.CreateFileAsync(dataFileName, CreationCollisionOption.ReplaceExisting);
			using (var stream = await file.OpenStreamForWriteAsync())
			{
				var serializer = new DataContractSerializer(typeof(MetroManager));
				serializer.WriteObject(stream, _current);

				await stream.FlushAsync();
			}
		}

		#endregion
	}
}