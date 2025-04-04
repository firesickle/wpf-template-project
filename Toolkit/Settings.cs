using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace DataToolkit
{
    public class Settings : INotifyPropertyChanged
    {
        private string _workingFolderPath = "";
        private bool _debugMode = false;
        private string _name = "";

        public string WorkingFolder_Path
        {
            get => _workingFolderPath;
            set
            {
                if (_workingFolderPath != value)
                {
                    _workingFolderPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool DebugMode
        {
            get => _debugMode;
            set
            {
                if (_debugMode != value)
                {
                    _debugMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Save settings to JSON file
        public void SaveSettings(string filePath)
        {
            try
            {
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
                throw;
            }
        }

        // Load settings from JSON file
        public static Settings LoadSettings(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
            }

            return new Settings();
        }

        // Create a copy of the current settings
        public Settings Clone()
        {
            return new Settings
            {
                WorkingFolder_Path = this.WorkingFolder_Path,
                DebugMode = this.DebugMode,
                Name = this.Name
            };
        }
    }
}