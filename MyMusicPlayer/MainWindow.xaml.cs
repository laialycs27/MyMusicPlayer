using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Linq;
using MyMusicPlayer.Services;
using System.Threading;
using MyMusicPlayer.Models;

namespace MyMusicPlayer
{
    public partial class MainWindow : Window
    {
        private MediaPlayer mediaPlayer = new MediaPlayer();
        private DispatcherTimer timer = new DispatcherTimer();
        private DispatcherTimer slideshowTimer = new DispatcherTimer();

        private List<MusicTrack> library = new List<MusicTrack>();
        private bool isDragging = false;

        private const string FILE_NAME = "library.json";

        private ItunesService _itunesService = new ItunesService();
        private CancellationTokenSource? _cts;
        private MetadataStore _metadataStore = new MetadataStore();

        private List<string> currentImages = new List<string>();
        private int currentImageIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            // Force default image on startup
            imgArtwork.Source = new BitmapImage(
                new Uri("default.png", UriKind.Relative));

            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;

            slideshowTimer.Interval = TimeSpan.FromSeconds(3);
            slideshowTimer.Tick += SlideshowTimer_Tick;

            LoadLibrary();
        }

        // ---------------------------
        // Slideshow
        // ---------------------------
        private void SlideshowTimer_Tick(object? sender, EventArgs e)
        {
            if (currentImages.Count == 0)
                return;

            currentImageIndex++;

            if (currentImageIndex >= currentImages.Count)
                currentImageIndex = 0;

            imgArtwork.Source = new BitmapImage(
                new Uri(currentImages[currentImageIndex]));
        }

        // ---------------------------
        // Player Controls
        // ---------------------------
        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            timer.Start();
            txtStatus.Text = "Playing";
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
            txtStatus.Text = "Paused";
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            timer.Stop();
            slideshowTimer.Stop();
            sliderProgress.Value = 0;
            txtStatus.Text = "Stopped";
        }

      

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (mediaPlayer.Source != null &&
                mediaPlayer.NaturalDuration.HasTimeSpan &&
                !isDragging)
            {
                sliderProgress.Maximum =
                    mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;

                sliderProgress.Value =
                    mediaPlayer.Position.TotalSeconds;
            }
        }

        private void Slider_DragStarted(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
        }

        private void Slider_DragCompleted(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            mediaPlayer.Position =
                TimeSpan.FromSeconds(sliderProgress.Value);
        }

        // ---------------------------
        // Library
        // ---------------------------
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "MP3 Files|*.mp3";

            if (ofd.ShowDialog() == true)
            {
                foreach (string file in ofd.FileNames)
                {
                    if (!library.Any(x => x.FilePath == file))
                    {
                        library.Add(new MusicTrack
                        {
                            Title = Path.GetFileNameWithoutExtension(file),
                            FilePath = file
                        });
                    }
                }

                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                library.Remove(track);
                UpdateLibraryUI();
                SaveLibrary();
            }
        }

        private void LstLibrary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                PlayTrack(track);
            }
        }

        // ---------------------------
        // Play + Metadata
        // ---------------------------
        private async void PlayTrack(MusicTrack track)
        {
            slideshowTimer.Stop();
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            mediaPlayer.Open(new Uri(track.FilePath));
            mediaPlayer.Play();
            timer.Start();

            txtCurrentSong.Text = track.Title;
            txtStatus.Text = "Playing";
            txtFilePath.Text = $"Path: {track.FilePath}";
            txtArtist.Text = "Artist: Loading...";
            txtAlbum.Text = "Album: Loading...";

            imgArtwork.Source = new BitmapImage(
                new Uri("default.png", UriKind.Relative));

            var cached = _metadataStore.Get(track.FilePath);

            if (cached != null)
            {
                DisplayMetadata(cached);
                return;
            }

            try
            {
                var result = await _itunesService.SearchOneAsync(
                    track.Title,
                    _cts.Token);

                if (result != null)
                {
                    _metadataStore.SaveMetadata(track.FilePath, result);
                    DisplayMetadata(result);
                }
                else
                {
                    txtArtist.Text = "Artist: Not found";
                    txtAlbum.Text = "Album: Not found";
                }
            }
            catch { }
        }

        private void DisplayMetadata(ItunesTrackInfo info)
        {
            txtCurrentSong.Text = info.TrackName;
            txtArtist.Text = $"Artist: {info.ArtistName}";
            txtAlbum.Text = $"Album: {info.AlbumName}";

            slideshowTimer.Stop();
            currentImages.Clear();

            if (info.CustomImages != null && info.CustomImages.Count > 0)
            {
                currentImages = info.CustomImages;
                currentImageIndex = 0;

                imgArtwork.Source = new BitmapImage(
                    new Uri(currentImages[0]));

                slideshowTimer.Start();
            }
            else if (!string.IsNullOrEmpty(info.ArtworkUrl))
            {
                imgArtwork.Source = new BitmapImage(
                    new Uri(info.ArtworkUrl));
            }
            else
            {
                imgArtwork.Source = new BitmapImage(
                    new Uri("default.png", UriKind.Relative));
            }
        }

        // ---------------------------
        // Save / Load
        // ---------------------------
        private void UpdateLibraryUI()
        {
            lstLibrary.ItemsSource = null;
            lstLibrary.ItemsSource = library;
        }

        private void SaveLibrary()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(library, options);
            File.WriteAllText(FILE_NAME, json);
        }

        private void LoadLibrary()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);
                library = JsonSerializer.Deserialize<List<MusicTrack>>(json)
                          ?? new List<MusicTrack>();

                UpdateLibraryUI();
            }
        }

        // ---------------------------
        // Edit Window
        // ---------------------------
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstLibrary.SelectedItem is MusicTrack track)
            {
                var metadata = _metadataStore.Get(track.FilePath);

                if (metadata == null)
                {
                    MessageBox.Show("No metadata found yet.");
                    return;
                }

                EditSongWindow editWindow = new EditSongWindow(metadata);
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    _metadataStore.SaveMetadata(track.FilePath, metadata);
                    DisplayMetadata(metadata);
                }
            }
        }

        // ---------------------------
        // Settings
        // ---------------------------
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsWin = new Settings();
            settingsWin.OnScanCompleted += SettingsWin_OnScanCompleted;
            settingsWin.ShowDialog();
        }

        private void SettingsWin_OnScanCompleted(List<MusicTrack> tracks)
        {
            foreach (var track in tracks)
            {
                if (!library.Any(x => x.FilePath == track.FilePath))
                {
                    library.Add(track);
                }
            }

            UpdateLibraryUI();
            SaveLibrary();
        }
    }
}
