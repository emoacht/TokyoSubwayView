using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TokyoSubwayView.Views.Controls
{
	public abstract class RailwayMember : UserControl
	{
		public RailwayMember()
		{
			this.Loaded += (_, __) => IsLoaded = true;
		}


		#region Load

		public bool IsLoaded
		{
			get { return (bool)GetValue(IsLoadedProperty); }
			set { SetValue(IsLoadedProperty, value); }
		}
		public static readonly DependencyProperty IsLoadedProperty =
			DependencyProperty.Register(
				"IsLoaded",
				typeof(bool),
				typeof(RailwayMember),
				new PropertyMetadata(false));

		#endregion
	}
}