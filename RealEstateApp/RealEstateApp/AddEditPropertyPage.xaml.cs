using RealEstateApp.Models;
using RealEstateApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using TinyIoC;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddEditPropertyPage : ContentPage
    {
        private IRepository Repository;

        #region PROPERTIES
        public ObservableCollection<Agent> Agents { get; }

        private Property _property;
        public Property Property
        {
            get => _property;
            set
            {
                _property = value;
                if (_property.AgentId != null)
                {
                    SelectedAgent = Agents.FirstOrDefault(x => x.Id == _property?.AgentId);
                }
            }
        }

        private Agent _selectedAgent;

        public Agent SelectedAgent
        {
            get => _selectedAgent;
            set
            {
                if (Property != null)
                {
                    _selectedAgent = value;
                    Property.AgentId = _selectedAgent?.Id;
                }
            }
        }

        public string StatusMessage { get; set; }
        public Color StatusColor { get; set; } = Color.White;
        public bool IsOnline { get; set; }

        #region BATTERY PROPERTIES
        private Color _batteryMessageColor;

        public Color BatteryMessageColor
        {
            get => _batteryMessageColor;
            set { _batteryMessageColor = value; OnPropertyChanged(nameof(BatteryMessageColor)); }
        }

        private string _batteryMessage;

        public string BatteryMessage
        {
            get => _batteryMessage;
            set { _batteryMessage = value; OnPropertyChanged(nameof(BatteryMessage)); }
        }
        #endregion

        #region FLASHLIGHT PROPERTIES
        private CancellationTokenSource _cancellationToken;

        private bool _isShown;

        public bool IsShown
        {
            get => _isShown;
            set { _isShown = value; OnPropertyChanged(nameof(IsShown)); }
        }

        #endregion
        #endregion

        protected override void OnAppearing()
        {
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            BatteryStatus();
            Battery.BatteryInfoChanged += Battery_BatteryInfoChanged;
            Battery.EnergySaverStatusChanged += OnEnergySaverStatusChanged;
        }

        protected override void OnDisappearing()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
            Battery.BatteryInfoChanged -= Battery_BatteryInfoChanged;
            Battery.EnergySaverStatusChanged -= OnEnergySaverStatusChanged;
        }

        public AddEditPropertyPage(Property property = null)
        {
            var profiles = Connectivity.ConnectionProfiles;
            InitializeComponent();

            Repository = TinyIoCContainer.Current.Resolve<IRepository>();
            Agents = new ObservableCollection<Agent>(Repository.GetAgents());

            if (property == null)
            {
                Title = "Add Property";
                Property = new Property();
            }
            else
            {
                Title = "Edit Property";
                Property = property;
            }

            BindingContext = this;

            #region CONNECTIVITY.
            if (!profiles.Contains(ConnectionProfile.WiFi))
            {
                IsOnline = false;
                DisplayAlert("Offline", "You're not connected to the wifi.", "Ok");
            }
            if (profiles.Contains(ConnectionProfile.WiFi))
            {
                IsOnline = true;
            }
            #endregion
        }

        #region CONNECTIVITY.
        void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var profiles = e.ConnectionProfiles;
            if (Connectivity.NetworkAccess == e.NetworkAccess && profiles.Contains(ConnectionProfile.WiFi))
            {
                IsOnline = true;
            }
            else if (!profiles.Contains(ConnectionProfile.WiFi))
            {
                DisplayAlert("Offline", "You're not connected to the wifi.", "Ok");
                IsOnline = false;
            }
        }
        #endregion

        #region BATTERY
        private void BatteryStatus()
        {            
            if (Battery.ChargeLevel < 0.2)
            {
                BatteryMessage = "Battery is low. Please save your changes!";
                if (Battery.State != BatteryState.Charging)
                {
                    BatteryMessageColor = Color.Red;
                }
                else
                {
                    BatteryMessageColor = Color.Yellow;
                }
                if (Battery.EnergySaverStatus == EnergySaverStatus.On)
                {
                    BatteryMessageColor = Color.Green;
                }
            }
            else
            {
                BatteryMessage = null;
            }
        }

        private void Battery_BatteryInfoChanged(object sender, BatteryInfoChangedEventArgs e)
        {
            BatteryStatus();
        }

        private void OnEnergySaverStatusChanged(object sender, EnergySaverStatusChangedEventArgs e)
        {
            BatteryStatus();
        }
        #endregion

        #region FLASHLIGHT
        private async void TurnFlashlightOn_Clicked(object sender, System.EventArgs e)
        {
            IsShown = true;
            await Flashlight.TurnOnAsync();
        }

        private async void TurnFlashlightOff_Clicked(object sender, System.EventArgs e)
        {
            IsShown = false;
            await Flashlight.TurnOffAsync();
        }
        #endregion

        #region COMPASS.
        private async void GetCurrentAspect_Clicked(object sender, System.EventArgs e)
        {
            Navigation.PushModalAsync(new CompassPage(Property));
        }
        #endregion

        private async void SaveProperty_Clicked(object sender, System.EventArgs e)
        {
            if (IsValid() == false)
            {
                // Vibrate assignment 3.5
                Vibration.Vibrate(TimeSpan.FromSeconds(2));
                StatusMessage = "Please fill in all required fields";
                StatusColor = Color.Red;
            }
            else
            {
                Repository.SaveProperty(Property);
                await Navigation.PopToRootAsync();
            }
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(Property.Address)
                || Property.Beds == null
                || Property.Price == null
                || Property.AgentId == null)
                return false;

            return true;
        }

        private async void CancelSave_Clicked(object sender, System.EventArgs e)
        {
            HapticFeedback.Perform(HapticFeedbackType.Click);
            await Navigation.PopToRootAsync();
        }

        #region GEOLOCATION
        CancellationTokenSource cts;
        private async void GetCurrentLocation_Clicked(object sender, EventArgs e)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token);

                if (location != null)
                {
                    Property.Latitude = location.Latitude;
                    Property.Longitude = location.Longitude;
                }

                var addresses = await Geocoding.GetPlacemarksAsync(location);
                var address = addresses.FirstOrDefault();
                if (address != null)
                {
                    Property.Address = $"{address.SubThoroughfare} {address.Thoroughfare}, {address.Locality} {address.AdminArea} {address.PostalCode} {address.CountryName}";
                }
                OnPropertyChanged(nameof(Property));
            }

            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }
        }
        #endregion

        #region GEOCODING.
        private async void GetDistanceFromAddress_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Property.Address))
            {
                await DisplayAlert("Address is missing", "Please enter an address.", "OK");
                return;
            }
            var locations = await Geocoding.GetLocationsAsync(Property.Address);
            var location = locations.FirstOrDefault();
            if (location != null)
            {
                Property.Latitude = location.Latitude;
                Property.Longitude = location.Longitude;
            }
        }
        #endregion
    }
}