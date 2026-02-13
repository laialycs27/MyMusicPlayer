using Microsoft.Win32;
using System.Windows;
using MyMusicPlayer.Models;
using MyMusicPlayer.ViewModels;

namespace MyMusicPlayer
{
    public partial class EditSongWindow : Window
    {
        private EditSongViewModel _viewModel;

        public EditSongWindow(ItunesTrackInfo info)
        {
            InitializeComponent();
            _viewModel = new EditSongViewModel(info);
            DataContext = _viewModel;
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.png;*.jpeg";

            if (ofd.ShowDialog() == true)
            {
                _viewModel.Images.Add(ofd.FileName);
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            if (ImagesListBox.SelectedItem is string selected)
            {
                _viewModel.Images.Remove(selected);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
            DialogResult = true;
            Close();
        }
    }
}
