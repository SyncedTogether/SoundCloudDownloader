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
using SoundCloudExplode.Search;
using System.Linq;

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
            string trackLink = TrackLinkTextBox.Text;
            string downloadPath = DestinationTextBox.Text;

            // Validate the track link and download path
            if (string.IsNullOrWhiteSpace(trackLink) || !Uri.TryCreate(trackLink, UriKind.Absolute, out Uri uri) || !uri.Host.Equals("soundcloud.com"))
            {
                MessageBox.Show("Invalid track link");
                return;
            }

            SoundCloudClient soundcloud = new SoundCloudClient();
            SoundCloudExplode.Track.TrackInformation track = await soundcloud.Tracks.GetAsync(trackLink);

            if (string.IsNullOrWhiteSpace(downloadPath))
            {
                MessageBox.Show("Invalid download path");
                return;
            }

            string trackName = string.Join("_", track.Title.Split(System.IO.Path.GetInvalidFileNameChars()));
            downloadPath = System.IO.Path.Combine(downloadPath, $"{trackName}.mp3");



            Progress<double> progress = new Progress<double>(value =>
            {
                DownloadProgressBar.Value = value;
            });


            UpdateMetaDataDisplay(track);
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

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchBox.Text;

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                MessageBox.Show("Search query cannot be empty");
                return;
            }

            SoundCloudClient soundcloud = new SoundCloudClient();
            List<ISearchResult> results = await soundcloud.Search.GetResultsAsync(searchQuery);

            // Only show the first 10 tracks in the results
            SearchResultsBox.ItemsSource = results.OfType<TrackSearchResult>().Take(10);
        }

        private async void DownloadPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            string playlistLink = PlaylistLinkTextBox.Text;

            if (string.IsNullOrWhiteSpace(playlistLink) || !Uri.TryCreate(playlistLink, UriKind.Absolute, out Uri uri) || !uri.Host.Equals("soundcloud.com"))
            {
                MessageBox.Show("Invalid playlist link");
                return;
            }

            string downloadPath = DestinationTextBox.Text;

            if (string.IsNullOrWhiteSpace(downloadPath))
            {
                MessageBox.Show("Invalid download path");
                return;
            }

            SoundCloudClient soundcloud = new SoundCloudClient();
            SoundCloudExplode.Playlist.PlaylistInformation playlist = await soundcloud.Playlists.GetAsync(playlistLink);

            // Download all tracks in the playlist
            foreach (SoundCloudExplode.Track.TrackInformation track in playlist.Tracks)
            {
                UpdateMetaDataDisplay(track);
                string trackName = string.Join("_", track.Title.Split(System.IO.Path.GetInvalidFileNameChars()));
                string trackDownloadPath = System.IO.Path.Combine(downloadPath, $"{trackName}.mp3");
                Progress<double> progress = new Progress<double>(value =>
                {
                    DownloadProgressBar.Value = value;
                });
                await soundcloud.DownloadAsync(track, trackDownloadPath, progress);
            }

            MessageBox.Show("Playlist download complete!");
        }

        private void UpdateMetaDataDisplay(SoundCloudExplode.Track.TrackInformation track)
        {
            MetaDataArtwork.Source = new BitmapImage(track.ArtworkUrl);
            MetaDataTitle.Text = $"Title: {track.Title}";
            MetaDataGenre.Text = $"Genre: {track.Genre}";
            MetaDataDuration.Text = $"Duration: {track.Duration} seconds";
            MetaDataDisplayDate.Text = $"Display Date: {track.DisplayDate}";
            MetaDataCaption.Text = $"Caption: {track.Caption}";
            MetaDataDescription.Text = $"Description: {track.Description}";
        }

        private async void SearchResultsBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SearchResultsBox.SelectedItem is TrackSearchResult selectedTrack)
            {
                string downloadPath = DestinationTextBox.Text;

                if (string.IsNullOrWhiteSpace(downloadPath))
                {
                    MessageBox.Show("Invalid download path");
                    return;
                }

                SoundCloudClient soundcloud = new SoundCloudClient();
                SoundCloudExplode.Track.TrackInformation track = await soundcloud.Tracks.GetAsync(selectedTrack.Uri.ToString());

                string trackName = string.Join("_", track.Title.Split(System.IO.Path.GetInvalidFileNameChars()));
                downloadPath = System.IO.Path.Combine(downloadPath, $"{trackName}.mp3");

                UpdateMetaDataDisplay(track);

                Progress<double> progress = new Progress<double>(value =>
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
        }

    }
}
