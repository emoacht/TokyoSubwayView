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
	public sealed partial class RailwayBadge : UserControl
	{
		public RailwayBadge()
		{
			this.InitializeComponent();
		}


		public double Diameter
		{
			get { return (double)GetValue(DiameterProperty); }
			set { SetValue(DiameterProperty, value); }
		}
		public static readonly DependencyProperty DiameterProperty =
			DependencyProperty.Register(
				"Diameter",
				typeof(double),
				typeof(RailwayBadge),
				new PropertyMetadata(
					40D, // Default
					(d, e) =>
					{
						var badge = (RailwayBadge)d;
						var factor = (double)e.NewValue / 40D;

						if (factor != 1D)
						{
							badge.DiameterTransform.ScaleX = factor;
							badge.DiameterTransform.ScaleY = factor;
						}
					}));

		public string RailwayId
		{
			get { return (string)GetValue(RailwayIdProperty); }
			set { SetValue(RailwayIdProperty, value); }
		}
		public static readonly DependencyProperty RailwayIdProperty =
			DependencyProperty.Register(
				"RailwayId",
				typeof(string),
				typeof(RailwayBadge),
				new PropertyMetadata(
					String.Empty,
					(d, e) =>
					{
						var badge = (RailwayBadge)d;
						var railwayId = (string)e.NewValue;

						if ((MetroManager.Current.RailwayIds != null) && MetroManager.Current.RailwayIds.Contains(railwayId))
						{
							badge.Circle.Stroke = MetroManager.Current.RailwayIdBrushMap[railwayId];
							badge.LineCode.Text = MetroManager.Current.RailwayIdCodeMap[railwayId];
						}
					}));
	}
}