using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateApp.Models
{
    public class BarometerMeasurement
    {
        public double Pressure { get; set; }
        public double Altitude { get; set; }
        public string MeasurementLabel { get; set; }
        public double HeightChange { get; set; }

        public string Display => $"{MeasurementLabel}: {Altitude:N2}m";
    }
}
