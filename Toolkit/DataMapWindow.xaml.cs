using DataToolkit.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.IO;
using System.Reflection;

namespace DataToolkit
{

    /// <summary>
    /// Interaction logic for DataMapWindow.xaml
    /// </summary>
    public partial class DataMapWindow : Window, INotifyPropertyChanged
    {
        public class InsertionAdorner : Adorner
        {
            public enum InsertionPosition { Top, Bottom }

            private readonly InsertionPosition _position;
            private readonly Pen _pen;

            public InsertionAdorner(UIElement adornedElement, InsertionPosition position)
                : base(adornedElement)
            {
                _position = position;

                // Create pen for drawing the insertion line
                SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(180, 0, 122, 204)); // Semi-transparent blue
                _pen = new Pen(brush, 3);
                _pen.Freeze();

                // Make sure this adorner doesn't interfere with hit testing
                IsHitTestVisible = false;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                // Get the adorned element's size
                Rect adornedRect = new Rect(AdornedElement.RenderSize);

                // Determine where to draw the line
                double y = _position == InsertionPosition.Top ? 0 : adornedRect.Bottom;

                // Add a small inset from the left and right edges
                double leftX = adornedRect.Left + 5;
                double rightX = adornedRect.Right - 5;

                // Draw the line
                drawingContext.DrawLine(_pen, new Point(leftX, y), new Point(rightX, y));
            }
        }

        /// <summary>
        /// Converter for boolean values to colors (used for visual feedback)
        /// </summary>
        public class BooleanToColorConverter : IValueConverter
        {
            public Brush TrueValue { get; set; }
            public Brush FalseValue { get; set; }

            public BooleanToColorConverter()
            {
                // Default values
                TrueValue = new SolidColorBrush(Colors.Green);
                FalseValue = new SolidColorBrush(Colors.Red);
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool boolValue)
                {
                    return boolValue ? TrueValue : FalseValue;
                }
                return FalseValue;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        // Drag and drop tracking
        private Point _startPoint;
        private bool _isDragging;
        private ListViewItem _draggedItem;
        private ListView _originListView;
        private ListView _currentTargetListView;
        private int _insertIndex = -1;

        // Ghost indicator for drop position
        private AdornerLayer _adornerLayer;
        private InsertionAdorner _insertionAdorner;

        // Statistics properties
        public int MappedCount => DataMaps?.Count(m => m.IsMapped) ?? 0;
        public int TotalCount => DataMaps?.Count ?? 0;
        // Add these to the class fields at the top of the DataMapWindow class


        // Collection that holds both input and output columns
        private ObservableCollection<DataMap> _dataMaps;
        // Binding properties
        public ObservableCollection<DataMap> DataMaps
        {
            get { return _dataMaps; }
            set
            {
                _dataMaps = value;
                OnPropertyChanged(nameof(DataMaps));
                OnPropertyChanged(nameof(MappedCount));
                OnPropertyChanged(nameof(TotalCount));
            }
        }

        private string _inputFilePath = string.Empty;
        public string InputFilePath
        {
            get { return _inputFilePath; }
            set
            {
                _inputFilePath = value;
                //txtInputFilePath.Text = value;
                OnPropertyChanged(nameof(InputFilePath));
                OnPropertyChanged(nameof(IsInputFilePathVisible));
            }
        }
        public bool IsInputFilePathVisible
        {
            get { return _inputFilePath != string.Empty; }
        }

        private string _outputFilePath = string.Empty;
        public string OutputFilePath
        {
            get { return _outputFilePath; }
            set
            {
                _outputFilePath = value;
                //txtOutputFilePath.Text = value;
                OnPropertyChanged(nameof(OutputFilePath));
                OnPropertyChanged(nameof(IsOutputFilePathVisible));
            }
        }
        public bool IsOutputFilePathVisible
        {
            get { return _outputFilePath != string.Empty; }
        }

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DataMapWindow()
        {
            InitializeComponent();

            // Initialize with sample data
            InitializeDataSources();

            // Set the DataContext to this window
            this.DataContext = this;
        }

