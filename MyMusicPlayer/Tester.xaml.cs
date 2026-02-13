using MyMusicPlayer.Models;
using MyMusicPlayer.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyMusicPlayer
{
    public partial class Tester : Window
    {
        private readonly ItunesService _itunesService = new ItunesService();
        private CancellationTokenSource? _cts;
        private MediaPlayer _player = new MediaPlayer();

        public Tester()
        {
            InitializeComponent();
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PlaySong("C:\\Users\\Win 10\\Downloads\\Post Malone, Swae Lee - Sunflower (Spider-Man_ Into the Spider-Verse) (Official Video).mp3");
        }

        private void PlaySong(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            string songName = Path.GetFileNameWithoutExtension(filePath);

            PlayLocalFile(filePath);
            ClearSongInfo();
            StatusText.Text = "Searching song info...";

            _ = LoadSongInfoAsync(songName, _cts.Token);
        }

        private void PlayLocalFile(string filePath)
        {
            _player.Open(new Uri(filePath));
            _player.Play();
        }

        private async Task LoadSongInfoAsync(string songName, CancellationToken token)
        {
            try
            {
                var info = await _itunesService.SearchOneAsync(songName, token);
                if (info == null)
                {
                    StatusText.Text = "No information found.";
                    return;
                }

                TrackNameText.Text = info.TrackName;
                ArtistNameText.Text = info.ArtistName;
                AlbumNameText.Text = info.AlbumName;
                StatusText.Text = "Info loaded.";

                if (!string.IsNullOrWhiteSpace(info.ArtworkUrl))
                {
                    AlbumImage.Source = new BitmapImage(new Uri(info.ArtworkUrl));
                }
            }
            catch (OperationCanceledException) { }
            catch
            {
                StatusText.Text = "Error loading song info.";
            }
        }

        private void ClearSongInfo()
        {
            TrackNameText.Text = "";
            ArtistNameText.Text = "";
            AlbumNameText.Text = "";
            AlbumImage.Source = null;
        }
    }
}
