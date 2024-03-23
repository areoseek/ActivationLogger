using Microsoft.Maui.Controls;
using System.ComponentModel;

namespace ActivationLoggerAlpha
{
    public partial class ActivationPage : ContentPage
    {
        public ActivationPage()
        {
            InitializeComponent();
            callsignEntry.TextChanged += OnFormUpdated;
            parkNumberEntry.TextChanged += OnFormUpdated;
            gridSquareEntry.TextChanged += OnFormUpdated;
            licenseClassPicker.SelectedIndexChanged += OnFormUpdated;
        }

        private void OnFormUpdated(object sender, EventArgs e)
        {
            // Check if all conditions are met
            confirmButton.IsEnabled = !string.IsNullOrWhiteSpace(callsignEntry.Text)
                                      && !string.IsNullOrWhiteSpace(parkNumberEntry.Text)
                                      && !string.IsNullOrWhiteSpace(gridSquareEntry.Text)
                                      && licenseClassPicker.SelectedIndex >= 0;
        }

        private async void OnConfirmClicked(object sender, EventArgs e)
        {
            // Logic for confirming the activation
            await DisplayAlert("Confirmed", "Activation started successfully.", "OK");
            // Navigate back or close this page
        }

        private async void OnAbortClicked(object sender, EventArgs e)
        {
            // Logic for aborting the action
            await Navigation.PopAsync();
        }
    }
}
