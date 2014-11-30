using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TokyoSubwayView.Common;
using TokyoSubwayView.Models.Metro;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TokyoSubwayView.Views
{
	public sealed partial class ExtendedSplash : Page
	{
		private readonly Frame rootFrame; // To hold root Frame
		private readonly SplashScreen systemSplashScreen; // To hold the system splash screen

		private bool isActivated;

		public ExtendedSplash(Frame rootFrame, SplashScreen systemSplashscreen, bool loadState)
		{
			InitializeComponent();

			// Listen for image opened event to wait for the extended splash image being read and painted 
			// so as to prevent the image from blinking at the transition from the system splash screen 
			// to the extended splash screen.
			SplashImage.ImageOpened += OnImageOpened;

			// Listen for window resize events to reposition the extended splash image accordingly.
			Window.Current.SizeChanged += OnSizeChanged;

			this.rootFrame = rootFrame;
			this.systemSplashScreen = systemSplashscreen;

			PositionImageProgressRing();

			RestoreStateAsync(loadState);
		}
		
		private async void OnImageOpened(object sender, RoutedEventArgs e)
		{
			// ImageOpened means the file has been read, but the image hasn't been painted yet.
			// Short interval will give the extended splash image a chance to render, before showing 
			// the extended splash screen and starting the animation.
			await Task.Delay(TimeSpan.FromMilliseconds(50));

			// Activate the extended splash screen.
			Window.Current.Activate();
			isActivated = true;
		}

		private void OnSizeChanged(Object sender, WindowSizeChangedEventArgs e)
		{
			PositionImageProgressRing();
		}

		private void PositionImageProgressRing()
		{
			if (systemSplashScreen == null)
				return;

			var splashImageRect = systemSplashScreen.ImageLocation;

			PositionImage(splashImageRect);
			PositionRing(splashImageRect);
		}

		private void PositionImage(Rect splashImageRect)
		{
			// Position the extended splash image in the same location as the system splash image.
			SplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
			SplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
			SplashImage.Height = splashImageRect.Height;
			SplashImage.Width = splashImageRect.Width;
		}

		private void PositionRing(Rect splashImageRect)
		{
			SplashProgressRing.SetValue(Canvas.LeftProperty, splashImageRect.X + (splashImageRect.Width * 0.5) - (SplashProgressRing.Width * 0.5));
			SplashProgressRing.SetValue(Canvas.TopProperty, (splashImageRect.Y + splashImageRect.Height + splashImageRect.Height * 0.1));
		}

		private async void RestoreStateAsync(bool loadState)
		{
			if (loadState)
			{
				try
				{
					await SuspensionManager.RestoreAsync();
				}
				catch (SuspensionManagerException)
				{
				}
			}

			await MetroManager.Current.GetRailwaysStationsTrainsCachedAsync();

			// In case restoring process ends in very short time, wait for the extended splash screen 
			// being activated/shown. Otherwise, the transition to MainPage may not happen.
			while (true)
			{
				if (isActivated)
					break;

				await Task.Delay(TimeSpan.FromMilliseconds(50));
			}

			DismissExtendedSplash();
		}

		private void DismissExtendedSplash()
		{
			SplashImage.ImageOpened -= OnImageOpened;
			Window.Current.SizeChanged -= OnSizeChanged;

			// Navigate to MainPage.
			if (!rootFrame.Navigate(typeof(MainPage)))
			{
				Debug.WriteLine("Failed to navigate to MainPage.");
			}

			Window.Current.Content = rootFrame;
		}
	}
}