using RealEstateApp.Models;
using RealEstateApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TinyIoC;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RealEstateApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PropertyListPage : ContentPage
    {
        IRepository Repository;
        private Location _location;
        public ObservableCollection<PropertyListItem> PropertiesCollection { get; } = new ObservableCollection<PropertyListItem>();

        public PropertyListPage()
        {
            InitializeComponent();

            Repository = TinyIoCContainer.Current.Resolve<IRepository>();
            LoadProperties();
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadProperties();
        }

        void OnRefresh(object sender, EventArgs e)
        {
            var list = (ListView)sender;
            LoadProperties();
            list.IsRefreshing = false;
        }

        void LoadProperties()
        {
            PropertiesCollection.Clear();

            try
            {
                var properties = Repository.GetProperties();
                var items = new List<PropertyListItem>();       // Instantiating a new List of PropertyListItem.

                foreach (Property property in properties)
                {
                    var item = new PropertyListItem(property);  // Instantiating a new PropertyListItem.
                    if (_location != null)                      // If _locationg is different from null, then calculate the distance in our new PropertyListItem
                    {
                        item.Distance = _location.CalculateDistance((double)property.Latitude, (double)property.Longitude, DistanceUnits.Kilometers);
                    }
                    items.Add(item);
                }
                foreach (var item in items.OrderBy(x => x.Distance))
                {
                    PropertiesCollection.Add(item);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void ItemsListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new PropertyDetailPage(e.Item as PropertyListItem));
        }

        private async void AddProperty_Clicked(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new AddEditPropertyPage());
        }

        private async void SortProperties_Clicked(object sender, EventArgs e)
        {
            _location = await Geolocation.GetLocationAsync();
            LoadProperties();
        }
    }
}