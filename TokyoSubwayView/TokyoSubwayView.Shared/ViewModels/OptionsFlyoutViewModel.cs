using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokyoSubwayView.Common;
using TokyoSubwayView.Models;
using TokyoSubwayView.Models.Metro;
using Windows.ApplicationModel.Resources.Core;
using Windows.Globalization;

namespace TokyoSubwayView.ViewModels
{
	public class OptionsFlyoutViewModel : ViewModelBase
	{
		#region Railway

		public ObservableCollection<RailwayItemViewModel> SourceItems
		{
			get { return _sourceItems ?? (_sourceItems = new ObservableCollection<RailwayItemViewModel>()); }
		}
		private ObservableCollection<RailwayItemViewModel> _sourceItems;

		public int[] SelectedIndices
		{
			get { return _selectedIndices; }
			set
			{
				if (_selectedIndices == value)
					return;

				_selectedIndices = value;
				RaisePropertyChanged();
				ChangeSelectedItems();
			}
		}
		private int[] _selectedIndices;

		public void PopulateItems()
		{
			if ((MetroManager.Current.RailwayIds == null) || !MetroManager.Current.RailwayIds.Any())
				return;

			foreach (var railwayId in MetroManager.Current.RailwayIds)
			{
				var existingItem = SourceItems
					.FirstOrDefault(x => x.RailwayIds
						.Select(y => y.Replace("Branch", String.Empty))
						.Contains(railwayId.Replace("Branch", String.Empty))); // For MarunouchiBranch

				if (existingItem != null)
				{
					existingItem.RailwayIds.Add(railwayId);
				}
				else
				{
					var newItem = new RailwayItemViewModel { RailwayIds = new List<string> { railwayId } };
					SourceItems.Add(newItem);
				}
			}

			if ((Settings.Current.RailwayIdPriority == null) || !Settings.Current.RailwayIdPriority.Any())
			{
				SelectedIndices = Enumerable.Range(0, SourceItems.Count).ToArray();
			}
			else
			{
				var indices = new List<int>();

				int order = 0;
				foreach (var railwayId in Settings.Current.RailwayIdPriority)
				{
					int index = 0;
					foreach (var item in SourceItems)
					{
						if (item.RailwayIds[0] == railwayId)
						{
							item.IsSelected = true;
							item.Order = order;
							indices.Add(index);
							break;
						}
						index++;
					}
					order++;
				}

				SelectedIndices = indices.ToArray();
			}
		}

		public void SetItemDescriptions()
		{
			if (SourceItems == null)
				return;

			foreach (var item in SourceItems)
			{
				item.Description = MetroHelper.GetRailwayDescription(item.RailwayIds[0], Settings.Current.LanguageTag);
			}
		}

		private void ClearItems()
		{
			if (SourceItems == null)
				return;

			foreach (var item in SourceItems)
			{
				item.Order = 0;
				item.IsSelected = false;
			}

			SelectedIndices = null;
		}

		private void ChangeSelectedItems()
		{
			int index = 0;
			foreach (var item in SourceItems)
			{
				if ((SelectedIndices != null) && SelectedIndices.Contains(index))
				{
					if (!item.IsSelected)
						item.IsSelected = true;
				}
				else
				{
					item.IsSelected = false;
					item.Order = 0;
				}
				index++;
			}

			var sortedItems = SourceItems
				.Where(x => x.IsSelected)
				.OrderBy(x => x.SelectedDate)
				.ThenBy(x => x.Order)
				.ToArray();

			int order = 0;
			foreach (var item in sortedItems)
			{
				var matchingItem = SourceItems.FirstOrDefault(x => x.RailwayIds[0] == item.RailwayIds[0]);
				if (matchingItem != null)
					matchingItem.Order = ++order;
			}

			RaiseCanExecuteChanged();
		}

		private void ApplySelectedItems()
		{
			if (!SourceItems.Any(x => x.IsSelected))
				return;

			Settings.Current.RailwayIdPriority = SourceItems
				.Where(x => x.IsSelected)
				.OrderBy(x => x.Order)
				.SelectMany(x => x.RailwayIds)
				.ToArray();
		}


		#region Command

		#region Clear Command

		public RelayCommand ClearCommand
		{
			get { return _clearCommand ?? (_clearCommand = new RelayCommand(ClearExecute, CanClearExecute)); }
		}
		private RelayCommand _clearCommand;

		public void ClearExecute()
		{
			ClearItems();
		}

		public bool CanClearExecute()
		{
			return SourceItems.Any(x => x.IsSelected) && !Settings.Current.IsInitiating;
		}

		#endregion

		#region Apply Command

		public RelayCommand ApplyCommand
		{
			get { return _applyCommand ?? (_applyCommand = new RelayCommand(ApplyExecute, CanApplyExecute)); }
		}
		private RelayCommand _applyCommand;

		public void ApplyExecute()
		{
			ApplySelectedItems();
		}

		public bool CanApplyExecute()
		{
			return SourceItems.Any(x => x.IsSelected) && !Settings.Current.IsInitiating;
		}

		#endregion

		public void RaiseCanExecuteChanged()
		{
			ClearCommand.RaiseCanExecuteChanged();
			ApplyCommand.RaiseCanExecuteChanged();
		}

		#endregion

		#endregion


		#region Language

		public string[] LanguageNames
		{
			get { return Settings.LanguageList.Select(x => x.DisplayName).ToArray(); }
		}

		public int LanguageIndex
		{
			get
			{
				if (_languageIndex == -1)
					_languageIndex = CheckLanguage();

				return _languageIndex;
			}
			set
			{
				if (_languageIndex == value)
					return;

				_languageIndex = value;
				RaisePropertyChanged();
				ApplyLanguage(value);
			}
		}
		private int _languageIndex = -1; // Not Selected

		private int CheckLanguage()
		{
			return Settings.LanguageList.ToList().FindIndex(x => x.Subtag == Settings.Current.LanguageTag);
		}

		private void ApplyLanguage(int index)
		{
			var languageBag = Settings.LanguageList[index];

			ApplicationLanguages.PrimaryLanguageOverride = languageBag.FullTag;
			ResourceContext.GetForCurrentView().Reset();

			Settings.Current.LanguageTag = languageBag.Subtag;
		}

		#endregion
	}
}