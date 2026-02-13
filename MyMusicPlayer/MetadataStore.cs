using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MyMusicPlayer.Models;

namespace MyMusicPlayer
{
    public class MetadataStore
    {
        private const string FILE_NAME = "metadata.json";

        private Dictionary<string, ItunesTrackInfo> _metadata
            = new Dictionary<string, ItunesTrackInfo>();

        public MetadataStore()
        {
            Load();
        }

        public ItunesTrackInfo? Get(string filePath)
        {
            if (_metadata.ContainsKey(filePath))
                return _metadata[filePath];

            return null;
        }

        public void SaveMetadata(string filePath, ItunesTrackInfo info)
        {
            _metadata[filePath] = info;
            Save();
        }

        private void Save()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(_metadata, options);
            File.WriteAllText(FILE_NAME, json);
        }

        private void Load()
        {
            if (File.Exists(FILE_NAME))
            {
                string json = File.ReadAllText(FILE_NAME);

                _metadata =
                    JsonSerializer.Deserialize<Dictionary<string, ItunesTrackInfo>>(json)
                    ?? new Dictionary<string, ItunesTrackInfo>();
            }
        }
    }
}
