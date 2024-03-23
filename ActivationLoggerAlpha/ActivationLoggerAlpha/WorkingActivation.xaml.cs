using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions; // For input validation
using UraniumUI.Pages;

namespace ActivationLoggerAlpha
{
    public partial class WorkingActivation : UraniumContentPage
    {
        public const string AdifFilePath = "qso_log.adi";
        private System.Timers.Timer dateTimeUpdateTimer;
        private bool adifFileCreated = false;
        public ObservableCollection<QSORecord> QSORecords { get; set; } = new ObservableCollection<QSORecord>();

        public WorkingActivation(string callsign, string parkNumber, string parkName, string gridSquare, string licenseClass)
        {
            InitializeComponent();

            dateTimeUpdateTimer = new System.Timers.Timer(1000);
            dateTimeUpdateTimer.Elapsed += (sender, e) => UpdateDateTime();
            dateTimeUpdateTimer.Start();

            if (!File.Exists(AdifFilePath))
            {
                CreateAdifFile();
                adifFileCreated = true;
            }

            activationInfoLabel.Text = $"Activation in Progress by {callsign} at {parkNumber} {parkName}";
            parkNumberLabel.Text = $"Park Number: {parkNumber}";
            parkNameLabel.Text = $"Park Name: {parkName}";
            callsignLabel.Text = $"Callsign: {callsign}";
            gridSquareLabel.Text = $"Grid Square: {gridSquare}";
            licenseClassLabel.Text = $"License Class: {licenseClass}";

            // Validation event handlers
            frequencyEntry.TextChanged += FrequencyEntry_TextChanged;
            myPowerEntry.TextChanged += MyPowerEntry_TextChanged;

            // Bind the collection to the DataGrid (CollectionView in this context)
            qsosDataGrid.ItemsSource = QSORecords;
        }

