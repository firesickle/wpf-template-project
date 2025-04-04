using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataToolkit
{
    public partial class SettingsWindow : Window
    {
        private readonly Settings _originalSettings;
        private readonly Settings _workingSettings;
        private const string SettingsFileName = "settings.json";

        public SettingsWindow(Settings settings)
        {
            InitializeComponent();

            _originalSettings = settings;
            _workingSettings = settings.Clone();

            GenerateSettingsForm();
        }

        private void GenerateSettingsForm()
        {
            // Get all properties from Settings class
            PropertyInfo[] properties = typeof(Settings).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                // Skip non-relevant properties (like events)
                if (property.PropertyType.IsSubclassOf(typeof(MulticastDelegate)))
                    continue;

                // Create a container for each property
                Grid propertyGrid = new Grid();
                propertyGrid.Margin = new Thickness(0, 5, 0, 5);
                propertyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                propertyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Label for the property
                TextBlock label = new TextBlock
                {
                    Text = FormatPropertyName(property.Name),
                    Style = (Style)FindResource("SettingsLabelStyle")
                };
                Grid.SetColumn(label, 0);

                // Special handling for WorkingFolder_Path
                if (property.Name == "WorkingFolder_Path")
                {
                    // Create a container for the textbox and browse button
                    Grid folderGrid = new Grid();
                    folderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    folderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                    // Textbox for the folder path
                    TextBox pathTextBox = new TextBox
                    {
                        Style = (Style)FindResource("DarkTextBoxStyle"),
                        Text = (string)property.GetValue(_workingSettings) ?? string.Empty
                    };
                    pathTextBox.TextChanged += (s, e) => property.SetValue(_workingSettings, pathTextBox.Text);
                    Grid.SetColumn(pathTextBox, 0);

                    // Browse button
                    Button browseButton = new Button
                    {
                        Content = "Browse...",
                        Style = (Style)FindResource("BrowseButtonStyle"),
                        Margin = new Thickness(5, 0, 0, 0),
                        Padding = new Thickness(10, 0, 10, 0),
                        Height = 30
                    };
                    browseButton.Click += (s, e) => BrowseFolderPath(pathTextBox);
                    Grid.SetColumn(browseButton, 1);

                    folderGrid.Children.Add(pathTextBox);
                    folderGrid.Children.Add(browseButton);

                    Grid.SetColumn(folderGrid, 1);
                    propertyGrid.Children.Add(label);
                    propertyGrid.Children.Add(folderGrid);
                }
                // Handle boolean properties
                else if (property.PropertyType == typeof(bool))
                {
                    System.Windows.Controls.CheckBox checkBox = new System.Windows.Controls.CheckBox
                    {
                        Style = (Style)FindResource("DarkCheckBoxStyle"),
                        IsChecked = (bool)property.GetValue(_workingSettings),
                        Content = FormatPropertyName(property.Name)
                    };
                    checkBox.Checked += (s, e) => property.SetValue(_workingSettings, true);
                    checkBox.Unchecked += (s, e) => property.SetValue(_workingSettings, false);

                    Grid.SetColumnSpan(checkBox, 2);
                    propertyGrid.Children.Add(checkBox);
                }
                // Default handling for other property types (assuming string for simplicity)
                else
                {
                    TextBox textBox = new TextBox
                    {
                        Style = (Style)FindResource("DarkTextBoxStyle"),
                        Text = property.GetValue(_workingSettings)?.ToString() ?? string.Empty
                    };
                    textBox.TextChanged += (s, e) => property.SetValue(_workingSettings, textBox.Text);

                    Grid.SetColumn(textBox, 1);
                    propertyGrid.Children.Add(label);
                    propertyGrid.Children.Add(textBox);
                }

                SettingsPanel.Children.Add(propertyGrid);
            }
        }

        private void BrowseFolderPath(TextBox pathTextBox)
        {
            // WPF alternative to using FolderBrowserDialog
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Folder",
                CheckFileExists = false,
                FileName = "Select Folder", // This will be ignored
                ValidateNames = false,
                CheckPathExists = true,
                // This forces the dialog to allow folder selection instead of file selection
                InitialDirectory = !string.IsNullOrEmpty(pathTextBox.Text) && Directory.Exists(pathTextBox.Text)
                    ? pathTextBox.Text
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (dialog.ShowDialog() == true)
            {
                // Get the directory path from the selected "file"
                string folderPath = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(folderPath))
                {
                    pathTextBox.Text = folderPath;
                }
            }
        }

        private string FormatPropertyName(string propertyName)
        {
            // Replace underscores with spaces
            string formattedName = propertyName.Replace("_", " ");

            // Insert spaces before capital letters (except the first letter)
            for (int i = 1; i < formattedName.Length; i++)
            {
                if (char.IsUpper(formattedName[i]) && formattedName[i - 1] != ' ')
                {
                    formattedName = formattedName.Insert(i, " ");
                    i++; // Skip the space we just inserted
                }
            }

            return formattedName;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the application directory
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string settingsPath = Path.Combine(appDirectory, SettingsFileName);

                // Save the settings to file
                _workingSettings.SaveSettings(settingsPath);

                // Update the original settings object with new values
                typeof(Settings).GetProperties().ForEach(p =>
                {
                    if (!p.PropertyType.IsSubclassOf(typeof(MulticastDelegate)))
                    {
                        p.SetValue(_originalSettings, p.GetValue(_workingSettings));
                    }
                });

                System.Windows.MessageBox.Show("Settings saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Reset to default values
            _workingSettings.WorkingFolder_Path = "";
            _workingSettings.DebugMode = false;
            _workingSettings.Name = "";

            // Refresh the UI
            SettingsPanel.Children.Clear();
            GenerateSettingsForm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // Extension method for IEnumerable
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            foreach (var item in array)
            {
                action(item);
            }
        }
    }
}