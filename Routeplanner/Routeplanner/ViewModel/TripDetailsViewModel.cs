using CommunityToolkit.Mvvm.ComponentModel;
using Routeplanner.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Routeplanner.ViewModel
{
    public class TripDetailsViewModel : ObservableObject
    {
        private Trip _trip;

        public Trip Trip
        {
            get => _trip;
            set => SetProperty(ref _trip, value);
        }

        public TripDetailsViewModel(Trip trip)
        {
            Trip = trip;
        }
    }
}
