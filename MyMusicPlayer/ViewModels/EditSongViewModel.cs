using System.Collections.ObjectModel;
using System.ComponentModel;
using MyMusicPlayer.Models;

namespace MyMusicPlayer.ViewModels
{
    public class EditSongViewModel : INotifyPropertyChanged
    {
        private ItunesTrackInfo _trackInfo;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? TrackName
        {
            get => _trackInfo.TrackName;
            set
            {
                _trackInfo.TrackName = value;
                OnPropertyChanged(nameof(TrackName));
            }
        }

        public string? ArtistName
        {
            get => _trackInfo.ArtistName;
            set
            {
                _trackInfo.ArtistName = value;
                OnPropertyChanged(nameof(ArtistName));
            }
        }

        public string? AlbumName
        {
            get => _trackInfo.AlbumName;
            set
            {
                _trackInfo.AlbumName = value;
                OnPropertyChanged(nameof(AlbumName));
            }
        }

        public ObservableCollection<string> Images { get; set; }

        public EditSongViewModel(ItunesTrackInfo info)
        {
            _trackInfo = info;
            Images = new ObservableCollection<string>(info.CustomImages);
        }

        public void Save()
        {
            _trackInfo.CustomImages = new List<string>(Images);
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
