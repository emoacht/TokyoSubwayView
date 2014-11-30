using System;
using System.Collections.Generic;
using System.Text;

namespace TokyoSubwayView.ViewModels
{
	public class RailwayItemViewModel : ViewModelBase
	{
		public List<string> RailwayIds { get; set; } // For Marunouchi and MarunouchiBranch

		public string Description
		{
			get { return _description; }
			set
			{
				_description = value;
				RaisePropertyChanged();
			}
		}
		private string _description;

		public int Order
		{
			get { return _order; }
			set
			{
				_order = value;
				RaisePropertyChanged();
			}
		}
		private int _order;

		public bool IsSelected
		{
			get { return DateTime.MinValue < SelectedDate; }
			set
			{
				SelectedDate = value ? DateTime.Now : DateTime.MinValue;
				RaisePropertyChanged();
			}
		}

		public DateTime SelectedDate { get; private set; }
	}
}