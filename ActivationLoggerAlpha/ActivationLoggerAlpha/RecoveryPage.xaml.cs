using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using UraniumUI.Pages;

namespace ActivationLoggerAlpha
{
    public partial class RecoveryPage : UraniumContentPage
    {
        public ObservableCollection<string> adiFiles { get; set; } = new ObservableCollection<string>();

        public RecoveryPage()
        {
            InitializeComponent();

            // Set the BindingContext of the page to itself
            BindingContext = this;

            // Load ADI files from the same directory as the one used for saving
            LoadADIFiles();
        }

        private void LoadADIFiles()
        {
            try
            {
                // Get the directory where the app is running
                string appDirectory = AppContext.BaseDirectory;

                // Get all files with the .adi extension
                string[] files = Directory.GetFiles(appDirectory, "*.adi");

                // Add the file names to the ObservableCollection
                foreach (string file in files)
                {
                    adiFiles.Add(Path.GetFileName(file));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading ADI files: {ex.Message}");
            }
        }


        private void OpenDirectoryButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Get the directory where the app is running
                string appDirectory = AppContext.BaseDirectory;

                // Open the directory containing .adi files
                Process.Start("explorer.exe", appDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening directory: {ex.Message}");
            }
        }

    }
}
