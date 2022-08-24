using RealEstateApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HeightCalculatorPage : ContentPage
    {
        #region PROPERTIES
        private double _currentAltitude;

        public double CurrentAltitude
        {
            get => _currentAltitude;
            set { _currentAltitude = value; OnPropertyChanged(nameof(CurrentAltitude)); }
        }

        private double _currentPressure;

        public double CurrentPressure
        {
            get => _currentPressure;
            set { _currentPressure = value; OnPropertyChanged(nameof(CurrentPressure)); }
        }

        private double _seaLevelPressure = 1021.6;

        public double SeaLevelPressure
        {
            get => _seaLevelPressure;
            set { _seaLevelPressure = value; OnPropertyChanged(nameof(SeaLevelPressure)); }
        }

        private string _measurementLabel;

        public string MeasurementLabel
        {
            get => _measurementLabel;
            set { _measurementLabel = value; OnPropertyChanged(nameof(MeasurementLabel)); }
        }
        public ObservableCollection<BarometerMeasurement> Measurements { get; set; } = new ObservableCollection<BarometerMeasurement>();
        #endregion
        public HeightCalculatorPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            Barometer.ReadingChanged += OnReadingChanged;

            if (!Barometer.IsMonitoring)
                Barometer.Start(SensorSpeed.UI);

            var location = await Geolocation.GetLocationAsync();
        }

        protected override void OnDisappearing()
        {
            Barometer.ReadingChanged -= OnReadingChanged;

            if (Barometer.IsMonitoring)
                Barometer.Stop();
        }

        private void OnReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            CurrentPressure = e.Reading.PressureInHectopascals;
            CurrentAltitude = CalculateAltitudeInMetres(CurrentPressure, SeaLevelPressure);
        }

        public double CalculateAltitudeInMetres(double currentPressure, double seaLevelPressure)
        {
            return 44307.694 * (1 - Math.Pow(currentPressure / seaLevelPressure, 0.190284));
        }

        private async void SaveMeasurement_Clicked(object sender, EventArgs e)
        {
            var newMeasurement = new BarometerMeasurement
            {
                Pressure = CurrentPressure,
                Altitude = CurrentAltitude,
                MeasurementLabel = MeasurementLabel
            };

            var previousMeasurement = Measurements.OrderBy(x => x.Altitude).LastOrDefault();

            if (previousMeasurement != null)
            {
                newMeasurement.HeightChange = newMeasurement.Altitude - previousMeasurement.Altitude;
            }

            Measurements.Add(newMeasurement);
        }

    }
}