using RealEstateApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CompassPage : ContentPage
    {
        #region PROPERTIES
        private string _currentAspect;

        public string CurrentAspect
        {
            get => _currentAspect;
            set { _currentAspect = value; OnPropertyChanged(nameof(CurrentAspect)); }
        }

        private double _currentHeading;

        public double CurrentHeading
        {
            get => _currentHeading;
            set { _currentHeading = value; OnPropertyChanged(nameof(CurrentHeading)); }
        }

        private double _rotationAngle;

        public double RotationAngle
        {
            get => _rotationAngle;
            set { _rotationAngle = value; OnPropertyChanged(nameof(RotationAngle)); }
        }
        private Property _property;

        #endregion

        public CompassPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public CompassPage(Property property)
        {
            InitializeComponent();
            BindingContext = this;
            _property = property;
        }

        protected override void OnAppearing()
        {
            Compass.ReadingChanged += ReadingCompassChanges;

            if (Compass.IsMonitoring == false)
            {
                Compass.Start(SensorSpeed.UI, true);
            }
        }

        protected override void OnDisappearing()
        {
            Compass.ReadingChanged -= ReadingCompassChanges;
            if (Compass.IsMonitoring)
            {
                Compass.Stop();
            }
        }

        public void ReadingCompassChanges(object sender, CompassChangedEventArgs e)
        {

            CurrentHeading = e.Reading.HeadingMagneticNorth;        // This is the angle in portrait mode.
            RotationAngle = CurrentHeading * -1;                    // For it to always point north, we * the heading with -1.
            // Dividing CurrentHeading with 90 before we round that number closest to the number to zero
            // Then we multiply that value with 90 to retrieve the closest degree.
            var closestDegree = Math.Round(CurrentHeading / 90d, MidpointRounding.AwayFromZero) * 90;       

            switch (closestDegree)
            {
                case 360:
                    CurrentAspect = "North";
                    break;
                case 90:
                    CurrentAspect = "East";
                    break;
                case 180:
                    CurrentAspect = "South";
                    break;
                case 270:
                    CurrentAspect = "West";
                    break;
                default:
                    break;
            }
        }

        private async void SaveAspect_Clicked(object sender, EventArgs e)
        {
            _property.Aspect = CurrentAspect;
            await Navigation.PopModalAsync();
        }

        private async void Return_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}