        /// <summary>
        /// Initializes the data sources with sample data.
        /// In production, this would load from a real data source.
        /// </summary>
        private void InitializeDataSources()
        {
            _dataMaps = new ObservableCollection<DataMap>();

            // Add sample columns
            AddSampleColumns();
        }

        /// <summary>
        /// Adds sample columns for demonstration
        /// </summary>
        private void AddSampleColumns()
        {
            // Sample input columns
            DataMaps.Add(new DataMap
            {
                InputColumnName = "CustomerID",
                InputColumnPosition = 0,
                SampleData = new List<string> { "12345", "67890", "54321" }
            });
            DataMaps.Add(new DataMap
            {
                InputColumnName = "FirstName",
                InputColumnPosition = 1,
                SampleData = new List<string> { "John", "Jane", "Bob" }
            });
            DataMaps.Add(new DataMap
            {
                InputColumnName = "LastName",
                InputColumnPosition = 2,
                SampleData = new List<string> { "Smith", "Doe", "Johnson" }
            });
            DataMaps.Add(new DataMap
            {
                InputColumnName = "Email",
                InputColumnPosition = 3,
                SampleData = new List<string> { "john@example.com", "jane@example.com" }
            });
            DataMaps.Add(new DataMap
            {
                InputColumnName = "Phone",
                InputColumnPosition = 4,
                SampleData = new List<string> { "555-1234", "555-5678" }
            });
            DataMaps.Add(new DataMap
            {
                InputColumnName = "Address",
                InputColumnPosition = 5,
                SampleData = new List<string> { "123 Main St", "456 Oak Ave" }
            });
            DataMaps.Add(new DataMap
            {
                InputColumnName = "City",
                InputColumnPosition = 6,
                SampleData = new List<string> { "New York", "Los Angeles" }
            });
            DataMaps.Add(new DataMap
            {
                InputColumnName = "State",
                InputColumnPosition = 7,
                SampleData = new List<string> { "NY", "CA" }
            });
            DataMaps.Add(new DataMap
            {
                InputColumnName = "ZipCode",
                InputColumnPosition = 8,
                SampleData = new List<string> { "10001", "90001" }
            });
            DataMaps.Add(new DataMap
            {
                InputColumnName = "Country",
                InputColumnPosition = 9,
                SampleData = new List<string> { "USA", "Canada" }
            });

            // Sample output columns (not mapped initially)
            DataMaps.Add(new DataMap
            {
                OutputColumnName = "ID",
                OutputColumnPosition = 0
            });
            DataMaps.Add(new DataMap
            {
                OutputColumnName = "Name",
                OutputColumnPosition = 1
            });
            DataMaps.Add(new DataMap
            {
                OutputColumnName = "ContactEmail",
                OutputColumnPosition = 2
            });
            DataMaps.Add(new DataMap
            {
                OutputColumnName = "ContactPhone",
                OutputColumnPosition = 3
            });
            DataMaps.Add(new DataMap
            {
                OutputColumnName = "FullAddress",
                OutputColumnPosition = 4
            });
            DataMaps.Add(new DataMap
            {
                OutputColumnName = "RegionCode",
                OutputColumnPosition = 5
            });
        }

        #region Drag and Drop Event Handlers

        private void InputColumnsList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _originListView = InputColumnsList;
            StartDragOperation(sender, e);
        }

