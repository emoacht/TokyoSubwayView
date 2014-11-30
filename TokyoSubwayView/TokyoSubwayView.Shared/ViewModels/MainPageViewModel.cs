using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokyoSubwayView.Common;
using TokyoSubwayView.Models;
using TokyoSubwayView.Models.Metro;
using TokyoSubwayView.Views;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace TokyoSubwayView.ViewModels
{
	public class MainPageViewModel : ViewModelBase
	{
		public ObservableCollection<RailwayMemberViewModel> Core
		{
			get { return _core ?? (_core = new ObservableCollection<RailwayMemberViewModel>()); }
		}
		private ObservableCollection<RailwayMemberViewModel> _core;

		public Operation Op { get; private set; }


		public MainPageViewModel()
		{
			Op = new Operation(Core);
		}


		#region Operation

		private static readonly TimeSpan checkIntervalLength = TimeSpan.FromMilliseconds(50);

		internal async Task InitiateAsync(FrameworkElement mainViewer)
		{
			try
			{
				IsMainViewerInitiating = true;

				await Op.InitiateBaseAsync(mainViewer);

				RaisePropertyChanged(() => MainViewerZoomFactor);
				RaisePropertyChanged(() => MainCanvasCenterPosition);

				// Wait for all railway members being loaded. Otherwise, subsequent restoring position 
				// may not work correctly.
				while (true)
				{
					if (Core.All(x => x.IsLoaded))
						break;

					await Task.Delay(checkIntervalLength);
				}
			}
			finally
			{
				IsMainViewerInitiating = false;
			}
		}

		internal void StartTimer()
		{
			Op.StartTimerBase();
		}

		internal async Task UpdatePriorityAsync()
		{
			await InitiateAsync(null);
			await Op.UpdateBaseAsync(Settings.Current.LanguageTag);
		}

		internal async Task UpdateLanguageAsync()
		{
			await Op.UpdateBaseAsync(Settings.Current.LanguageTag);
		}

		#endregion


		#region Page

		public bool HasEnoughWidth // AppBar's elements cannot be binded to Page's properties.
		{
			get { return _hasEnoughWidth; }
			set
			{
				_hasEnoughWidth = value;
				RaisePropertyChanged();
			}
		}
		private bool _hasEnoughWidth = true;

		#endregion


		#region Viewer

		public float MainViewerZoomFactor
		{
			get { return Op.ConvertZoomFactorFromReal(Settings.Current.RealZoomFactor); }
			set
			{
				Settings.Current.RealZoomFactor = Op.ConvertZoomFactorToReal(value);
				RaisePropertyChanged();
			}
		}

		public Point MainCanvasCenterPosition
		{
			get { return Op.ConvertPositionFromReal(Settings.Current.RealCenterPosition); }
			set
			{
				Settings.Current.RealCenterPosition = Op.ConvertPositionToReal(value);
				RaisePropertyChanged();
			}
		}

		public bool IsMainViewerInitiating
		{
			get { return Settings.Current.IsInitiating; }
			set
			{
				Settings.Current.IsInitiating = value;
				RaisePropertyChanged();
			}
		}

		public ZoomDirectionMode ZoomDirection // AppBar's elements cannot be binded to Page's properties.
		{
			get { return _zoomDirection; }
			set
			{
				_zoomDirection = value;
				RaisePropertyChanged();
			}
		}
		private ZoomDirectionMode _zoomDirection;

		#endregion


		#region Options

		public RelayCommand ShowOptionsCommand
		{
			get { return _showOptionsCommand ?? (_showOptionsCommand = new RelayCommand(ShowOptionsExecute, () => true)); }
		}
		private RelayCommand _showOptionsCommand;

		public void ShowOptionsExecute()
		{
			new OptionsFlyout().ShowIndependent(); // Independent
		}

		#endregion
	}
}