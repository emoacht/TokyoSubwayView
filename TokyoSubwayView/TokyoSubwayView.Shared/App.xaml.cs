using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TokyoSubwayView.Common;
using TokyoSubwayView.Views;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace TokyoSubwayView
{
	public sealed partial class App : Application
	{
#if WINDOWS_PHONE_APP
		private TransitionCollection transitions;
#endif

		public App()
		{
			this.InitializeComponent();
			this.Suspending += this.OnSuspending;

			this.UnhandledException += OnUnhandledException;
		}

		private async void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Debug.WriteLine("Unhandled exception happened. {0}", e.Exception);
			e.Handled = true;

			// The following exception can be safely ignored.
			// Message: The parameter is incorrect.
			// Exception type: System.ArgumentException
			// Exception message: "The parameter is incorrect." or "Value does not fall within the expected range."
			// This exception seems to be thrown from a ScrollViewer sometimes when it is manipulated by touch.
			if (e.Exception.HResult == -2147024809)
				return;

			var loader = new ResourceLoader();

			var message = String.Format("{0}\r\n{1}\r\n\r\nHResult:{2}\r\nSource:{3}\r\nStackTrace:{4}\r\nInnerException:{5}\r\nBaseException:{6}",
				e.Message,
				e.Exception,
				e.Exception.HResult,
				e.Exception.Source,
				e.Exception.StackTrace,
				e.Exception.InnerException,
				e.Exception.GetBaseException());

			var dialog = new MessageDialog(message, loader.GetString("UnhandledErrorTitle"));
			await dialog.ShowAsync();

			Application.Current.Exit();
		}

		protected async override void OnLaunched(LaunchActivatedEventArgs e)
		{
#if DEBUG
			if (Debugger.IsAttached)
			{
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif

			var rootFrame = Window.Current.Content as Frame;

			if (rootFrame == null)
			{
				// Create a Frame to act as the navigation context.
				rootFrame = new Frame();

				SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

				rootFrame.CacheSize = 1;

				// Display the extended splash screen if application is not running.
				if (e.PreviousExecutionState != ApplicationExecutionState.Running)
				{
					var loadState = (e.PreviousExecutionState == ApplicationExecutionState.Terminated);
					var extendedSplash = new ExtendedSplash(rootFrame, e.SplashScreen, loadState);
					rootFrame.Content = extendedSplash;

					Window.Current.Content = rootFrame;
					return;
				}

				// Restore the saved session state only when appropriate.
				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
				{
					try
					{
						// Load state from previously suspended application.
						await SuspensionManager.RestoreAsync();
					}
					catch (SuspensionManagerException)
					{
					}
				}

				// Place the frame in the current window.
				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content == null)
			{
#if WINDOWS_PHONE_APP
				if (rootFrame.ContentTransitions != null)
				{
					this.transitions = new TransitionCollection();
					foreach (var c in rootFrame.ContentTransitions)
					{
						this.transitions.Add(c);
					}
				}

				rootFrame.ContentTransitions = null;
				rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

				if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
				{
					throw new Exception("Failed to create initial page");
				}
			}

			// Ensure the current window is active.
			Window.Current.Activate();
		}

#if WINDOWS_PHONE_APP
		private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
		{
			var rootFrame = sender as Frame;
			rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
			rootFrame.Navigated -= this.RootFrame_FirstNavigated;
		}
#endif

		private async void OnSuspending(object sender, SuspendingEventArgs e)
		{
			var deferral = e.SuspendingOperation.GetDeferral();
			await SuspensionManager.SaveAsync();
			deferral.Complete();
		}

#if WINDOWS_APP
		protected override void OnWindowCreated(WindowCreatedEventArgs args)
		{
			base.OnWindowCreated(args);

			SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
		}

		private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
		{
			var loader = new ResourceLoader();

			// Add link to Options.
			args.Request.ApplicationCommands.Add(
				new SettingsCommand(
					Guid.NewGuid(), // If this is string, a FormatException will be thrown internally.
					loader.GetString("LinkToOptions"),
					_ => new OptionsFlyout().Show())); // Not independent

			// Add link to Privacy Policy.
			args.Request.ApplicationCommands.Add(
				new SettingsCommand(
					Guid.NewGuid(),
					loader.GetString("LinkToPrivacyPolicy"),
					async _ => await Launcher.LaunchUriAsync(new Uri(loader.GetString("PrivacyPolicyUrl")))));
		}
#endif
	}
}