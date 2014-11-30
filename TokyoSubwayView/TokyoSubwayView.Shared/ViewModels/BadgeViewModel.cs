using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TokyoSubwayView.ViewModels
{
    [DataContract]
    public class BadgeViewModel : ViewModelBase
    {
        [DataMember]
        public string StationCode { get; set; }

        [DataMember]
        public string RailwayId { get; set; }
    }
}