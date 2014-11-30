using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml;

namespace TokyoSubwayView.Models.Metro
{
	public class MetroHelper
	{
		#region Railway Title

		public static string GetRailwayTitleJa(string railwayId)
		{
			if (String.IsNullOrWhiteSpace(railwayId))
				return String.Empty;

			return (MetroManager.Current.RailwayIdTitleMap.ContainsKey(railwayId))
				? MetroManager.Current.RailwayIdTitleMap[railwayId]
				: railwayId.Split(new[] { '.', ':' }).Last();
		}

		public static string GetRailwayTitleEn(string railwayId)
		{
			if (String.IsNullOrWhiteSpace(railwayId))
				return String.Empty;

			var buff = railwayId.Split(new[] { '.', ':' }).Last();

			return buff.Replace("Branch", String.Empty); // For MarunouchiBranch
		}

		public static string GetRailwayDescription(string railwayId, LanguageSubtag languageTag)
		{
			var format = new ResourceLoader().GetString("CaptionLine");

			return (languageTag == LanguageSubtag.Ja)
				? String.Format(format, GetRailwayTitleJa(railwayId))
				: String.Format(format, GetRailwayTitleEn(railwayId));
		}

		#endregion


		#region Train Type

		public static string GetTrainTypeNameJa(string trainType)
		{
			if (String.IsNullOrWhiteSpace(trainType))
				return String.Empty;

			var buff = trainType.Split(new[] { '.', ':' }).Last();

			return (MetroManager.Current.TrainTypeMap.ContainsKey(buff))
				? MetroManager.Current.TrainTypeMap[buff]
				: buff;
		}

		public static string GetTrainTypeNameEn(string trainType)
		{
			if (String.IsNullOrWhiteSpace(trainType))
				return String.Empty;

			var buff = trainType.Split(new[] { '.', ':' }).Last();

			var sb = new StringBuilder();
			foreach (var chr in buff.ToCharArray())
			{
				if ((0 < sb.Length) && Char.IsUpper(chr))
					sb.Append(" " + chr);
				else
					sb.Append(chr);
			}
			return sb.ToString();
		}

		public static string GetTrainTypeDescription(string trainType, LanguageSubtag languageTag)
		{
			return ((languageTag == LanguageSubtag.Ja)
				? GetTrainTypeNameJa(trainType)
				: GetTrainTypeNameEn(trainType));
		}

		#endregion


		#region Station Title

		public static string GetStationTitleJa(string stationId)
		{
			return (MetroManager.Current.StationIdTitleMap.ContainsKey(stationId))
				? MetroManager.Current.StationIdTitleMap[stationId]
				: stationId.Split(new[] { '.', ':' }).Last();
		}

		public static string GetStationTitleEn(string stationId)
		{
			if (String.IsNullOrWhiteSpace(stationId))
				return String.Empty;

			var buff = stationId.Split(new[] { '.', ':' }).Last();

			var sb = new StringBuilder();
			foreach (var chr in buff.ToCharArray())
			{
				if ((0 < sb.Length) && Char.IsUpper(chr))
					sb.Append("-" + Char.ToLower(chr));
				else
					sb.Append(chr);
			}
			return sb.ToString();
		}

		public static string GetStationDescription(string stationId, LanguageSubtag languageTag)
		{
			return (languageTag == LanguageSubtag.Ja)
				? GetStationTitleJa(stationId)
				: GetStationTitleEn(stationId);
		}

		#endregion


		#region Rail Direction

		private static readonly Regex bracketsPattern = new Regex("〈.+〉"); // For station name with brackets

		public static string GetRailDirectionNameJa(string railDirection)
		{
			if (String.IsNullOrWhiteSpace(railDirection))
				return String.Empty;

			var buff = railDirection.Split(new[] { '.', ':' }).Last();

			var station = MetroManager.Current.StationIdTitleMap.FirstOrDefault(x => x.Key.EndsWith(buff));
			if (station.Equals(default(KeyValuePair<string, string>)))
				return buff;

			return bracketsPattern.Replace(station.Value, String.Empty);
		}

		public static string GetRailDirectionNameEn(string railDirection)
		{
			return GetStationTitleEn(railDirection);
		}

		public static string GetRailDirectionDescription(string railDirection, LanguageSubtag languageTag)
		{
			var format = new ResourceLoader().GetString("CaptionDirection");

			return (languageTag == LanguageSubtag.Ja)
				? String.Format(format, GetRailDirectionNameJa(railDirection))
				: String.Format(format, GetRailDirectionNameEn(railDirection));
		}

		#endregion
	}
}