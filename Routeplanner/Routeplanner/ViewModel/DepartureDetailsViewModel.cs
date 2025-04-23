using CommunityToolkit.Mvvm.ComponentModel;
using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Routeplanner.ViewModel
{
    public class DepartureDetailsViewModel : ObservableObject
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
