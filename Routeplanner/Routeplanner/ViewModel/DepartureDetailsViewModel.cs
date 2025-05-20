using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.ViewModel
{
    public partial class DepartureDetailsViewModel : ObservableObject
    {
        private Departure _departure;

        public Departure Departure
        {
            get => _departure;
            set => SetProperty(ref _departure, value);
        }

        public DepartureDetailsViewModel(Departure departure)
        {
            Departure = departure;
        }
    }
}
