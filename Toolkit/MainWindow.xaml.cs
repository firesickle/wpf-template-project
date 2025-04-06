using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace DataToolkit
{
    public partial class MainWindow : Window
    {
        // List to store processed files
        private List<string> processedFiles = new List<string>();

        // Application settings
        private Settings appSettings;
        private const string SettingsFileName = "settings.json";

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string settingsPath = System.IO.Path.Combine(appDirectory, SettingsFileName);

                appSettings = Settings.LoadSettings(settingsPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                // Create default settings if loading fails
                appSettings = new Settings();
            }
        }

        #region Button Click Handlers

        private void OnScaffoldClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Scaffold will be implemented");
            // TODO: Create and show Scaffold form
        }

        private void OnReloadClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Reload will be implemented");
            // TODO: Create and show Reload form or functionality
        }

        private void OnSettingsClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(appSettings);
            settingsWindow.Owner = this;

            bool? result = settingsWindow.ShowDialog();

            if (result == true)
            {
                // Settings were saved, you might want to update UI or reload certain components
                if (appSettings.DebugMode)
                {
                    // Add debug mode specific logic if needed
                }

                // Update window title if name is set
                if (!string.IsNullOrEmpty(appSettings.Name))
                {
                    Title = $"Data Toolkit - {appSettings.Name}";
                }
            }
        }

        private void OnOneOffClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("OneOff will be implemented");
            // TODO: Create and show OneOff form
        }

        private void OnCompareClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Compare will be implemented");
            // TODO: Create and show Compare form
        }

        private void OnAnalyzeClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Analyze will be implemented");
            // TODO: Create and show Analyze form
        }

        private void OnDataMapClick(object sender, RoutedEventArgs e)
        {
            // Create and show the DataMap window
            DataMapWindow dataMapWindow = new DataMapWindow();
            dataMapWindow.Owner = this;
            dataMapWindow.ShowDialog();
        }

        private void OnViewSamplesClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("View Samples will be implemented");
            // TODO: Create and show View Samples form
        }

        private void OnFarmClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Farm will be implemented");
            // TODO: Create and show Farm form
        }

        private void OnScheduleJobsClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ScheduleJobs will be implemented");
            // TODO: Create and show ScheduleJobs form
        }
        #endregion

        #region Expanded Panel Functions

        private void ExpandDragPanel()
        {
            // Start the animation to expand the drag panel
            Storyboard sb = (Storyboard)FindResource("ExpandDragPanel");
            sb.Begin();
        }

        private void CollapseDragPanel()
        {
            // Restore original state
            Storyboard sb = (Storyboard)FindResource("CollapseDragPanel");
            sb.Begin();
        }

        private void OnCollapseViewClick(object sender, RoutedEventArgs e)
        {
            CollapseDragPanel();
        }

        private void OnAnalyzeFilesClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Analyze Files function will be implemented");
            // TODO: Implement Analyze Files functionality
        }

        private void OnEditMetadataClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Edit Metadata function will be implemented");
            // TODO: Implement Edit Metadata functionality
        }

        private void OnBatchProcessClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Batch Process function will be implemented");
            // TODO: Implement Batch Process functionality
        }

        #endregion

        #region Drag & Drop Functionality

        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                ((Border)sender).Background = new SolidColorBrush(Color.FromRgb(20, 75, 100)); // Dark blue highlight
                ((Border)sender).BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204)); // Accent blue
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            ((Border)sender).Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)); // Default dark theme background
            ((Border)sender).BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70)); // Default border
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            ((Border)sender).Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)); // Default dark theme background
            ((Border)sender).BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70)); // Default border

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ProcessDroppedFiles(files);
            }
        }

        private void ProcessDroppedFiles(string[] files)
        {
            if (files == null || files.Length == 0)
                return;

            bool firstFilesAdded = processedFiles.Count == 0;

            foreach (string file in files)
            {
                if (!processedFiles.Contains(file))
                {
                    processedFiles.Add(file);

                    // Add to visual list with file icon and name
                    StackPanel filePanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 2, 0, 2)
                    };

                    // Simple file icon
                    System.Windows.Shapes.Path fileIcon = new System.Windows.Shapes.Path
                    {
                        Data = Geometry.Parse("M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z"),
                        Fill = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                        Width = 16,
                        Height = 16,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(0, 0, 8, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    TextBlock fileNameBlock = new TextBlock
                    {
                        Text = System.IO.Path.GetFileName(file),
                        Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                        ToolTip = file,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    filePanel.Children.Add(fileIcon);
                    filePanel.Children.Add(fileNameBlock);
                    FileListBox.Items.Add(filePanel);
                }
            }

            // If this is the first time files are added, expand the panel
            if (firstFilesAdded && processedFiles.Count > 0)
            {
                ExpandDragPanel();
            }

            // Use working folder from settings if available
            if (!string.IsNullOrEmpty(appSettings.WorkingFolder_Path) && appSettings.DebugMode)
            {
                Title = $"Data Toolkit - {appSettings.Name} - Working in: {appSettings.WorkingFolder_Path}";
            }
        }

        #endregion

        #region Quick Action Button Handlers

        private void OnProcessFilesClick(object sender, RoutedEventArgs e)
        {
            if (processedFiles.Count == 0)
            {
                MessageBox.Show("No files to process. Please drag and drop files first.", "No Files", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Check if working folder is set in settings
            if (!string.IsNullOrEmpty(appSettings.WorkingFolder_Path) && Directory.Exists(appSettings.WorkingFolder_Path))
            {
                // Use working folder from settings
                if (appSettings.DebugMode)
                {
                    string fileList = string.Join(Environment.NewLine, processedFiles);
                    MessageBox.Show($"Debug Mode: Would process {processedFiles.Count} files to {appSettings.WorkingFolder_Path}:{Environment.NewLine}{fileList}",
                                   "Debug Mode", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Normal processing mode
                    string fileList = string.Join(Environment.NewLine, processedFiles);
                    MessageBox.Show($"Processing {processedFiles.Count} files to {appSettings.WorkingFolder_Path}:{Environment.NewLine}{fileList}",
                                   "Processing Files", MessageBoxButton.OK, MessageBoxImage.Information);

                    // TODO: Add actual processing logic here
                }
            }
            else
            {
                // No working folder set or folder doesn't exist
                MessageBox.Show("Working folder not set or doesn't exist. Please check settings.",
                                "Settings Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnClearListClick(object sender, RoutedEventArgs e)
        {
            processedFiles.Clear();
            FileListBox.Items.Clear();

            // Collapse the panel back to normal view
            CollapseDragPanel();
        }

        private void OnBrowseFilesClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Select Files",
                Filter = "All Files (*.*)|*.*"
            };

            // Set initial directory from settings if available
            if (!string.IsNullOrEmpty(appSettings.WorkingFolder_Path) && Directory.Exists(appSettings.WorkingFolder_Path))
            {
                openFileDialog.InitialDirectory = appSettings.WorkingFolder_Path;
            }

            if (openFileDialog.ShowDialog() == true)
            {
                ProcessDroppedFiles(openFileDialog.FileNames);
            }
        }

        #endregion
    }
}