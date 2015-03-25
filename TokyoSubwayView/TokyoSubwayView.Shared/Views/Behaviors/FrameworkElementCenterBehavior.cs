using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;

namespace TokyoSubwayView.Views.Behaviors
{
	public class FrameworkElementCenterBehavior : DependencyObject, IBehavior
	{
		public DependencyObject AssociatedObject { get; private set; }

		private FrameworkElement AssociatedFrameworkElement
		{
			get { return (FrameworkElement)this.AssociatedObject; }
		}

		public void Attach(DependencyObject associatedObject)
		{
			this.AssociatedObject = associatedObject as FrameworkElement;

			if (this.AssociatedObject == null)
			{
				Debug.WriteLine("Associated object is not FrameworkElement!");
				return;
			}

			AssociatedFrameworkElement.SizeChanged += OnSizeChanged;
		}

		public void Detach()
		{
			if (this.AssociatedObject == null)
				return;

			AssociatedFrameworkElement.SizeChanged -= OnSizeChanged;
		}


		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			var margin = new Thickness(Math.Round(e.NewSize.Width / -2), Math.Round(e.NewSize.Height / -2), 0, 0); // Fractional portion may cause layout loop.

			AssociatedFrameworkElement.Margin = margin;
		}
	}
}