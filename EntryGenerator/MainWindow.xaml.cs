using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using EntryGenerator.ShellClasses;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;

namespace EntryGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Dictionary<FileSystemInfo, string[]> _selectedCars = new Dictionary<FileSystemInfo, string[]>();

        public MainWindow()
        {
            InitializeComponent();
            AssettoCorsaLocation.Visibility = Visibility.Hidden;
            FileTreeView.Visibility = Visibility.Hidden;
            EntryListPanel.Visibility = Visibility.Hidden;
        }

        //private void InitializeFileSystemObjects()
        //{
        //    var drives = DriveInfo.GetDrives();
        //    DriveInfo.GetDrives().ToList().ForEach(drive =>
        //    {
        //        FileSystemObjectInfo fileSystemObject = new FileSystemObjectInfo(drive);
        //        fileSystemObject.BeforeExplore += FileSystemObject_BeforeExplore;
        //        fileSystemObject.AfterExplore += FileSystemObject_AfterExplore;
        //        FileTreeView.Items.Add(fileSystemObject);
        //    });
        //}

        private void FileSystemObject_AfterExplore(object sender, EventArgs e)
        {
            Cursor = Cursors.Arrow;
        }

        private void FileSystemObject_BeforeExplore(object sender, EventArgs e)
        {
            Cursor = Cursors.Wait;
        }

        public void FileSystemObjectInfo_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is FileSystemObjectInfo car)) return;

            DirectoryInfo di = new DirectoryInfo(car.FileSystemInfo.FullName).Parent;
            if (di == null || !di.Name.Equals("cars", StringComparison.CurrentCultureIgnoreCase)) return;

            if (car.IsChecked)
            {
                FileSystemObjectInfo fsoInfo = new FileSystemObjectInfo(new DirectoryInfo(
                    Path.Combine(car.FileSystemInfo.FullName, "skins\\")
                ));

                _selectedCars.Add(car.FileSystemInfo,
                    new FileSystemObjectInfo(
                            new DirectoryInfo(
                                Path.Combine(car.FileSystemInfo.FullName, "skins\\")
                            )
                        ) {IsExpanded = true}
                        .Children.Where(
                            x => !x.FileSystemInfo.Name.Contains(" ")
                        ).Select(
                            x => x.FileSystemInfo.Name
                        ).ToArray());
            }
            else
            {
                _selectedCars.Remove(car.FileSystemInfo);
            }

            EntryList_OnTextChanged(car, null);
        }

        private void SelectAssettoCorsaLocation_OnClick(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = @"Select Assetto Corsa Installation Location";
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;

                DialogResult result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK) return;

                try
                {
                    FileTreeView.Items.Clear();

                    FileSystemObjectInfo fsoInfo = new FileSystemObjectInfo(
                        new DirectoryInfo(
                            Path.Combine(dialog.SelectedPath, "content\\cars\\")
                        )
                    ) {IsExpanded = true};

                    fsoInfo.BeforeExplore += FileSystemObject_BeforeExplore;
                    fsoInfo.AfterExplore += FileSystemObject_AfterExplore;
                    fsoInfo.CheckedChanged += FileSystemObjectInfo_CheckedChanged;

                    foreach (FileSystemObjectInfo carFolder in fsoInfo.Children)
                    {
                        carFolder.Children.Clear();
                    }

                    AssettoCorsaLocation.Text = Path.Combine(dialog.SelectedPath, "content\\cars\\");
                    
                    FileTreeView.Items.Add(fsoInfo);

                    AssettoCorsaLocation.Visibility = Visibility.Visible;
                    FileTreeView.Visibility = Visibility.Visible;
                }
                catch (Exception)
                {
                    AssettoCorsaLocation.Visibility = Visibility.Hidden;
                    FileTreeView.Visibility = Visibility.Hidden;
                    EntryListPanel.Visibility = Visibility.Hidden;


                    MessageBox.Show(@"This doesn't appear to be an Assetto Corsa installation directory.",
                        @"Invalid Directory", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }

        private void EntryList_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_selectedCars.Count > 0)
            {
                // EntryList.Text = string.Join("\n", _selectedCars.Select(x => $"{x.Key.Name}={string.Join(",", x.Value)}"));

                EntryList.Text = string.Empty;

                if (!int.TryParse(CarCount.Text, out int carsToGenerate)) return;

                string[] carNames = _selectedCars.Select(x => x.Key.Name).ToArray();

                Random r = new Random();

                for (int i = 0; i < carsToGenerate; i++)
                {
                    KeyValuePair<FileSystemInfo, string[]> selectedCar = _selectedCars.First(x => x.Key.Name.Equals(carNames[i % carNames.Length], StringComparison.CurrentCultureIgnoreCase));

                    EntryList.Text += $"[CAR_{i}]\nMODEL={selectedCar.Key.Name}\nSKIN={selectedCar.Value[r.Next(selectedCar.Value.Length)]}\nBALLAST=0\nRESTRICTOR=0\n\n";
                }

                EntryListPanel.Visibility = Visibility.Visible;
            }
            else
            {
                if(EntryList != null) EntryList.Text = string.Empty;
                EntryListPanel.Visibility = Visibility.Hidden;
            }
        }

        private void CarCount_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]?[^1-9]");
        }
    }
}
