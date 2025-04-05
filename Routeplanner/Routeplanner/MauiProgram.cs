using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Routeplanner.Services;
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
            builder.Services.AddSingleton<PlannerViewModel>();
            builder.Services.AddSingleton<PlannerPage>();
            builder.Services.AddSingleton<DeparturesViewModel>();
            builder.Services.AddSingleton<DeparturesPage>();
            builder.Services.AddSingleton<ITripService, TripService>();
            builder.Services.AddSingleton<APICallService>();
            builder.Services.AddSingleton<SqliteDatabaseService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
