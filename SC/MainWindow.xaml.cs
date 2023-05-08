using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SoundCloudExplode;
using SC.Properties;

namespace SC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Load the last download path from settings
            DestinationTextBox.Text = Settings.Default.LastDownloadPath;
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the track link and download path from the text boxes
            var trackLink = TrackLinkTextBox.Text;
            var downloadPath = DestinationTextBox.Text;

            // Validate the track link and download path
            if (string.IsNullOrWhiteSpace(trackLink) || !Uri.TryCreate(trackLink, UriKind.Absolute, out Uri uri) || !uri.Host.Equals("soundcloud.com"))
            {
                MessageBox.Show("Invalid track link");
                return;
            }

            var soundcloud = new SoundCloudClient();
            var track = await soundcloud.Tracks.GetAsync(trackLink);

            if (string.IsNullOrWhiteSpace(downloadPath))
            {
                MessageBox.Show("Invalid download path");
                return;
            }

            var trackName = string.Join("_", track.Title.Split(System.IO.Path.GetInvalidFileNameChars()));
            downloadPath = System.IO.Path.Combine(downloadPath, $"{trackName}.mp3");

            var progress = new Progress<double>(value =>
            {
                DownloadProgressBar.Value = value;
            });

            await soundcloud.DownloadAsync(track, downloadPath, progress);

            // Download complete, show a message and reset the progress bar
            MessageBox.Show("Download complete!");
            DownloadProgressBar.Value = 0;

            // Save the download path to settings
            Properties.Settings.Default.LastDownloadPath = downloadPath;
            Properties.Settings.Default.Save();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Save the last download path to settings when the application is closed
            Settings.Default.LastDownloadPath = DestinationTextBox.Text;
            Settings.Default.Save();
        }
    }
}
