using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using TokyoSubwayView.Common;
using TokyoSubwayView.Models;
using TokyoSubwayView.Models.Metro;
using TokyoSubwayView.ViewModels;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TokyoSubwayView.Views
{
	public sealed partial class MainPage : Page
	{
		public NavigationHelper NavigationHelper
		{
			get { return this.navigationHelper; }
		}
		private NavigationHelper navigationHelper;

		private readonly MainPageViewModel mainPageViewModel;

		public MainPage()
		{
			this.InitializeComponent();

			mainPageViewModel = (MainPageViewModel)this.DataContext;

			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += navigationHelper_LoadState;
			this.navigationHelper.SaveState += navigationHelper_SaveState;

			this.Loaded += OnLoaded;
			this.SizeChanged += OnSizeChanged;

			this.SetBinding(
				RailwayIdPriorityProperty,
				new Binding
				{
					Source = Settings.Current,
					Path = new PropertyPath("RailwayIdPriority"),
					Mode = BindingMode.OneWay,
				});

			this.SetBinding(
				LanguageTagProperty,
				new Binding
				{
					Source = Settings.Current,
					Path = new PropertyPath("LanguageTag"),
					Mode = BindingMode.OneWay,
				});

			this.SetBinding(
				ErrorMessageProperty,
				new Binding
				{
					Source = MetroManager.Current,
					Path = new PropertyPath("ErrorMessage"),
					Mode = BindingMode.OneWay,
				});
		}

		private async void OnLoaded(object sender, RoutedEventArgs e)
		{
			Debug.WriteLine("OnLoaded MainViewer Size => {0}-{1}", MainViewer.ActualWidth, MainViewer.ActualHeight);

			if (TopAppBar != null)
				TopAppBar.IsOpen = true;

			await mainPageViewModel.InitiateAsync(MainViewer);

			MainViewer.Opacity = 1;

			mainPageViewModel.StartTimer();
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			Debug.WriteLine("OnSizeChanged Page Size => {0}-{1} ({2})", e.NewSize.Width, e.NewSize.Height, DisplayInformation.GetForCurrentView().CurrentOrientation);
			Debug.WriteLine("OnSizeChanged MainViewer Size => {0}-{1}", MainViewer.ActualWidth, MainViewer.ActualHeight);

			var isPageLandscape = e.NewSize.Width > e.NewSize.Height; // This page's orientation may not match display orientation.

			mainPageViewModel.HasEnoughWidth = isPageLandscape;
		}


		#region AppBar, Flyout

		private void PageHeader_Tapped(object sender, TappedRoutedEventArgs e)
		{
			e.Handled = true;

			if (TopAppBar != null)
				TopAppBar.IsOpen = true;
		}

		private void CanvasItem_Tapped(object sender, TappedRoutedEventArgs e)
		{
			try
			{
				e.Handled = true;

				var item = sender as FrameworkElement;
				if (item != null)
					FlyoutBase.ShowAttachedFlyout(item);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to handle canvas item tapped event and show flyout. {0}", ex);
				throw new Exception("Failed to handle canvas item tapped event and show flyout.", ex);
			}
		}

		#endregion


		#region Priority

		public string[] RailwayIdPriority
		{
			get { return (string[])GetValue(RailwayIdPriorityProperty); }
			set { SetValue(RailwayIdPriorityProperty, value); }
		}
		public static readonly DependencyProperty RailwayIdPriorityProperty =
			DependencyProperty.Register(
				"RailwayIdPriority",
				typeof(string[]),
				typeof(MainPage),
				new PropertyMetadata(
					null,
					async (d, e) => await ((MainPage)d).mainPageViewModel.UpdatePriorityAsync()));

		#endregion


		#region Language

		public LanguageSubtag LanguageTag
		{
			get { return (LanguageSubtag)GetValue(LanguageTagProperty); }
			set { SetValue(LanguageTagProperty, value); }
		}
		public static readonly DependencyProperty LanguageTagProperty =
			DependencyProperty.Register(
				"LanguageTag",
				typeof(LanguageSubtag),
				typeof(MainPage),
				new PropertyMetadata(
					default(LanguageSubtag),
					async (d, e) => await ((MainPage)d).mainPageViewModel.UpdateLanguageAsync()));

		#endregion


		#region Message

		public string ErrorMessage
		{
			get { return (string)GetValue(ErrorMessageProperty); }
			set { SetValue(ErrorMessageProperty, value); }
		}
		public static readonly DependencyProperty ErrorMessageProperty =
			DependencyProperty.Register(
				"ErrorMessage",
				typeof(string),
				typeof(MainPage),
				new PropertyMetadata(
					String.Empty,
					async (d, e) => await ((MainPage)d).ShowErrorMessage()));

		private async Task ShowErrorMessage()
		{
			if (String.IsNullOrEmpty(ErrorMessage))
				return;

			try
			{
				var loader = new ResourceLoader();

				var dialog = new MessageDialog(ErrorMessage, loader.GetString("ErrorTitle"));
				await dialog.ShowAsync();
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to show error message. {0}", ex);
			}
		}

		#endregion


		#region NavigationHelper

		private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
		}

		private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			navigationHelper.OnNavigatedFrom(e);
		}

		#endregion
	}
}