using FlightBroadcaster.Core;
using Microsoft.Extensions.Logging;
using Microsoft.FlightSimulator.SimConnect;
using System;
using System.Threading.Tasks;

namespace FlightBroadcaster.SimConnectFSX
{
    public class FlightConnect
    {
        private const int StatusDelayMilliseconds = 1000;

        public event EventHandler<FlightStatusUpdatedEventArgs> FlightStatusUpdated;
        public event EventHandler Closed;

        // User-defined win32 event
        const int WM_USER_SIMCONNECT = 0x0402;
        private readonly ILogger<FlightConnect> logger;

        public IntPtr Handle { get; private set; }

        private SimConnect simconnect = null;

        public FlightConnect(ILogger<FlightConnect> logger)
        {
            this.logger = logger;
        }

        // Simconnect client will send a win32 message when there is 
        // a packet to process. ReceiveMessage must be called to
        // trigger the events. This model keeps simconnect processing on the main thread.
        public IntPtr HandleSimConnectEvents(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool isHandled)
        {
            isHandled = false;

            switch (message)
            {
                case WM_USER_SIMCONNECT:
                    {
                        if (simconnect != null)
                        {
                            try
                            {
                                this.simconnect.ReceiveMessage();
                            }
                            catch { RecoverFromError(); }

                            isHandled = true;
                        }
                    }
                    break;

                default:
                    break;
            }

            return IntPtr.Zero;
        }

        // Set up the SimConnect event handlers
        public void Initialize(IntPtr Handle)
        {
            simconnect = new SimConnect("Flight Broadcaster", Handle, WM_USER_SIMCONNECT, null, 0);

            // listen to connect and quit msgs
            simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
            simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);

            // listen to exceptions
            simconnect.OnRecvException += simconnect_OnRecvException;

            // catch the assigned object IDs
            simconnect.OnRecvSimobjectDataBytype += simconnect_OnRecvSimobjectDataBytypeAsync;
            RegisterFlightStatusDefinition();
        }

        public void CloseConnection()
        {
            try
            {
                if (simconnect != null)
                {
                    // Dispose serves the same purpose as SimConnect_Close()
                    simconnect.Dispose();
                    simconnect = null;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Cannot unsubscribe events! Error: {ex.Message}");
            }
        }

        private void RegisterFlightStatusDefinition()
        {
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "SIMULATION RATE",
                "number",
                SIMCONNECT_DATATYPE.INT32,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);

            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "PLANE LATITUDE",
                "Degrees",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "PLANE LONGITUDE",
                "Degrees",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "PLANE ALTITUDE",
                "Feet",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "PLANE ALT ABOVE GROUND",
                "Feet",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "PLANE PITCH DEGREES",
                "Degrees",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "PLANE BANK DEGREES",
                "Degrees",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "PLANE HEADING DEGREES TRUE",
                "Degrees",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "PLANE HEADING DEGREES MAGNETIC",
                "Degrees",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "GROUND ALTITUDE",
                "Meters",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "GROUND VELOCITY",
                "Knots",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "AIRSPEED INDICATED",
                "Knots",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "VERTICAL SPEED",
                "Feet per minute",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);

            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "FUEL TOTAL QUANTITY",
                "Gallons",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);

            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "AMBIENT WIND VELOCITY",
                "Feet per second",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "AMBIENT WIND DIRECTION",
                "Degrees",
                SIMCONNECT_DATATYPE.FLOAT64,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);

            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "SIM ON GROUND",
                "number",
                SIMCONNECT_DATATYPE.INT32,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "STALL WARNING",
                "number",
                SIMCONNECT_DATATYPE.INT32,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);
            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "OVERSPEED WARNING",
                "number",
                SIMCONNECT_DATATYPE.INT32,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);

            simconnect.AddToDataDefinition(DEFINITIONS.FlightStatus,
                "AUTOPILOT MASTER",
                "number",
                SIMCONNECT_DATATYPE.INT32,
                0.0f,
                SimConnect.SIMCONNECT_UNUSED);

            // IMPORTANT: register it with the simconnect managed wrapper marshaller
            // if you skip this step, you will only receive a uint in the .dwData field.
            simconnect.RegisterDataDefineStruct<FlightStatusStruct>(DEFINITIONS.FlightStatus);
        }

        private async void simconnect_OnRecvSimobjectDataBytypeAsync(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            // Must be general SimObject information
            switch (data.dwRequestID)
            {
                case (uint)DATA_REQUESTS.FLIGHT_STATUS:
                    {
                        var flightStatus = data.dwData[0] as FlightStatusStruct?;

                        if (flightStatus.HasValue)
                        {
                            FlightStatusUpdated?.Invoke(this, new FlightStatusUpdatedEventArgs(
                                new FlightStatus
                                {
                                    //SimTime = flightStatus.Value.SimTime,
                                    //SimRate = flightStatus.Value.SimRate,
                                    Latitude = flightStatus.Value.Latitude,
                                    Longitude = flightStatus.Value.Longitude,
                                    Altitude = flightStatus.Value.Altitude,
                                    AltitudeAboveGround = flightStatus.Value.AltitudeAboveGround,
                                    Pitch = flightStatus.Value.Pitch,
                                    Bank = flightStatus.Value.Bank,
                                    Heading = flightStatus.Value.MagneticHeading,
                                    TrueHeading = flightStatus.Value.TrueHeading,
                                    GroundSpeed = flightStatus.Value.GroundSpeed,
                                    IndicatedAirSpeed = flightStatus.Value.IndicatedAirSpeed,
                                    VerticalSpeed = flightStatus.Value.VerticalSpeed,
                                    FuelTotalQuantity = flightStatus.Value.FuelTotalQuantity,
                                    IsOnGround = flightStatus.Value.IsOnGround == 1,
                                    StallWarning = flightStatus.Value.StallWarning == 1,
                                    OverspeedWarning = flightStatus.Value.OverspeedWarning == 1,
                                    IsAutopilotOn = flightStatus.Value.IsAutopilotOn == 1
                                }));
                        }
                        else
                        {
                            // Cast failed
                            logger.LogError("Cannot cast to FlightStatusStruct!");
                        }

                        await Task.Delay(StatusDelayMilliseconds);
                        simconnect?.RequestDataOnSimObjectType(DATA_REQUESTS.FLIGHT_STATUS, DEFINITIONS.FlightStatus, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
                    }
                    break;
            }
        }

        void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            logger.LogInformation("Connected to Flight Simulator");
            simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.FLIGHT_STATUS, DEFINITIONS.FlightStatus, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
        }

        // The case where the user closes Flight Simulator
        void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            logger.LogInformation("Flight Simulator has exited");
            Closed?.Invoke(this, new EventArgs());
            CloseConnection();
        }

        void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            logger.LogError("Exception received: {0}", data.dwException);
        }

        private void RecoverFromError()
        {
            string errorMessage;
            //Disconnect();

            //bool wasSuccess = Connect(out errorMessage);

            //// Start monitoring the user's SimObject. This will continuously monitor information
            //// about the user's Stations attached to their SimObject.
            //if (wasSuccess)
            //{
            //    StartMonitoring();
            //}
        }
    }
}
