using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls;
using UraniumUI.Pages;

namespace ActivationLoggerAlpha
{
    public class ParkValidator
    {
        public static Dictionary<string, (string ParkName, string GridSquare)> LoadParks()
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(ActivationPage)).Assembly;
            var resourceName = "ActivationLoggerAlpha.Data.parks.csv";
            var parks = new Dictionary<string, (string ParkName, string GridSquare)>(StringComparer.OrdinalIgnoreCase);

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (values.Length >= 8)
                    {
                        var parkId = values[0].Trim('"');
                        var parkName = values[1].Trim('"');
                        var gridSquare = values[7].Trim('"');
                        parks[parkId] = (parkName, gridSquare);
                    }
                }
            }

            return parks;
        }
    }

    public partial class ActivationPage : UraniumContentPage
    {
        private Dictionary<string, (string ParkName, string GridSquare)> _parks;

        public ActivationPage()
        {
            InitializeComponent();
            SetupEventHandlers();
            _parks = ParkValidator.LoadParks();
        }

        private void SetupEventHandlers()
        {
            callsignEntry.TextChanged += OnInputChanged;
            parkNumberEntry.TextChanged += OnInputChanged;
            gridSquareEntry.TextChanged += OnInputChanged;
            licenseClassPicker.SelectedIndexChanged += OnInputChanged;
            populateParkInfoButton.Clicked += PopulateParkClicked;
        }

        private void OnInputChanged(object sender, EventArgs e)
        {
            confirmButton.IsEnabled = !string.IsNullOrWhiteSpace(callsignEntry.Text) &&
                                      !string.IsNullOrWhiteSpace(parkNumberEntry.Text) &&
                                      !string.IsNullOrWhiteSpace(gridSquareEntry.Text) &&
                                      licenseClassPicker.SelectedIndex >= 0;
        }

        private async void PopulateParkClicked(object sender, EventArgs e)
        {
            var parkId = parkNumberEntry.Text.ToUpper();
            if (_parks.TryGetValue(parkId, out var parkInfo))
            {
                parkNameEntry.Text = parkInfo.ParkName;
                gridSquareEntry.Text = parkInfo.GridSquare;
            }
            else
            {
                await DisplayAlert("Error", "The Park number is invalid. Please correct the entry.", "OK");
            }
        }

        private void OnAddAnotherParkClicked(object sender, EventArgs e)
        {
            // Dynamically create and add new park number and name fields
            var newParkNumberEntry = new Entry { Placeholder = "Activation Park Number (e.g us-1402)", HorizontalOptions = LayoutOptions.FillAndExpand };
            var newParkNameEntry = new Entry { Placeholder = "Park Name", IsReadOnly = true };
            var populateButton = new Button { Text = "Populate Park Info", WidthRequest = 250 };

            populateButton.Clicked += async (s, args) =>
            {
                var parkId = newParkNumberEntry.Text.ToUpper();
                if (_parks.TryGetValue(parkId, out var parkInfo))
                {
                    newParkNameEntry.Text = parkInfo.ParkName;
                }
                else
                {
                    await DisplayAlert("Error", "The Park number is invalid. Please correct the entry.", "OK");
                }
            };

            var parkInfoLayout = new HorizontalStackLayout();
            parkInfoLayout.Children.Add(populateButton);
            parkInfoLayout.Children.Add(newParkNumberEntry);

            parksContainer.Children.Add(parkInfoLayout);
            parksContainer.Children.Add(newParkNameEntry);
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            // Extract the data from input fields
            string callsign = callsignEntry.Text;
            string parkNumber = parkNumberEntry.Text;
            string parkName = parkNameEntry.Text; // Assuming this gets populated somewhere in your code
            string gridSquare = gridSquareEntry.Text;
            string licenseClass = licenseClassPicker.SelectedItem.ToString();



            // Navigate to WorkingActivation with the data
            await Navigation.PushAsync(new WorkingActivation(callsign, parkNumber, parkName, gridSquare, licenseClass));
        }



        private async void OnAbortClicked(object sender, EventArgs e)
        {
            // Navigate back on abort
            await Navigation.PopAsync();
            Console.WriteLine("Abort clicked.");
        }
    }
}
