using RealEstateApp.Models;
using RealEstateApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TinyIoC;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PropertyDetailPage : ContentPage
    {
        public PropertyDetailPage(PropertyListItem propertyListItem)
        {
            InitializeComponent();

            Property = propertyListItem.Property;

            IRepository Repository = TinyIoCContainer.Current.Resolve<IRepository>();
            Agent = Repository.GetAgents().FirstOrDefault(x => x.Id == Property.AgentId);

            BindingContext = this;
        }

        public Agent Agent { get; set; }

        public Property Property { get; set; }

        #region TEXT-TO-SPEECH PROPERTIES.

        private CancellationTokenSource _cancellationToken;
        private bool _isSpeaking;

        public bool IsSpeaking
        {
            get => _isSpeaking;
            set { _isSpeaking = value; OnPropertyChanged(nameof(IsSpeaking)); }
        }

        private float _selectedVolume = 1;

        public float SelectedVolume
        {
            get => _selectedVolume;
            set { _selectedVolume = value; OnPropertyChanged(nameof(SelectedVolume)); }
        }

        private float _selectedPitch = 1;

        public float SelectedPitch
        {
            get => _selectedPitch;
            set { _selectedPitch = value; OnPropertyChanged(nameof(SelectedPitch)); }
        }

        #endregion

        private async void EditProperty_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new AddEditPropertyPage(Property));
        }

        #region TEXT-TO-SPEECH
        private async void SpeakButton_Clicked(object sender, System.EventArgs e)
        {
            _cancellationToken = new CancellationTokenSource();
            IsSpeaking = true;
            var speechOptions = new SpeechOptions
            {
                Volume = this.SelectedVolume,
                Pitch = this.SelectedPitch
            };
            await TextToSpeech.SpeakAsync(Property.Description, speechOptions, _cancellationToken.Token);
            IsSpeaking = false;
        }

        private void CancelSpeechButton_Clicked(object sender, System.EventArgs e)
        {
            if (_cancellationToken?.IsCancellationRequested ?? true)
                return;
            _cancellationToken.Cancel();
        }
        #endregion

        #region IMAGE.

        private async void Image_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new ImageListPage(Property));
        }
        #endregion

        #region PHONE, EMAIL, SMS.
        private async void Phone_Tapped(object sender, System.EventArgs e)
        {
            string actionDisplay = await DisplayActionSheet("Call or Message?", "Cancel", null, "Call", "Message");
            switch (actionDisplay)
            {
                case "Call":
                    MakePhoneCall();
                    break;
                case "Message":
                    await SendSMS();
                    break;
                default:
                    break;
            }
        }

        public void MakePhoneCall()
        {
            try
            {
                PhoneDialer.Open(Property.Vendor.Phone);
            }
            catch (ArgumentNullException anEx)
            {
                throw new ArgumentException("Phone number wasn't found", anEx);
            }
            catch (FeatureNotSupportedException ex)
            {
                throw new FeatureNotSupportedException("Feature isn't support on this device: ", ex);
            }
        }

        public async Task SendSMS()
        {
            try
            {
                var message = new SmsMessage($"Hej {Property.Vendor.FirstName}, angående {Property.Address}", new[] { Property.Vendor.Phone });
                await Sms.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException ex)
            {
                throw new FeatureNotSupportedException("Feature isn't support on this device: ", ex);
            }
            catch (Exception ex)
            {
                // Other error has occurred.
            }
        }

        private async void Email_Tapped(object sender, System.EventArgs e)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var attachmentFilePath = Path.Combine(folder, "property.txt");
            File.WriteAllText(attachmentFilePath, $"{Property.Address}");
            try
            {
                var message = new EmailMessage
                {
                    Subject = $"About property: {Property.Address}",
                    Body = $"Hello {Property.Vendor.FullName},\n there's some questions about this property {Property.Address} and its price {Property.Price}",
                    To = new List<string>() { Property.Vendor.Email },
                    Attachments = new List<EmailAttachment>()
                    {
                        new EmailAttachment(attachmentFilePath)
                    }
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException fbsEx)
            {
                throw new FeatureNotSupportedException("Feature not supported on this device: ", fbsEx);
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region MAPS
        private async void MapMarkedAlt_Clicked(object sender, EventArgs e)
        {
            var location = new Location((double)Property.Latitude, (double)Property.Longitude);
            try
            {
                await Map.OpenAsync(location);
            }
            catch (Exception ex)
            {
                throw new Exception("No map available", ex);
            }
        }

        private async void MapDirections_Clicked(object sender, EventArgs e)
        {
            var location = new Location((double)Property.Latitude, (double)Property.Longitude);
            var options = new MapLaunchOptions
            {
                NavigationMode = NavigationMode.Driving
            };

            await Map.OpenAsync(location, options);
        }
        #endregion

        #region OPEN BROWSER N LAUNCHER
        public async void BrowserLink_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                var options = new BrowserLaunchOptions
                {
                    LaunchMode = BrowserLaunchMode.SystemPreferred, //BrowserLaunchMode.External,
                    TitleMode = BrowserTitleMode.Default,
                    PreferredToolbarColor = Color.DeepSkyBlue,
                    PreferredControlColor = Color.Black
                };
                await Browser.OpenAsync(Property.NeighbourhoodUrl, options);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async void ShowPDF_Clicked(object sender, System.EventArgs e)
        {
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(Property.ContractFilePath)
            });
        }
        #endregion


    }
}