using System;
using System.Collections.Generic;
using System.Text;
using TokyoSubwayView.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TokyoSubwayView.Views.Controls
{
    class RailwayMemberTemplateSelector : DataTemplateSelector
    {
        public DataTemplate StationGroupTemplate { get; set; }
        public DataTemplate ConnectorTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var viewModel = item as RailwayMemberViewModel;

            return (viewModel is StationGroupViewModel)
                ? StationGroupTemplate
                : ConnectorTemplate;
        }
    }
}