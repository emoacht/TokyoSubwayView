using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TokyoSubwayView.Models.Metro;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TokyoSubwayView.Views.Controls
{
	public sealed partial class StationGroupMember : RailwayMember
	{
		public StationGroupMember()
		{
			this.InitializeComponent();
		}


		#region Geometry

		public double Diameter
		{
			get { return (double)GetValue(DiameterProperty); }
			set { SetValue(DiameterProperty, value); }
		}
		public static readonly DependencyProperty DiameterProperty =
			DependencyProperty.Register(
				"Diameter",
				typeof(double),
				typeof(StationGroupMember),
				new PropertyMetadata(0D));

		#endregion


		#region State

		public TrainState State
		{
			get { return (TrainState)GetValue(StateProperty); }
			set { SetValue(StateProperty, value); }
		}
		public static readonly DependencyProperty StateProperty =
			DependencyProperty.Register(
				"State",
				typeof(TrainState),
				typeof(StationGroupMember),
				new PropertyMetadata(
					default(TrainState),
					(d, e) => ((StationGroupMember)d).OnStateChanged()));

		private void OnStateChanged()
		{
			switch (State)
			{
				case TrainState.Vacant:
					VisualStateManager.GoToState(this, "Vacant", true);
					break;

				case TrainState.OnTime:
					VisualStateManager.GoToState(this, "OnTime", true);
					break;

				case TrainState.DelayShort:
					VisualStateManager.GoToState(this, "DelayShort", true);
					break;

				case TrainState.DelayLong:
					VisualStateManager.GoToState(this, "DelayLong", true);
					break;
			}
		}

		#endregion
	}
}