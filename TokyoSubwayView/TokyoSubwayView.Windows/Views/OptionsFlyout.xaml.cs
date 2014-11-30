using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TokyoSubwayView.Models;
using TokyoSubwayView.Models.Metro;
using TokyoSubwayView.ViewModels;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TokyoSubwayView.Views
{
	public sealed partial class OptionsFlyout : SettingsFlyout
	{
		private readonly OptionsFlyoutViewModel optionsFlyoutViewModel;

		public OptionsFlyout()
		{
			this.InitializeComponent();

			optionsFlyoutViewModel = (OptionsFlyoutViewModel)this.DataContext;

			this.SetBinding(
				RailwayIdsProperty,
				new Binding
				{
					Source = MetroManager.Current,
					Path = new PropertyPath("RailwayIds"),
					Mode = BindingMode.OneWay,
				});

			this.SetBinding(
				IsInitiatingProperty,
				new Binding
				{
					Source = Settings.Current,
					Path = new PropertyPath("IsInitiating"),
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
		}


		#region Railways

		public List<string> RailwayIds
		{
			get { return (List<string>)GetValue(RailwayIdsProperty); }
			set { SetValue(RailwayIdsProperty, value); }
		}
		public static readonly DependencyProperty RailwayIdsProperty =
			DependencyProperty.Register(
				"RailwayIds",
				typeof(List<string>),
				typeof(OptionsFlyout),
				new PropertyMetadata(
					null,
					(d, e) =>
					{
						var flyout = (OptionsFlyout)d;

						flyout.optionsFlyoutViewModel.PopulateItems();
						flyout.optionsFlyoutViewModel.SetItemDescriptions();
					}));

		public bool IsInitiating
		{
			get { return (bool)GetValue(IsInitiatingProperty); }
			set { SetValue(IsInitiatingProperty, value); }
		}
		public static readonly DependencyProperty IsInitiatingProperty =
			DependencyProperty.Register(
				"IsInitiating",
				typeof(bool),
				typeof(OptionsFlyout),
				new PropertyMetadata(
					false,
					(d, e) => ((OptionsFlyout)d).optionsFlyoutViewModel.RaiseCanExecuteChanged()));

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
				typeof(OptionsFlyout),
				new PropertyMetadata(
					default(LanguageSubtag),
					(d, e) =>
					{
						var flyout = (OptionsFlyout)d;

						flyout.optionsFlyoutViewModel.SetItemDescriptions();
						flyout.LoadLanguage((LanguageSubtag)e.NewValue);
					}));

		private void LoadLanguage(LanguageSubtag languageTag)
		{
			const string optionsHeader = "OptionsHeaderText";
			const string lineSelectionPriority = "LineSelectionPriorityText";
			const string languageSelection = "LanguageSelectionText";
			const string clearButton = "ClearButtonText";
			const string applyButton = "ApplyButtonText";

			var languageBag = Settings.LanguageList.FirstOrDefault(x => x.Subtag == languageTag);
			if (languageBag != null)
			{
				var context = ResourceContext.GetForViewIndependentUse(); // Constructing ResourceContext may cause freeze when called multiple times.
				context.Languages = new List<String> { languageBag.FullTag };

				var resourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

				this.Title = resourceMap.GetValue(optionsHeader, context).ValueAsString;
				this.LineSelectionPriority.Text = resourceMap.GetValue(lineSelectionPriority, context).ValueAsString;
				this.LanguageSelection.Text = resourceMap.GetValue(languageSelection, context).ValueAsString;
				this.ClearButton.Content = resourceMap.GetValue(clearButton, context).ValueAsString;
				this.ApplyButton.Content = resourceMap.GetValue(applyButton, context).ValueAsString;
			}
			else // Fallback
			{
				var loader = ResourceLoader.GetForCurrentView();

				this.Title = loader.GetString(optionsHeader);
				this.LineSelectionPriority.Text = loader.GetString(lineSelectionPriority);
				this.LanguageSelection.Text = loader.GetString(languageSelection);
				this.ClearButton.Content = loader.GetString(clearButton);
				this.ApplyButton.Content = loader.GetString(applyButton);
			}
		}

		#endregion
	}
}