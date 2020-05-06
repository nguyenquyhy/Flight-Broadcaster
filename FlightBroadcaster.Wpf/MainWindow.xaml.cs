using FlightBroadcaster.SimConnectFSX;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FlightBroadcaster.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int DefaultUdpPort = 49002;
        private readonly MainViewModel viewModel;
        private readonly ILogger<MainWindow> logger;
        private UdpClient client = null;
        private bool isReady = false;

        public MainWindow(MainViewModel viewModel, FlightConnect flightConnect, ILogger<MainWindow> logger)
        {
            InitializeComponent();

            DataContext = viewModel;
            flightConnect.FlightStatusUpdated += FlightConnect_FlightStatusUpdated;
            this.viewModel = viewModel;
            this.logger = logger;
        }

        private async void FlightConnect_FlightStatusUpdated(object sender, FlightStatusUpdatedEventArgs e)
        {
            viewModel.FlightStatus = e.FlightStatus;

            if (isReady)
            {
                try
                {
                    var gpsData = Encoding.UTF8.GetBytes($"XGPSFS2020,{e.FlightStatus.Longitude},{e.FlightStatus.Latitude},{e.FlightStatus.Altitude},{e.FlightStatus.TrueHeading},{e.FlightStatus.GroundSpeed}");
                    var statusData = Encoding.UTF8.GetBytes($"XATTFS2020,{e.FlightStatus.TrueHeading},{-e.FlightStatus.Pitch},{-e.FlightStatus.Bank}");
                    await client?.SendAsync(gpsData, gpsData.Length);
                    await client?.SendAsync(statusData, statusData.Length);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Cannot send flight status!");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            client = new UdpClient(new IPEndPoint(IPAddress.Any, DefaultUdpPort))
            {
                EnableBroadcast = true
            };
            client.Connect(new IPEndPoint(IPAddress.Broadcast, DefaultUdpPort));
            isReady = true;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false;
            viewModel.IsCompact = !viewModel.IsCompact;

            if (viewModel.IsCompact)
            {
                App.Current.MainWindow.WindowStyle = WindowStyle.None;
                App.Current.MainWindow.Topmost = true;
                Height = 24;
                Width = 230;
            }
            else
            {
                App.Current.MainWindow.WindowStyle = WindowStyle.SingleBorderWindow;
                App.Current.MainWindow.Topmost = false;
                Height = 220;
                Width = 300;
            }
            button.IsEnabled = true;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
