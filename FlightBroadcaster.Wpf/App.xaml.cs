﻿using FlightBroadcaster.SimConnectFSX;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace FlightBroadcaster.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ServiceProvider ServiceProvider { get; private set; }

        private MainWindow mainWindow = null;
        private IntPtr Handle;

        public IConfigurationRoot Configuration { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Loaded += MainWindow_Loaded;
            mainWindow.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.File("flightbroadcaster.log").CreateLogger();

            services.AddOptions();
            //services.Configure<AppSettings>((appSettings) =>
            //{
            //    Configuration.GetSection("AppSettings").Bind(appSettings);
            //});
            services.AddLogging(configure =>
            {
                configure
                    .AddDebug()
                    .AddSerilog();
            });

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<FlightConnect>();

            services.AddTransient(typeof(MainWindow));
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize SimConnect
            var simConnect = ServiceProvider.GetService<FlightConnect>();
            if (simConnect != null)
            {
                simConnect.Closed += SimConnect_Closed;

                // Create an event handle for the WPF window to listen for SimConnect events
                Handle = new WindowInteropHelper(sender as Window).Handle; // Get handle of main WPF Window
                var HandleSource = HwndSource.FromHwnd(Handle); // Get source of handle in order to add event handlers to it
                HandleSource.AddHook(simConnect.HandleSimConnectEvents);
                var viewModel = ServiceProvider.GetService<MainViewModel>();

                await InitializeSimConnectAsync(simConnect, viewModel).ConfigureAwait(true);
            }
        }

        private async Task InitializeSimConnectAsync(FlightConnect simConnect, MainViewModel viewModel)
        {
            while (true)
            {
                try
                {
                    viewModel.SimConnectionState = ConnectionState.Connecting;
                    simConnect.Initialize(Handle);
                    viewModel.SimConnectionState = ConnectionState.Connected;
                    break;
                }
                catch (COMException)
                {
                    viewModel.SimConnectionState = ConnectionState.Failed;
                    await Task.Delay(5000).ConfigureAwait(true);
                }
            }
        }

        private async void SimConnect_Closed(object sender, EventArgs e)
        {
            var simConnect = ServiceProvider.GetService<FlightConnect>();
            var viewModel = ServiceProvider.GetService<MainViewModel>();
            viewModel.SimConnectionState = ConnectionState.Idle;

            await InitializeSimConnectAsync(simConnect, viewModel).ConfigureAwait(true);
        }
    }
}
