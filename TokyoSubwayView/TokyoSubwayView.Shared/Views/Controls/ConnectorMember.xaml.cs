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
    public sealed partial class ConnectorMember : UserControl
    {
        public ConnectorMember()
        {
            this.InitializeComponent();

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
                typeof(ConnectorMember),
                new PropertyMetadata(false));

        #endregion


        #region Geometry

        public double X1
        {
            get { return (double)GetValue(X1Property); }
            set { SetValue(X1Property, value); }
        }
        public static readonly DependencyProperty X1Property =
            DependencyProperty.Register(
                "X1",
                typeof(double),
                typeof(ConnectorMember),
                new PropertyMetadata(0D, OnCoordinateChanged));

        public double X2
        {
            get { return (double)GetValue(X2Property); }
            set { SetValue(X2Property, value); }
        }
        public static readonly DependencyProperty X2Property =
            DependencyProperty.Register(
                "X2",
                typeof(double),
                typeof(ConnectorMember),
                new PropertyMetadata(0D, OnCoordinateChanged));

        public double Y1
        {
            get { return (double)GetValue(Y1Property); }
            set { SetValue(Y1Property, value); }
        }
        public static readonly DependencyProperty Y1Property =
            DependencyProperty.Register(
                "Y1",
                typeof(double),
                typeof(ConnectorMember),
                new PropertyMetadata(0D, OnCoordinateChanged));

        public double Y2
        {
            get { return (double)GetValue(Y2Property); }
            set { SetValue(Y2Property, value); }
        }
        public static readonly DependencyProperty Y2Property =
            DependencyProperty.Register(
                "Y2",
                typeof(double),
                typeof(ConnectorMember),
                new PropertyMetadata(0D, OnCoordinateChanged));

        public double LineLength
        {
            get { return (double)GetValue(LineLengthProperty); }
            set { SetValue(LineLengthProperty, value); }
        }
        public static readonly DependencyProperty LineLengthProperty =
            DependencyProperty.Register(
                "LineLength",
                typeof(double),
                typeof(ConnectorMember),
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
                typeof(ConnectorMember),
                new PropertyMetadata(40D));

        public double LineAngle
        {
            get { return (double)GetValue(LineAngleProperty); }
            set { SetValue(LineAngleProperty, value); }
        }
        public static readonly DependencyProperty LineAngleProperty =
            DependencyProperty.Register(
                "LineAngle",
                typeof(double),
                typeof(ConnectorMember),
                new PropertyMetadata(30D));

        private static void OnCoordinateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var lines = (ConnectorMember)d;

            if ((0 < lines.X1) && (0 < lines.X2) && (0 < lines.Y1) && (0 < lines.Y2)) // TODO: Conider later
            {
                var lengthX = lines.X2 - lines.X1;
                var lengthY = lines.Y2 - lines.Y1;

                lines.LineLength = CalcLength(lengthX, lengthY);
                lines.LineAngle = CalcAngle(lengthX, lengthY);
            }
        }

        private static double CalcLength(double lengthX, double lengthY)
        {
            return Math.Sqrt(Math.Pow(lengthX, 2D) + Math.Pow(lengthY, 2D));
        }

        private static double CalcAngle(double lengthX, double lengthY)
        {
            var angle = Math.Atan(lengthY / lengthX) * 180D / Math.PI;

            if (lengthX < 0)
                return 180D + angle;

            if (lengthY < 0)
                return 360D + angle;

            return angle;
        }

        #endregion


        #region State

        public TrainState UpperState
        {
            get { return (TrainState)GetValue(UpperStateProperty); }
            set { SetValue(UpperStateProperty, value); }
        }
        public static readonly DependencyProperty UpperStateProperty =
            DependencyProperty.Register(
                "UpperState",
                typeof(TrainState),
                typeof(ConnectorMember),
                new PropertyMetadata(TrainState.Vacant));

        public TrainState LowerState
        {
            get { return (TrainState)GetValue(LowerStateProperty); }
            set { SetValue(LowerStateProperty, value); }
        }
        public static readonly DependencyProperty LowerStateProperty =
            DependencyProperty.Register(
                "LowerState",
                typeof(TrainState),
                typeof(ConnectorMember),
                new PropertyMetadata(TrainState.Vacant));

        #endregion
    }
}