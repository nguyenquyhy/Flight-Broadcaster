﻿using System;

namespace FlightBroadcaster.SimConnectFSX
{
    public class FlightStatusUpdatedEventArgs : EventArgs
    {
        public FlightStatus FlightStatus { get; private set; }

        public FlightStatusUpdatedEventArgs(FlightStatus flightStatus)
        {
            FlightStatus = flightStatus;
        }
    }

    public class FlightStatus
    {
        public double SimTime { get; set; }
        public int? LocalTime { get; set; }
        public int? ZuluTime { get; set; }
        public long? AbsoluteTime { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double AltitudeAboveGround { get; set; }

        public double Heading { get; set; }
        public double TrueHeading { get; set; }

        public double GroundSpeed { get; set; }
        public double IndicatedAirSpeed { get; set; }
        public double VerticalSpeed { get; set; }

        public double FuelTotalQuantity { get; set; }

        public double Pitch { get; set; }
        public double Bank { get; set; }

        public bool IsOnGround { get; set; }
        public bool StallWarning { get; set; }
        public bool OverspeedWarning { get; set; }

        public bool IsAutopilotOn { get; set; }

        public string ScreenshotUrl { get; set; }
    }
}
