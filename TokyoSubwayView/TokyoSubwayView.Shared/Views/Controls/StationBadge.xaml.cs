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
    public sealed partial class StationBadge : UserControl
    {
        public StationBadge()
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
                typeof(StationBadge),
                new PropertyMetadata(
                    60D,
                    (d, e) =>
                    {
                        var badge = (StationBadge)d;
                        var factor = (double)e.NewValue / 40D;

                        if (factor != 1D)
                        {
                            badge.DiameterTransform.ScaleX = factor;
                            badge.DiameterTransform.ScaleY = factor;
                        }
                    }));

        public string StationCode
        {
            get { return (string)GetValue(StationCodeProperty); }
            set { SetValue(StationCodeProperty, value); }
        }
        public static readonly DependencyProperty StationCodeProperty =
            DependencyProperty.Register(
                "StationCode",
                typeof(string),
                typeof(StationBadge),
                new PropertyMetadata(
                    "M01", // Sample
                    (d, e) =>
                    {
                        var badge = (StationBadge)d;
                        var code = (string)e.NewValue;

                        if (3 <= code.Length)
                        {
                            badge.LineCode.Text = code.Substring(0, 1);
                            badge.StationNumber.Text = code.Substring(1, 2);
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
                typeof(StationBadge),
                new PropertyMetadata(
                    String.Empty,
                    (d, e) =>
                    {
                        var badge = (StationBadge)d;
                        var railwayId = (string)e.NewValue;

                        if ((MetroManager.Current.RailwayIds != null) && MetroManager.Current.RailwayIds.Contains(railwayId))
                        {
                            badge.Circle.Stroke = MetroManager.Current.RailwayIdBrushMap[railwayId];
                        }
                    }));
    }
}