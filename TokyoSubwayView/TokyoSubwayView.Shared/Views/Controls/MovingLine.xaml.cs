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
    public sealed partial class MovingLine : UserControl
    {
        public MovingLine()
        {
            this.InitializeComponent();
        }


        public double LineLength
        {
            get { return (double)GetValue(LineLengthProperty); }
            set { SetValue(LineLengthProperty, value); }
        }
        public static readonly DependencyProperty LineLengthProperty =
            DependencyProperty.Register(
                "LineLength",
                typeof(double),
                typeof(MovingLine),
                new PropertyMetadata(400D));

        public double LineWidth
        {
            get { return (double)GetValue(LineWidthProperty); }
            set { SetValue(LineWidthProperty, value); }
        }
        public static readonly DependencyProperty LineWidthProperty =
            DependencyProperty.Register(
                "LineWidth",
                typeof(double),
                typeof(MovingLine),
                new PropertyMetadata(
                    40D,
                    (d, e) =>
                    {
                        var line = ((MovingLine)d);

                        line.LineRoot.Y1 = (double)e.NewValue / 2D;
                        line.LineRoot.StrokeThickness = (double)e.NewValue;
                    }));

        public TrainState State
        {
            get { return (TrainState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register(
                "State",
                typeof(TrainState),
                typeof(MovingLine),
                new PropertyMetadata(TrainState.Vacant, OnStateChanged));

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var line = (MovingLine)d;

            switch ((TrainState)e.NewValue)
            {
                case TrainState.Vacant:
                    VisualStateManager.GoToState(line, "Vacant", true);
                    break;

                case TrainState.OnTime:
                    VisualStateManager.GoToState(line, "OnTime", true);
                    break;

                case TrainState.DelayShort:
                    VisualStateManager.GoToState(line, "DelayShort", true);
                    break;

                case TrainState.DelayLong:
                    VisualStateManager.GoToState(line, "DelayLong", true);
                    break;

                case TrainState.Phantom:
                    VisualStateManager.GoToState(line, "Phantom", true);
                    break;
            }
        }
    }
}