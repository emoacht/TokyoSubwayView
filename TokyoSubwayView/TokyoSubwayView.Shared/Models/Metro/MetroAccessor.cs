using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TokyoSubwayView.Models.Exceptions;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace TokyoSubwayView.Models.Metro
{
	public class MetroAccessor
	{
		#region Method (Public)

		private const string railwaysCacheFileName = "Railways.json";
		private const string stationsCacheFileName = "Stations.json";
		private const string trainsUpdateFileName = "Trains.json";

		public static async Task<Railway[]> GetRailwaysAsync(bool usesCache = false)
		{
			var uri = ComposeUri(InfoType.DataPoints, MethodType.Query, ComposeDataParameter(DataType.Railway));

			var json = (usesCache)
				? await GetStringCachedAsync(uri, railwaysCacheFileName)
				: await GetStringAsync(uri);

			Debug.WriteLine("Railways\r\n" + json);

			return JsonConvert.DeserializeObject<Railway[]>(json);
		}

		public static async Task<Station[]> GetStationsAsync(bool usesCache = false)
		{
			var uri = ComposeUri(InfoType.DataPoints, MethodType.Query, ComposeDataParameter(DataType.Station));

			var json = (usesCache)
				? await GetStringCachedAsync(uri, stationsCacheFileName)
				: await GetStringAsync(uri);

			Debug.WriteLine("Stations\r\n" + json);

			return JsonConvert.DeserializeObject<Station[]>(json);
		}

		public static async Task<Train[]> GetTrainsAsync()
		{
			var uri = ComposeUri(InfoType.DataPoints, MethodType.Query, ComposeDataParameter(DataType.Train));

#if DEBUG
			var json = await GetStringUpdatedAsync(uri, trainsUpdateFileName);
#endif
#if !DEBUG
			var json = await GetStringAsync(uri);
#endif

			Debug.WriteLine("Trains\r\n" + json);

			return JsonConvert.DeserializeObject<Train[]>(json);
		}

		public static async Task<TrainInformation[]> GetTrainInformationAsync()
		{
			var uri = ComposeUri(InfoType.DataPoints, MethodType.Query, ComposeDataParameter(DataType.TrainInformation));

			var json = await GetStringAsync(uri);

			Debug.WriteLine("TrainInformation\r\n" + json);

			return JsonConvert.DeserializeObject<TrainInformation[]>(json);
		}

		#endregion


		#region Method (Private)

		private static async Task<string> GetStringCachedAsync(Uri targetUri, string cacheFileName)
		{
			string json = null;

			var localFolder = ApplicationData.Current.LocalFolder;

			var file = await localFolder.TryGetItemAsync(cacheFileName) as StorageFile;
			if (file != null)
			{
				var modifiedTime = (await file.GetBasicPropertiesAsync()).DateModified;

				var dateTimeOffsetNowJst = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(9));
				var dateTimeOffsetTodayJst = new DateTimeOffset(dateTimeOffsetNowJst.Year, dateTimeOffsetNowJst.Month, dateTimeOffsetNowJst.Day, 0, 0, 0, TimeSpan.FromHours(9));

				var borderTime = (dateTimeOffsetNowJst.TimeOfDay < TimeSpan.FromHours(3))
					? dateTimeOffsetTodayJst.AddHours(-21) // Yesterday 03:00:00 in JST
					: dateTimeOffsetTodayJst.AddHours(3); // Today 03:00:00 in JST

				if (borderTime < modifiedTime)
				{
					json = await FileIO.ReadTextAsync(file);
				}
			}

			if (String.IsNullOrEmpty(json))
			{
				json = await GetStringAsync(targetUri);

				file = await localFolder.CreateFileAsync(cacheFileName, CreationCollisionOption.ReplaceExisting);
				await FileIO.WriteTextAsync(file, json);
			}

			return json;
		}

		private static DateTimeOffset lastUpdatedTime;

		private static async Task<string> GetStringUpdatedAsync(Uri targetUri, string updateFileName)
		{
			string json = null;

			var localFolder = ApplicationData.Current.LocalFolder;

			var file = await localFolder.TryGetItemAsync(updateFileName) as StorageFile;
			if (file != null)
			{
				var updatedTime = (await file.GetBasicPropertiesAsync()).DateModified;

				if (lastUpdatedTime < updatedTime)
				{
					lastUpdatedTime = updatedTime;

					json = await FileIO.ReadTextAsync(file);
				}
			}

			if (String.IsNullOrEmpty(json))
			{
				json = await GetStringAsync(targetUri);
			}

			return json;
		}

		private const int retryCountMax = 3; // Maximum retry count
		private static readonly TimeSpan retryLength = TimeSpan.FromSeconds(3); // Waiting time length before retry
		private static readonly TimeSpan timeoutLength = TimeSpan.FromSeconds(20); // Timeout length

		private static async Task<string> GetStringAsync(Uri targetUri)
		{
			int retryCount = 0;

			using (var cts = new CancellationTokenSource(timeoutLength))
			using (var filter = new HttpBaseProtocolFilter())
			{
				filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;

				using (var client = new HttpClient(filter))
				{
					while (true)
					{
						retryCount++;
						var retryWait = retryLength;

						try
						{
							try
							{
								var response = await client.GetAsync(targetUri).AsTask(cts.Token).ConfigureAwait(false);

								if (!response.IsSuccessStatusCode)
								{
									Debug.WriteLine("ResponseCode {0}", response.StatusCode);

									if ((response.StatusCode == HttpStatusCode.ServiceUnavailable) &&
										(response.Headers.RetryAfter != null)) // RetryAfter seems to be always null though.
									{
										if (response.Headers.RetryAfter.Delta.HasValue)
										{
											retryWait = response.Headers.RetryAfter.Delta.Value;
										}
										else if (response.Headers.RetryAfter.Date.HasValue)
										{
											retryWait = response.Headers.RetryAfter.Date.Value - DateTimeOffset.Now;
										}
									}
									throw new ConnectionFailureException("Connection failed.", response.StatusCode, response.ReasonPhrase);
								}

								var buff = await response.Content.ReadAsBufferAsync();

								using (var stream = buff.AsStream())
								using (var reader = new StreamReader(stream))
								{
									return await reader.ReadToEndAsync();
								}
							}
							catch (UnauthorizedAccessException)
							{
								throw new ConnectionUnavailableException();
							}
							catch (OperationCanceledException)
							{
								throw;
							}
							catch
							{
								if (retryCount >= retryCountMax)
									throw;
							}

							await Task.Delay(retryWait, cts.Token);
						}
						catch (Exception ex)
						{
							if ((ex.GetType() == typeof(OperationCanceledException)) ||
								(ex.GetType() == typeof(TaskCanceledException)))
								throw new ConnectionTimeoutException("Timed out.", ex);

							throw;
						}
					}
				}
			}
		}

		#endregion


		#region Uri

		private const string endPointUrl = "https://api.tokyometroapp.jp/api/v2/";

		private enum InfoType
		{
			DataPoints,
			Places,
		}

		private enum MethodType
		{
			Query,
			Retrieve,
		}

		private enum DataType
		{
			Railway,
			Station,
			Train,
			TrainInformation,
		}

		private static readonly Dictionary<DataType, string> dataMap = new Dictionary<DataType, string>
		{
			{DataType.Railway, "odpt:Railway"},
			{DataType.Station, "odpt:Station"},
			{DataType.Train, "odpt:Train"},
			{DataType.TrainInformation, "odpt:TrainInformation"},			
		};

		private static string ComposeDataParameter(DataType data)
		{
			return String.Format("rdf:type={0}", dataMap[data]);
		}

		private static Uri ComposeUri(InfoType info, MethodType method, string parameter)
		{
			var sb = new StringBuilder(endPointUrl);
			sb.Append((info == InfoType.DataPoints) ? "datapoints" : "places");
			sb.Append((method == MethodType.Query) ? '?' : '/');
			sb.Append(parameter);
			sb.Append((method == MethodType.Query) ? '&' : '?');
			sb.Append(String.Format("acl:consumerKey={0}", Supplement.Key));
			var url = sb.ToString();

			return new Uri(url, UriKind.Absolute);
		}

		#endregion
	}
}