using FlightBroadcaster.SimConnectFSX;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FlightBroadcaster.Wpf
{
    public enum ConnectionState
    {
        Idle,
        Connecting,
        Connected,
        Failed
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            //if ((storage == null && value != null) || !storage.Equals(value))
            {
                storage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }
            //return false;
        }

        private ConnectionState simConnectionState = ConnectionState.Idle;
        public ConnectionState SimConnectionState { get => simConnectionState; set => SetProperty(ref simConnectionState, value); }

        private FlightStatus flightStatus = null;
        public FlightStatus FlightStatus { get => flightStatus; set => SetProperty(ref flightStatus, value); }

        private bool isCompact = false;
        public bool IsCompact { get => isCompact; set => SetProperty(ref isCompact, value); }
    }
}
