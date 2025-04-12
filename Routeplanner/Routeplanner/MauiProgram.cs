using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Routeplanner.Services;
using Routeplanner.Services.API;
using Routeplanner.Services.Database;
using Routeplanner.Services.Departures;
using Routeplanner.Services.Planner;
using Routeplanner.Services.Repositories;
using Routeplanner.ViewModel;

namespace Routeplanner
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.UseMauiCommunityToolkit();

            // Pages
            builder.Services.AddSingleton<PlannerPage>();
            builder.Services.AddSingleton<DeparturesPage>();
            builder.Services.AddSingleton<SavedPage>();

            // ViewModels
            builder.Services.AddSingleton<PlannerViewModel>();
            builder.Services.AddSingleton<DeparturesViewModel>();
            builder.Services.AddSingleton<SavedViewModel>();

            // Services
            builder.Services.AddSingleton<ITripService, TripService>();
            builder.Services.AddSingleton<IDepartureService, DepartureService>();

            // Database / Tables
            builder.Services.AddSingleton<SQLiteDatabaseService>();
            builder.Services.AddSingleton<StationTable>();
            builder.Services.AddSingleton<RouteCacheTable>();
            builder.Services.AddSingleton<SavedTripsTable>();
            builder.Services.AddSingleton<SavedDeparturesTable>();

            // API Call Services
            builder.Services.AddTransient<TripsAPICallService>();
            builder.Services.AddTransient<DeparturesAPICallService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
