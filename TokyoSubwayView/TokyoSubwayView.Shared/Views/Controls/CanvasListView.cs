using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace TokyoSubwayView.Views.Controls
{
    public class CanvasListView : ListView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var contentControl = (ListViewItem)element;

            contentControl.SetBinding(
                Canvas.LeftProperty,
                new Binding { Path = new PropertyPath("Left"), Mode = BindingMode.OneWay });

            contentControl.SetBinding(
                Canvas.TopProperty,
                new Binding { Path = new PropertyPath("Top"), Mode = BindingMode.OneWay });

            contentControl.SetBinding(
                Canvas.ZIndexProperty,
                new Binding { Path = new PropertyPath("ZIndex"), Mode = BindingMode.OneTime }); // OneTime only

            base.PrepareContainerForItemOverride(element, item);
        }
    }
}