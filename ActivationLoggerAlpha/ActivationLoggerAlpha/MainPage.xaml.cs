using Microsoft.Maui.Controls;
using System;
using UraniumUI.Pages; // Ensure UraniumUI is correctly added to your project

namespace ActivationLoggerAlpha
{
    public partial class MainPage : UraniumContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage()); // Ensure SettingsPage is correctly set up
            Console.WriteLine("Settings clicked");
        }

        private async void OnStartNewActivationClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ActivationPage());
            Console.WriteLine("Start New Activation clicked");
        }

        private async void OnResumeExistingActivationClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WorkingActivation("ParkNumber", "ParkName", "Callsign", "GridSquare", "LicenseClass")); // Assuming you have parameters for WorkingActivation
            Console.WriteLine("Resume Existing Activation clicked");
        }

        private void OnViewPreviousActivationsClicked(object sender, EventArgs e)
        {
            Console.WriteLine("View Previous Activations clicked");
        }

        private async void RecoverUnsavedActivation_Clicked(object sender, EventArgs e)
        {
            // Navigate to the RecoveryPage
            await Navigation.PushAsync(new RecoveryPage());
            Console.WriteLine("Recover Unsaved Activation clicked");
        }
    }
}
