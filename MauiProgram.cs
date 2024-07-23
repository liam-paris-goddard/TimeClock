using Microsoft.Extensions.Logging;
using Goddard.Clock.Helpers;
using Goddard.Clock.Data;
using Goddard.Clock.Factories;
using Goddard.Clock.Controls;
using Goddard.Clock.Handlers;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Reflection;

namespace Goddard.Clock
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("Goddard.Clock.appsettings.json");

            if (stream == null)
            {
                throw new Exception("appsettings.json not found");
            }
            else
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();

                builder.Configuration.AddConfiguration(config);
            }

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .RegisterServices()
                .RegisterViewModels()
                .ConfigureMauiHandlers(handlers =>
                {
                    handlers.AddHandler(typeof(GoddardButton), typeof(GoddardButtonHandler));
                    handlers.AddHandler(typeof(GoddardImageButton), typeof(GoddardImageButtonHandler));
                    handlers.AddHandler(typeof(GoddardFrame), typeof(GoddardFrameHandler));
                });

            var connectionString = builder.Configuration["ApplicationInsightsConnectionString"];
            TelemetryConfiguration telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.ConnectionString = connectionString;

            TelemetryClient telemetryClient = new TelemetryClient(telemetryConfiguration);
            builder.Services.AddSingleton(telemetryClient);

            builder.Logging.AddApplicationInsights(
                config => config.ConnectionString = connectionString,
                options => options.IncludeScopes = true
            );

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        public static MauiAppBuilder RegisterViewModels(this MauiAppBuilder mauiAppBuilder)
        {
            _ = mauiAppBuilder.Services.AddTransient<ViewModels.AdminLoginPageViewModel>();
            _ = mauiAppBuilder.Services.AddTransient<ViewModels.AdminPageViewModel>();
            _ = mauiAppBuilder.Services.AddTransient<ViewModels.ChildSelectionPageViewModel>();
            _ = mauiAppBuilder.Services.AddTransient<ViewModels.EmployeeSelectionPageViewModel>();
            _ = mauiAppBuilder.Services.AddTransient<ViewModels.LoginPageViewModel>();
            _ = mauiAppBuilder.Services.AddTransient<ViewModels.PageHeaderViewModel>();
            _ = mauiAppBuilder.Services.AddTransient<ViewModels.SchoolSelectionPageViewModel>();
            _ = mauiAppBuilder.Services.AddTransient<ViewModels.StateSelectionPageViewModel>();

            return mauiAppBuilder;
        }

        public static MauiAppBuilder RegisterServices(this MauiAppBuilder mauiAppBuilder)
        {
            _ = mauiAppBuilder.Services.AddSingleton<NavigationService>();
            _ = mauiAppBuilder.Services.AddSingleton<IFileHelper, FileHelper>();
            _ = mauiAppBuilder.Services.AddSingleton<IAutoUpdateHelper, AutoUpdateHelper>();
            _ = mauiAppBuilder.Services.AddSingleton(provider =>
            {
                var fileHelper = provider.GetRequiredService<IFileHelper>();
                var path = fileHelper.GetLocalFilePath("GoddardClock.db3");
                return new ClockDatabase(path);
            });
            _ = mauiAppBuilder.Services.AddSingleton<SyncEngineService>();
            _ = mauiAppBuilder.Services.AddSingleton<ISchoolSelectionPageFactory, SchoolSelectionPageFactory>();
            _ = mauiAppBuilder.Services.AddSingleton<IStateSelectionPageFactory, StateSelectionPageFactory>();
            _ = mauiAppBuilder.Services.AddSingleton<ILoginPageFactory, LoginPageFactory>();
            _ = mauiAppBuilder.Services.AddSingleton<IEmployeeSelectionPageFactory, EmployeeSelectionPageFactory>();
            _ = mauiAppBuilder.Services.AddSingleton<IHomePageFactory, HomePageFactory>();
            _ = mauiAppBuilder.Services.AddSingleton<IPreCheckInPageFactory, PreCheckInPageFactory>();
            _ = mauiAppBuilder.Services.AddSingleton<IPinPadPageFactory, PinPadPageFactory>();
            _ = mauiAppBuilder.Services.AddSingleton<IAdminLoginPageFactory, AdminLoginPageFactory>();
            _ = mauiAppBuilder.Services.AddSingleton<IAdminPageFactory, AdminPageFactory>();
            _ = mauiAppBuilder.Services.AddSingleton<IChildSelectionPageFactory, ChildSelectionPageFactory>();

            _ = mauiAppBuilder.Services.AddSingleton<ViewModelLocator>();

            return mauiAppBuilder;
        }
    }
}
