using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Microsoft.Win32;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly YoutubeClient _youtube = new YoutubeClient();
        private StreamManifest _streamManifest;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void UrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            

            try
            {
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var url = UrlTextBox.Text;
            var saveLocation = LocationTextBox.Text;

            // Get video info and streams
            var video = await _youtube.Videos.GetAsync(url);
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(url);
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            if (string.IsNullOrEmpty(saveLocation))
            {
                MessageBox.Show("Please enter a download location.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Display meta data
            MetaDataTextBlock.Text = $"{video.Title}\nBy {video.Author}\nDescription: {video.Description}\nDuration: {video.Duration}\nUploaded On: {video.UploadDate}";

            // Get the highest resolution thumbnail
            var thumbnail = video.Thumbnails.OrderByDescending(t => t.Resolution.Height).First();

            var client = new WebClient();
            var imageBytes = await client.DownloadDataTaskAsync(thumbnail.Url);

            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(imageBytes))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            ThumbnailImage.Source = bitmap;

            var saveFilePath = System.IO.Path.Combine(saveLocation, $"{video.Title}.mp4");

            // Download video
            await _youtube.Videos.Streams.DownloadAsync(streamInfo, saveFilePath);

            MessageBox.Show("Download completed!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void DownloadPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            // doesn't work yet
        }


    }
}
