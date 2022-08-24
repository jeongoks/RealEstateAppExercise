using RealEstateApp.Models;
using RealEstateApp.Services;
using System.Linq;
using System.Threading;
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

        private async void Image_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushModalAsync(new ImageListPage(Property));
        }
    }
}