        private void OutputColumnsList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _originListView = OutputColumnsList;
            StartDragOperation(sender, e);
        }

        private void StartDragOperation(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = false;

            var listView = sender as ListView;
            if (listView == null) return;

            // Find the ListViewItem under the mouse
            var element = e.OriginalSource as FrameworkElement;
            while (element != null && !(element is ListViewItem))
                element = VisualTreeHelper.GetParent(element) as FrameworkElement;

            if (element == null) return;

            _draggedItem = element as ListViewItem;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null && !_isDragging)
            {
                Point position = e.GetPosition(null);

                // Check if mouse has moved far enough to start dragging
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;

                    // Get the dragged DataMap
                    var dataMap = _draggedItem.Content as DataMap;
                    if (dataMap == null) return;

                    // Start the drag & drop operation
                    DragDrop.DoDragDrop(_draggedItem, dataMap, DragDropEffects.Move);
                }
            }
        }

        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }

        private void ColumnsList_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(DataMap)))
            {
                _currentTargetListView = sender as ListView;
                e.Effects = DragDropEffects.Move;
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;

                // Change appearance on drag enter
                ListView listView = sender as ListView;
                if (listView != null)
                {
                    Border border = FindVisualParent<Border>(listView);
                    if (border != null)
                    {
                        border.Background = new SolidColorBrush(Color.FromRgb(20, 75, 100)); // Dark blue highlight
                        border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 122, 204)); // Accent blue
                    }
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
                return;
            }
        }

        private void ColumnsList_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(DataMap)))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            var targetListView = sender as ListView;
            if (targetListView == null) return;

            Point position = e.GetPosition(targetListView);

            // Find the item under the cursor or the nearest insertion point
            var result = VisualTreeHelper.HitTest(targetListView, position);
            ListViewItem targetItem = null;

            if (result != null)
            {
                DependencyObject obj = result.VisualHit;
                while (obj != null && !(obj is ListViewItem))
                    obj = VisualTreeHelper.GetParent(obj);

                targetItem = obj as ListViewItem;
            }

            // Determine insertion index based on cursor position
            if (targetItem != null)
            {
                var itemData = targetItem.Content as DataMap;
                if (itemData != null)
                {
                    // Check if we should insert above or below the current item
                    var itemRect = GetItemRect(targetListView, targetItem);

                    bool insertBelow = position.Y > itemRect.Y + itemRect.Height / 2;

                    int itemIndex = targetListView.Items.IndexOf(itemData);
                    _insertIndex = insertBelow ? itemIndex + 1 : itemIndex;

                    // Position the ghost indicator
                    ShowGhostIndicator(targetListView, targetItem, insertBelow);
                }
            }
            else if (targetListView.Items.Count > 0)
            {
                // If no item is under the cursor, insert at the end
                _insertIndex = targetListView.Items.Count;

                // Get the last item
                var lastItem = GetListViewItemAtIndex(targetListView, targetListView.Items.Count - 1);
                if (lastItem != null)
                {
                    ShowGhostIndicator(targetListView, lastItem, true);
                }
            }

            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void ColumnsList_DragLeave(object sender, DragEventArgs e)
        {
            // Hide ghost indicator when leaving the list
            HideGhostIndicator();

            // Restore default appearance
            ListView listView = sender as ListView;
            if (listView != null)
            {
                Border border = FindVisualParent<Border>(listView);
                if (border != null)
                {
                    border.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)); // Default color
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70)); // Default border
                }
            }
        }

        private void InputColumnsList_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, InputColumnsList, true);
        }

        private void OutputColumnsList_Drop(object sender, DragEventArgs e)
        {
            HandleDrop(e, OutputColumnsList, false);
        }

        private void HandleDrop(DragEventArgs e, ListView targetListView, bool isInputList)
        {
            // Hide the ghost indicator
            HideGhostIndicator();

            //ListView listView = sender as ListView;
            //TODO: Make sure this isnt extra effort here
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0]; // Take only the first file

                    if (targetListView == InputColumnsList)
                    {
                        InputFilePath = filePath;
                        // Here you would parse the file and load columns
                        ProcessInputFile(filePath);
                    }
                    else if (targetListView == OutputColumnsList)
                    {
                        OutputFilePath = filePath;
                        // Here you would parse the file and load columns
                        ProcessOutputFile(filePath);
                    }
                }
            }

            // Make sure to restore border appearance
            if (targetListView != null)
            {
                Border border = FindVisualParent<Border>(targetListView);
                if (border != null)
                {
                    border.Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)); // Default color
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70)); // Default border
                }
            }



            if (!e.Data.GetDataPresent(typeof(DataMap)))
                return;

            var draggedData = e.Data.GetData(typeof(DataMap)) as DataMap;
            if (draggedData == null) return;

            // Get the current item under the cursor
            Point position = e.GetPosition(targetListView);
            var result = VisualTreeHelper.HitTest(targetListView, position);
            ListViewItem targetItem = null;

            if (result != null)
            {
                DependencyObject obj = result.VisualHit;
                while (obj != null && !(obj is ListViewItem))
                    obj = VisualTreeHelper.GetParent(obj);

                targetItem = obj as ListViewItem;
            }

            if (targetItem != null)
            {
                var itemData = targetItem.Content as DataMap;
                if (itemData != null)
                {
                    // Check if we're dropping directly on an item
                    var itemRect = GetItemRect(targetListView, targetItem);
                    bool isDropOnItem = position.Y > itemRect.Y + 5 && position.Y < itemRect.Y + itemRect.Height - 5;

                    if (isDropOnItem)
                    {
                        // If dropping on an item, create a mapping between the two columns
                        if (isInputList)
                        {
                            // Map input column to output column (draggedData has output column info)
                            itemData.OutputColumnName = draggedData.OutputColumnName;
                            itemData.OutputColumnPosition = draggedData.OutputColumnPosition;
                            itemData.IsMapped = true;
                        }
                        else
                        {
                            // Map output column to input column (draggedData has input column info)
                            itemData.InputColumnName = draggedData.InputColumnName;
                            itemData.InputColumnPosition = draggedData.InputColumnPosition;
                            itemData.SampleData = new List<string>(draggedData.SampleData ?? new List<string>());
                            itemData.IsMapped = true;
                        }
                    }
                    else
                    {
                        // If dropping between items, adjust the column order
                        int fromIndex = DataMaps.IndexOf(draggedData);
                        int toIndex = DataMaps.IndexOf(itemData);

                        if (fromIndex != toIndex)
                        {
                            // Adjust for insert above/below
                            if (_insertIndex > fromIndex) _insertIndex--;
                            DataMaps.Move(fromIndex, _insertIndex);
                        }
                    }
                }
            }

            // Refresh the UI (property change notifications)
            OnPropertyChanged(nameof(MappedCount));
            OnPropertyChanged(nameof(TotalCount));

            // Reset drag operation variables
            _isDragging = false;
            _draggedItem = null;
            _insertIndex = -1;

            e.Handled = true;
        }

        /// <summary>
        /// Shows the ghost indicator at appropriate position relative to the target item
        /// </summary>
        private void ShowGhostIndicator(ListView listView, ListViewItem targetItem, bool below)
        {
            if (targetItem == null) return;

            // Remove any existing adorner
            HideGhostIndicator();

            // Create adorner layer if needed
            _adornerLayer = AdornerLayer.GetAdornerLayer(listView);
            if (_adornerLayer == null) return;

            // Create the insertion adorner
            _insertionAdorner = new InsertionAdorner(
                targetItem,
                below ? InsertionAdorner.InsertionPosition.Bottom : InsertionAdorner.InsertionPosition.Top);

            // Add the adorner to the layer
            _adornerLayer.Add(_insertionAdorner);
        }

        /// <summary>
        /// Hides the ghost indicator
        /// </summary>
        private void HideGhostIndicator()
        {
            if (_insertionAdorner != null && _adornerLayer != null)
            {
                _adornerLayer.Remove(_insertionAdorner);
                _insertionAdorner = null;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle of a ListView item in the ListView's coordinate space
        /// </summary>
        private Rect GetItemRect(ListView listView, ListViewItem item)
        {
            return item.TransformToAncestor(listView).TransformBounds(new Rect(0, 0, item.ActualWidth, item.ActualHeight));
        }

        /// <summary>
        /// Gets the ListViewItem at the specified index
        /// </summary>
        private ListViewItem GetListViewItemAtIndex(ListView listView, int index)
        {
            if (listView.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                return null;

            if (index < 0 || index >= listView.Items.Count)
                return null;

            return listView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        /// <summary>
        /// Finds a visual child of the specified type
        /// </summary>
        private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is T t)
                    return t;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }

            return null;
        }

        #endregion

        #region Button Click Handlers

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnHelpButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Data Mapping Instructions:\n\n" +
                "1. Drag columns from the Input list to the Output list to map them.\n" +
                "2. You can also drag from Output to Input to create mappings.\n" +
                "3. Dropping an item directly on another will create a mapping between them.\n" +
                "4. Use the arrow buttons to quickly map selected items.\n" +
                "5. Auto-match will attempt to find and map columns with similar names.\n" +
                "6. Use the red down arrow to split a column mapping and move it down.\n" +
                "7. Use the purple gear icon to access additional column settings.\n" +
                "8. Click 'Save Mapping' to finalize your configuration.",
                "Data Mapping Help",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OnSplitColumnClick(object sender, RoutedEventArgs e)
        {
            // Get the clicked button and its parent DataMap
            Button button = sender as Button;
            if (button == null) return;

            // Find the parent ListViewItem
            FrameworkElement parent = button;
            while (parent != null && !(parent is ListViewItem))
            {
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }

            if (parent == null) return;

            // Get the DataMap associated with this ListViewItem
            var dataMap = (parent as ListViewItem).Content as DataMap;
            if (dataMap == null) return;

            // Only proceed if this is a mapped column (has both input and output)
            if (!string.IsNullOrEmpty(dataMap.InputColumnName) && !string.IsNullOrEmpty(dataMap.OutputColumnName))
            {
                // Create separate input and output columns
                var inputColumn = new DataMap
                {
                    InputColumnName = dataMap.InputColumnName,
                    InputColumnPosition = dataMap.InputColumnPosition,
                    SampleData = new List<string>(dataMap.SampleData ?? new List<string>()),
                    IsMapped = false
                };

                var outputColumn = new DataMap
                {
                    OutputColumnName = dataMap.OutputColumnName,
                    OutputColumnPosition = dataMap.OutputColumnPosition,
                    IsMapped = false
                };

                // Get the index to insert at (after the current item)
                int currentIndex = DataMaps.IndexOf(dataMap);

                // Remove the mapped item
                DataMaps.Remove(dataMap);

                // Add the input column at the current position
                DataMaps.Insert(currentIndex, inputColumn);

                // Add the output column after the input column
                DataMaps.Insert(currentIndex + 1, outputColumn);

                // Update the property change notifications
                OnPropertyChanged(nameof(MappedCount));
                OnPropertyChanged(nameof(TotalCount));
            }
            else
            {
                MessageBox.Show("This operation is only available for mapped columns that have both input and output values.",
                    "Cannot Split", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnColumnSettingsClick(object sender, RoutedEventArgs e)
        {
            // Get the clicked button and its parent DataMap
            Button button = sender as Button;
            if (button == null) return;

            // Find the parent ListViewItem
            FrameworkElement parent = button;
            while (parent != null && !(parent is ListViewItem))
            {
                parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
            }

            if (parent == null) return;

            // Get the DataMap associated with this ListViewItem
            var dataMap = (parent as ListViewItem).Content as DataMap;
            if (dataMap == null) return;

            // Placeholder for column settings implementation
            string columnName = !string.IsNullOrEmpty(dataMap.InputColumnName)
                ? dataMap.InputColumnName
                : dataMap.OutputColumnName;

            MessageBox.Show($"Column settings for '{columnName}' will be implemented here.\n\nThis would typically open a form to configure special processing rules, transformations, validations, etc.",
                "Column Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnMapColumnsClick(object sender, RoutedEventArgs e)
        {
            // Map input column to output column
            var inputColumn = InputColumnsList.SelectedItem as DataMap;
            var outputColumn = OutputColumnsList.SelectedItem as DataMap;

            if (inputColumn != null && outputColumn != null)
            {
                // Create a new DataMap that combines both columns
                var newDataMap = new DataMap
                {
                    InputColumnName = inputColumn.InputColumnName,
                    InputColumnPosition = inputColumn.InputColumnPosition,
                    SampleData = new List<string>(inputColumn.SampleData ?? new List<string>()),
                    OutputColumnName = outputColumn.OutputColumnName,
                    OutputColumnPosition = outputColumn.OutputColumnPosition,
                    IsMapped = true,
                    IsSpecialCase = false,
                    IsHardCoded = false
                };

                // Remove the individual columns
                DataMaps.Remove(inputColumn);
                DataMaps.Remove(outputColumn);

                // Add the new combined mapping
                DataMaps.Add(newDataMap);

                // Update the property change notifications
                OnPropertyChanged(nameof(MappedCount));
                OnPropertyChanged(nameof(TotalCount));
            }
            else
            {
                MessageBox.Show("Please select both an input column and an output column to map them.",
                    "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnUnmapColumnsClick(object sender, RoutedEventArgs e)
        {
            // Unmap a mapped column
            var selectedItem = OutputColumnsList.SelectedItem as DataMap ?? InputColumnsList.SelectedItem as DataMap;

            if (selectedItem != null && selectedItem.IsMapped)
            {
                // Create separate input and output columns
                var inputColumn = new DataMap
                {
                    InputColumnName = selectedItem.InputColumnName,
                    InputColumnPosition = selectedItem.InputColumnPosition,
                    SampleData = new List<string>(selectedItem.SampleData ?? new List<string>()),
                    IsMapped = false
                };

                var outputColumn = new DataMap
                {
                    OutputColumnName = selectedItem.OutputColumnName,
                    OutputColumnPosition = selectedItem.OutputColumnPosition,
                    IsMapped = false
                };

                // Remove the mapped item
                DataMaps.Remove(selectedItem);

                // Add the separate columns
                DataMaps.Add(inputColumn);
                DataMaps.Add(outputColumn);

                // Update the property change notifications
                OnPropertyChanged(nameof(MappedCount));
                OnPropertyChanged(nameof(TotalCount));
            }
            else
            {
                MessageBox.Show("Please select a mapped column to unmap.",
                    "No Mapped Column Selected", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnAutoMatchClick(object sender, RoutedEventArgs e)
        {
            // Auto-match columns based on similar names
            var unmappedInputColumns = DataMaps.Where(m => !string.IsNullOrEmpty(m.InputColumnName) &&
                                                          string.IsNullOrEmpty(m.OutputColumnName)).ToList();

            var unmappedOutputColumns = DataMaps.Where(m => !string.IsNullOrEmpty(m.OutputColumnName) &&
                                                           string.IsNullOrEmpty(m.InputColumnName)).ToList();

            int matchCount = 0;

            // Try to match columns with the same or similar names
            foreach (var input in unmappedInputColumns)
            {
                // Try exact match first
                var exactMatch = unmappedOutputColumns.FirstOrDefault(o =>
                    o.OutputColumnName.Equals(input.InputColumnName, StringComparison.OrdinalIgnoreCase));

                if (exactMatch != null)
                {
                    // Create a mapping
                    var newDataMap = new DataMap
                    {
                        InputColumnName = input.InputColumnName,
                        InputColumnPosition = input.InputColumnPosition,
                        SampleData = new List<string>(input.SampleData ?? new List<string>()),
                        OutputColumnName = exactMatch.OutputColumnName,
                        OutputColumnPosition = exactMatch.OutputColumnPosition,
                        IsMapped = true
                    };

                    // Remove the individual columns
                    DataMaps.Remove(input);
                    DataMaps.Remove(exactMatch);
                    unmappedOutputColumns.Remove(exactMatch);

                    // Add the new combined mapping
                    DataMaps.Add(newDataMap);

                    matchCount++;
                    continue;
                }

                // Try partial match (e.g., "FirstName" to "Name", "CustomerID" to "ID")
                foreach (var output in unmappedOutputColumns.ToList())
                {
                    string inputName = input.InputColumnName.ToLower();
                    string outputName = output.OutputColumnName.ToLower();

                    bool isPartialMatch = inputName.Contains(outputName) || outputName.Contains(inputName);

                    if (isPartialMatch)
                    {
                        // Create a mapping
                        var newDataMap = new DataMap
                        {
                            InputColumnName = input.InputColumnName,
                            InputColumnPosition = input.InputColumnPosition,
                            SampleData = new List<string>(input.SampleData ?? new List<string>()),
                            OutputColumnName = output.OutputColumnName,
                            OutputColumnPosition = output.OutputColumnPosition,
                            IsMapped = true
                        };

                        // Remove the individual columns
                        DataMaps.Remove(input);
                        DataMaps.Remove(output);
                        unmappedOutputColumns.Remove(output);

                        // Add the new combined mapping
                        DataMaps.Add(newDataMap);

                        matchCount++;
                        break;
                    }
                }
            }

            // Update the property change notifications
            OnPropertyChanged(nameof(MappedCount));
            OnPropertyChanged(nameof(TotalCount));

            MessageBox.Show($"Auto-matching complete. {matchCount} columns were mapped.",
                "Auto-Match Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnResetButtonClick(object sender, RoutedEventArgs e)
        {
            // Ask for confirmation
            var result = MessageBox.Show(
                "Are you sure you want to reset all mappings? This will undo all your changes.",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // Clear and reinitialize data sources
                DataMaps.Clear();
                AddSampleColumns();
            }
        }

        private void OnSaveMappingButtonClick(object sender, RoutedEventArgs e)
        {
            // In a real app, this would save the mapping to a database or configuration file
            // For demo purposes, we'll just show a message

            int mappedCount = MappedCount;

            MessageBox.Show(
                $"Mapping configuration saved successfully!\n\n" +
                $"Total mapped columns: {mappedCount}\n" +
                $"Total columns: {TotalCount}\n" +
                $"Unmapped input columns: {DataMaps.Count(m => !string.IsNullOrEmpty(m.InputColumnName) && string.IsNullOrEmpty(m.OutputColumnName))}\n" +
                $"Unmapped output columns: {DataMaps.Count(m => !string.IsNullOrEmpty(m.OutputColumnName) && string.IsNullOrEmpty(m.InputColumnName))}",
                "Mapping Saved",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Close the window after saving
            this.Close();
        }

        #endregion

        // Add these methods to process the dropped files
        private void ProcessInputFile(string filePath)
        {
            InputFilePath = filePath;
            //try
            //{
            //    // Clear existing input columns
            //    var outputOnlyColumns = DataMaps.Where(d => !string.IsNullOrEmpty(d.OutputColumnName) &&
            //                                               string.IsNullOrEmpty(d.InputColumnName)).ToList();

            //    DataMaps.Clear();

            //    // Re-add output columns
            //    foreach (var column in outputOnlyColumns)
            //    {
            //        DataMaps.Add(column);
            //    }

            //    // This is where you would actually parse the file
            //    // For demonstration, let's just add some sample columns based on the file extension
            //    string extension = System.IO.Path.GetExtension(filePath).ToLower();

            //    if (extension == ".csv" || extension == ".txt")
            //    {
            //        // Read first few lines to get column headers and sample data
            //        if (File.Exists(filePath))
            //        {
            //            using (var reader = new StreamReader(filePath))
            //            {
            //                // Read header line
            //                string headerLine = reader.ReadLine();
            //                if (headerLine != null)
            //                {
            //                    string[] headers = headerLine.Split(',');

            //                    // Read a few sample data lines
            //                    List<string[]> sampleRows = new List<string[]>();
            //                    for (int i = 0; i < 3; i++)
            //                    {
            //                        string line = reader.ReadLine();
            //                        if (line != null)
            //                        {
            //                            sampleRows.Add(line.Split(','));
            //                        }
            //                        else
            //                        {
            //                            break;
            //                        }
            //                    }

            //                    // Create data maps for each column
            //                    for (int i = 0; i < headers.Length; i++)
            //                    {
            //                        var sampleData = new List<string>();
            //                        foreach (var row in sampleRows)
            //                        {
            //                            if (i < row.Length)
            //                            {
            //                                sampleData.Add(row[i]);
            //                            }
            //                        }

            //                        DataMaps.Add(new DataMap
            //                        {
            //                            InputColumnName = headers[i].Trim(),
            //                            InputColumnPosition = i,
            //                            SampleData = sampleData
            //                        });
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else if (extension == ".xlsx" || extension == ".xls")
            //    {
            //        // Simulated Excel file processing
            //        string[] mockExcelHeaders = new[] { "ID", "Product", "Category", "Price", "Quantity", "Date" };

            //        for (int i = 0; i < mockExcelHeaders.Length; i++)
            //        {
            //            DataMaps.Add(new DataMap
            //            {
            //                InputColumnName = mockExcelHeaders[i],
            //                InputColumnPosition = i,
            //                SampleData = new List<string> { $"Sample {mockExcelHeaders[i]} 1", $"Sample {mockExcelHeaders[i]} 2" }
            //            });
            //        }
            //    }
            //    else
            //    {
            //        // For unknown file types, add generic columns
            //        for (int i = 0; i < 5; i++)
            //        {
            //            DataMaps.Add(new DataMap
            //            {
            //                InputColumnName = $"Column {i + 1}",
            //                InputColumnPosition = i,
            //                SampleData = new List<string> { $"Sample data {i + 1}" }
            //            });
            //        }
            //    }

            //    MessageBox.Show($"Loaded {DataMaps.Count(d => !string.IsNullOrEmpty(d.InputColumnName))} columns from input file.",
            //                   "File Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error processing input file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        private void ProcessOutputFile(string filePath)
        {
            OutputFilePath = filePath;
            //try
            //{
            //    // Clear existing output columns
            //    var inputOnlyColumns = DataMaps.Where(d => !string.IsNullOrEmpty(d.InputColumnName) &&
            //                                              string.IsNullOrEmpty(d.OutputColumnName)).ToList();

            //    DataMaps.Clear();

            //    // Re-add input columns
            //    foreach (var column in inputOnlyColumns)
            //    {
            //        DataMaps.Add(column);
            //    }

            //    // This is where you would actually parse the file
            //    // For demonstration, let's just add some sample columns based on the file extension
            //    string extension = System.IO.Path.GetExtension(filePath).ToLower();

            //    if (extension == ".csv" || extension == ".txt")
            //    {
            //        // Read first line to get column headers
            //        if (File.Exists(filePath))
            //        {
            //            using (var reader = new StreamReader(filePath))
            //            {
            //                string headerLine = reader.ReadLine();
            //                if (headerLine != null)
            //                {
            //                    string[] headers = headerLine.Split(',');

            //                    for (int i = 0; i < headers.Length; i++)
            //                    {
            //                        DataMaps.Add(new DataMap
            //                        {
            //                            OutputColumnName = headers[i].Trim(),
            //                            OutputColumnPosition = i
            //                        });
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else if (extension == ".xlsx" || extension == ".xls")
            //    {
            //        // Simulated Excel file processing for output
            //        string[] mockExcelHeaders = new[] { "UserID", "FullName", "EmailAddress", "TotalSpent", "LastPurchaseDate" };

            //        for (int i = 0; i < mockExcelHeaders.Length; i++)
            //        {
            //            DataMaps.Add(new DataMap
            //            {
            //                OutputColumnName = mockExcelHeaders[i],
            //                OutputColumnPosition = i
            //            });
            //        }
            //    }
            //    else
            //    {
            //        // For unknown file types, add generic columns
            //        for (int i = 0; i < 4; i++)
            //        {
            //            DataMaps.Add(new DataMap
            //            {
            //                OutputColumnName = $"Target Column {i + 1}",
            //                OutputColumnPosition = i
            //            });
            //        }
            //    }

            //    MessageBox.Show($"Loaded {DataMaps.Count(d => !string.IsNullOrEmpty(d.OutputColumnName))} columns from output file.",
            //                   "File Loaded", MessageBoxButton.OK, MessageBoxImage.Information);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Error processing output file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
        }

        private void ddlMapVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}