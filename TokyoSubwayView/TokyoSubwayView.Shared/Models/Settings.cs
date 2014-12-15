using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TokyoSubwayView.Common;
using Windows.Foundation;
using Windows.Globalization;
using Windows.Storage;

namespace TokyoSubwayView.Models
{
	[DataContract]
	public class Settings : NotificationObject
	{
		private Settings()
		{ }

		public static Settings Current
		{
			get { return _current ?? (_current = new Settings()); }
			private set { _current = value; }
		}
		private static Settings _current;


		#region Viewer

		[DataMember]
		public Point RealCenterPosition
		{
			get { return GetValue<Point>(DataContainer.Roaming); } // Default (default(Point)) means no valid value has been saved yet.
			set
			{
				SetValue(value, DataContainer.Roaming);
				RaisePropertyChanged();

				//Debug.WriteLine("RealCenterPosition: {0}", value);
			}
		}

		[DataMember]
		public double RealZoomFactor
		{
			get { return GetValue<double>(DataContainer.Roaming); } // Default (0D) means that no valid value has been saved yet.
			set
			{
				SetValue(value, DataContainer.Roaming);
				RaisePropertyChanged();

				//Debug.WriteLine("RealZoomFactor: {0}", value);
			}
		}

		public bool IsInitiating
		{
			get { return _isInitiating; }
			set
			{
				_isInitiating = value;
				RaisePropertyChanged();
			}
		}
		private bool _isInitiating;

		#endregion


		#region Railway Priority

		[DataMember]
		public string[] RailwayIdPriority
		{
			get
			{
				string[] buff;
				return TryGetValue(out buff, DataContainer.Roaming) ? buff : null;
			}
			set
			{
				SetValue(value, DataContainer.Roaming);
				RaisePropertyChanged();

				//Debug.WriteLine("RailwayIdPriority:\r\n{0}", value.Aggregate((work, next) => work + Environment.NewLine + next));
			}
		}

		#endregion


		#region Language

		public static IReadOnlyList<LanguageBag> LanguageList
		{
			get { return _languageList; }
		}
		private static readonly List<LanguageBag> _languageList = new List<LanguageBag>
		{
			new LanguageBag(LanguageSubtag.En, "en-US", "English"),
			new LanguageBag(LanguageSubtag.Ja, "ja-JP", "日本語"),
		};

		public LanguageSubtag LanguageTag
		{
			get
			{
				if (!_languageTag.HasValue)
				{
					var firstLanguageSubtag = new String(ApplicationLanguages.Languages.First().TakeWhile(x => x != '-').ToArray());

					var bag = LanguageList.FirstOrDefault(x => x.Subtag.ToString().Equals(firstLanguageSubtag, StringComparison.OrdinalIgnoreCase));

					_languageTag = (bag != null) ? bag.Subtag : default(LanguageSubtag);
				}

				return _languageTag.Value;
			}
			set
			{
				// Application language will be preserved by OS automatically and so no need to save it here.
				_languageTag = value;
				RaisePropertyChanged();

				Debug.WriteLine("LanguageTag: {0}", value);
			}
		}
		private LanguageSubtag? _languageTag;

		#endregion


		#region Access to AppDataContainer

		private enum DataContainer
		{
			Local = 0,
			Roaming,
		}

		#region One by One

		private static T GetValue<T>(DataContainer container = default(DataContainer), [CallerMemberName] string propertyName = null)
		{
			try
			{
				var values = (container == DataContainer.Local)
					? ApplicationData.Current.LocalSettings.Values
					: ApplicationData.Current.RoamingSettings.Values;

				if (values.ContainsKey(propertyName))
				{
					return (T)values[propertyName];
				}
				else
				{
					return default(T);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to get property value. {0}", ex);
				return default(T);
			}
		}

		private static bool TryGetValue<T>(out T propertyValue, DataContainer container = default(DataContainer), [CallerMemberName] string propertyName = null)
		{
			try
			{
				var values = (container == DataContainer.Local)
					? ApplicationData.Current.LocalSettings.Values
					: ApplicationData.Current.RoamingSettings.Values;

				if (!values.ContainsKey(propertyName))
				{
					propertyValue = default(T);
					return false;
				}
				else
				{
					propertyValue = (T)values[propertyName];
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to try to get property value. {0}", ex);
				propertyValue = default(T);
				return false;
			}
		}

		private static void SetValue<T>(T propertyValue, DataContainer container = default(DataContainer), [CallerMemberName] string propertyName = null)
		{
			try
			{
				var settings = (container == DataContainer.Local)
					? ApplicationData.Current.LocalSettings
					: ApplicationData.Current.RoamingSettings;

				if (settings.Values.ContainsKey(propertyName))
				{
					settings.Values[propertyName] = propertyValue;
				}
				else
				{
					settings.Values.Add(propertyName, propertyValue);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to set property value. {0}", ex);
			}
		}

		#endregion

		#region In Bulk

		private const string settingsKey = "settings";

		public static async Task LoadLocalAsync()
		{
			await LoadAsync(DataContainer.Local);
		}

		private static async Task LoadAsync(DataContainer container = default(DataContainer))
		{
			try
			{
				var settings = (container == DataContainer.Local)
					? ApplicationData.Current.LocalSettings
					: ApplicationData.Current.RoamingSettings;

				if (!settings.Values.ContainsKey(settingsKey))
					return;

				using (var ms = new MemoryStream())
				using (var sw = new StreamWriter(ms))
				{
					await sw.WriteAsync(settings.Values[settingsKey].ToString());

					ms.Seek(0, SeekOrigin.Begin);

					var serializer = new DataContractSerializer(typeof(Settings));
					Current = (Settings)serializer.ReadObject(ms);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to load settings. {0}", ex);
				throw;
			}
		}

		public static async Task SaveLocalAsync()
		{
			await SaveAsync(DataContainer.Local);
		}

		private static async Task SaveAsync(DataContainer container = default(DataContainer))
		{
			try
			{
				var settings = (container == DataContainer.Local)
					? ApplicationData.Current.LocalSettings
					: ApplicationData.Current.RoamingSettings;

				using (var ms = new MemoryStream())
				using (var sr = new StreamReader(ms))
				{
					var serializer = new DataContractSerializer(typeof(Settings));
					serializer.WriteObject(ms, Current);

					ms.Seek(0, SeekOrigin.Begin);

					var buff = await sr.ReadToEndAsync();

					if (settings.Values.ContainsKey(settingsKey))
					{
						settings.Values[settingsKey] = buff;
					}
					else
					{
						settings.Values.Add(settingsKey, buff);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to save settings. {0}", ex);
				throw;
			}
		}

		#endregion

		#endregion
	}
}