        private void CreateAdifFile()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(AdifFilePath))
                {
                    writer.WriteLine("ADIF version: 3.0.9");
                    writer.WriteLine("<EOH>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating ADIF file: {ex.Message}");
            }
        }

        private async void LogQSOButton_Clicked(object sender, EventArgs e)
        {
            // Assuming validation is performed elsewhere
            var qsoRecord = new QSORecord
            {
                HunterCallsign = hunterCallsignEntry.Text,
                Frequency = frequencyEntry.Text,
                Band = bandPicker.SelectedItem.ToString(),
                MyPower = myPowerEntry.Text,
                Mode = modePicker.SelectedItem.ToString(), // Add this line
                DateTime = dateTimeLabel.Text,
                ParkNumber = parkNumberLabel.Text.Replace("Park Number: ", ""),
                Callsign = callsignLabel.Text.Replace("Callsign: ", ""),
                GridSquare = gridSquareLabel.Text.Replace("Grid Square: ", "")
            };

            // Check for duplicates, ignoring the time
            bool isDuplicate = QSORecords.Any(existingRecord =>
                existingRecord.HunterCallsign == qsoRecord.HunterCallsign &&
                existingRecord.Frequency == qsoRecord.Frequency &&
                existingRecord.Band == qsoRecord.Band &&
                existingRecord.MyPower == qsoRecord.MyPower &&
                existingRecord.Mode == qsoRecord.Mode && // Check for Mode as well
                existingRecord.ParkNumber == qsoRecord.ParkNumber &&
                existingRecord.Callsign == qsoRecord.Callsign &&
                existingRecord.GridSquare == qsoRecord.GridSquare);

            if (isDuplicate)
            {
                // Ask the user for confirmation before adding a duplicate entry
                bool answer = await DisplayAlert("Duplicate entry",
                    "This is a Duplicate entry, are you sure you want to log this contact?",
                    "Yes", "Cancel");

                if (!answer)
                {
                    // If the user chooses 'Cancel', exit the method without adding the QSO
                    return;
                }
            }

            // Add the QSO record to the collection
            QSORecords.Add(qsoRecord);

            // Append the new record to the ADIF file
            AppendToAdifFile(qsoRecord);

            // Clear the "hunter callsign" field
            hunterCallsignEntry.Text = "";
        }


        private void AppendToAdifFile(QSORecord qsoRecord)
        {
            try
            {
                using (StreamWriter writer = File.AppendText(AdifFilePath))
                {
                    string adifRecord = $"<QSO_DATE:{qsoRecord.DateTime.Length}>{qsoRecord.DateTime} <CALL:{qsoRecord.HunterCallsign.Length}>{qsoRecord.HunterCallsign} <FREQ:{qsoRecord.Frequency.Length}>{qsoRecord.Frequency} <BAND:{qsoRecord.Band.Length}>{qsoRecord.Band} <POWER:{qsoRecord.MyPower.Length}>{qsoRecord.MyPower} <PARK:{qsoRecord.ParkNumber.Length}>{qsoRecord.ParkNumber} <GRID:{qsoRecord.GridSquare.Length}>{qsoRecord.GridSquare} <ACTIVATOR:{qsoRecord.Callsign.Length}>{qsoRecord.Callsign}";
                    writer.WriteLine(adifRecord);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error appending to ADIF file: {ex.Message}");
            }
        }

        private void UpdateDateTime()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                dateTimeLabel.Text = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC";
            });
        }

        private void PauseButton_Clicked(object sender, EventArgs e)
        {
            dateTimeUpdateTimer.Enabled = !dateTimeUpdateTimer.Enabled;
            pauseButton.BackgroundColor = dateTimeUpdateTimer.Enabled ? Colors.LightGray : Colors.Red;
        }

        private async void OnSaveForLaterClicked(object sender, EventArgs e)
        {
            // Placeholder for save logic
            await DisplayAlert("Info", "Save Activation for Later functionality is not implemented yet.", "OK");
        }




        private async void OnSaveAndCloseClicked(object sender, EventArgs e)
        {
            // Remove invalid characters from callsign, park number, and date-time components
            string callsign = RemoveInvalidCharacters(callsignLabel.Text.Trim());
            string parkNumber = RemoveInvalidCharacters(parkNumberLabel.Text.Trim());
            string dateTime = DateTime.UtcNow.ToString("yyyyMMddHHmmss");

            // Construct the file name without including callsign, park number, and park name
            string adifFileName = $"{dateTime}.adi";

            try
            {
                // Get the directory where the app is running
                string appDirectory = AppContext.BaseDirectory;

                // Combine the directory path with the file name
                string filePath = Path.Combine(appDirectory, adifFileName);

                // Write QSO records to the file
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Write ADIF header
                    writer.WriteLine("ADIF version: 3.0.9");
                    writer.WriteLine("<EOH>");

                    // Write QSO records
                    foreach (var qsoRecord in QSORecords)
                    {
                        string adifRecord = $"<QSO_DATE:{qsoRecord.DateTime.Length}>{qsoRecord.DateTime} " +
                                            $"<CALL:{qsoRecord.HunterCallsign.Length}>{qsoRecord.HunterCallsign} " +
                                            $"<FREQ:{qsoRecord.Frequency.Length}>{qsoRecord.Frequency} " +
                                            $"<BAND:{qsoRecord.Band.Length}>{qsoRecord.Band} " +
                                            $"<POWER:{qsoRecord.MyPower.Length}>{qsoRecord.MyPower} " +
                                            $"<MODE:{qsoRecord.Mode.Length}>{qsoRecord.Mode} " + // Include mode
                                            $"<PARK:{qsoRecord.ParkNumber.Length}>{qsoRecord.ParkNumber} " +
                                            $"<GRID:{qsoRecord.GridSquare.Length}>{qsoRecord.GridSquare} " +
                                            $"<ACTIVATOR:{qsoRecord.Callsign.Length}>{qsoRecord.Callsign}";
                        writer.WriteLine(adifRecord);
                    }

                    await DisplayAlert("Success", $"QSO records saved to {filePath}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save QSO records: {ex.Message}", "OK");
            }
        }



        // Function to remove invalid characters from a string
        private string RemoveInvalidCharacters(string input)
        {
            // Define a regular expression pattern to match invalid characters
            string pattern = "[\\/:*?\"<>|]";

            // Replace invalid characters with an empty string
            return Regex.Replace(input, pattern, "");
        }



        private async void OnRecoverUnsavedActivationClicked(object sender, EventArgs e)
        {
            // Navigate to the WorkingActivation page to recover unsaved activation
            await Navigation.PushAsync(new WorkingActivation("ParkNumber", "ParkName", "Callsign", "GridSquare", "LicenseClass"));
            Console.WriteLine("Recover Unsaved Activation clicked");
        }

        // Validation for Frequency Entry
        private void FrequencyEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Regex.IsMatch(e.NewTextValue, @"^\d{1,3}(\.\d{1,3})?$"))
            {
                ((Entry)sender).BackgroundColor = Colors.Red;
            }
            else
            {
                // Adjusted to set background color to transparent for a valid input
                ((Entry)sender).BackgroundColor = Colors.Transparent;
            }
        }

        private void MyPowerEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Regex.IsMatch(e.NewTextValue, @"^\d{1,4}$"))
            {
                ((Entry)sender).BackgroundColor = Colors.Red;
            }
            else
            {
                // Adjusted to set background color to transparent for a valid input
                ((Entry)sender).BackgroundColor = Colors.Transparent;
            }
        }
    }

    public class QSORecord
    {
        public string HunterCallsign { get; set; }
        public string Frequency { get; set; }
        public string Band { get; set; }
        public string MyPower { get; set; }
        public string Mode { get; set; } // Add this line
        public string DateTime { get; set; }
        public string ParkNumber { get; set; }
        public string Callsign { get; set; }
        public string GridSquare { get; set; }
    }

}