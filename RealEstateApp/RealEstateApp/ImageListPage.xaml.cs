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
    public partial class ImageListPage : ContentPage
    {
        #region PROPERTIES
        public List<string> Position { get; set; }
        private SensorSpeed sensorSpeed = SensorSpeed.Game;
        private Property _property;
        #endregion

        public ImageListPage()
        {
            InitializeComponent();
        }

        public ImageListPage(Property property)
        {
            InitializeComponent();
            _property = property;
            GetImages();
            BindingContext = this;
        }

        private void GetImages()
        {
            Position = new List<string>();

            foreach (var item in _property.ImageUrls)
            {
                Position.Add(item.ToString()); ;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Accelerometer.IsMonitoring)
                return;

            Accelerometer.ShakeDetected += DetectingShake;
            Accelerometer.Start(sensorSpeed);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (!Accelerometer.IsMonitoring)
                return;

            Accelerometer.ShakeDetected -= DetectingShake;
            Accelerometer.Stop();
        }

        private void DetectingShake(object sender, EventArgs e)
        {
            if (MyCarouselView.Position < Position.Count - 1)
            {
                MyCarouselView.Position = MyCarouselView.Position++;
            }
            else
            {
                MyCarouselView.Position = 0;
            }
        }
    }
}