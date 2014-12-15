using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TokyoSubwayView.ViewModels
{
	[DataContract]
	public abstract class RailwayMemberViewModel : ViewModelBase
	{
		[DataMember]
		public double Left
		{
			get { return _left; }
			set
			{
				_left = value;
				RaisePropertyChanged();
			}
		}
		private double _left;

		[DataMember]
		public double Top
		{
			get { return _top; }
			set
			{
				_top = value;
				RaisePropertyChanged();
			}
		}
		private double _top;

		[DataMember]
		public int ZIndex
		{
			get { return _zIndex; }
			set
			{
				_zIndex = value;
				RaisePropertyChanged();
			}
		}
		private int _zIndex = 0;

		public bool IsLoaded { get; set; }
	}
}